using Microsoft.Extensions.DependencyInjection;
using StargateAPI.Business.Data;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace StargateAPI.Tests;

public class ApiRuleAndLoggingTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;
    private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);

    public ApiRuleAndLoggingTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
    }

#region Rule-Based Tests
    // A Person who has not had an astronaut assignment will not have Astronaut records.
    [Fact]
    public async Task CreatePerson_DoesNotCreateAstronautRecords()
    {
        using var client = _factory.CreateClient();
        const string personName = "NoDutyYet";
        var createResponse = await client.PostAsync("/Person", new StringContent($"\"{personName}\"", Encoding.UTF8, "application/json"));
        createResponse.EnsureSuccessStatusCode();

        await using var scope = _factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<StargateContext>();
        var person = db.People.Single(x => x.Name == personName);
        Assert.False(db.AstronautDetails.Any(x => x.PersonId == person.Id));
        Assert.False(db.AstronautDuties.Any(x => x.PersonId == person.Id));
    }

    // A Person is uniquely identified by their Name.
    [Fact]
    public async Task CreatePerson_DuplicateName_ReturnsBadRequest()
    {
        using var client = _factory.CreateClient();
        var personName = $"SameName-{Guid.NewGuid():N}";
        var first = await client.PostAsync("/Person", new StringContent($"\"{personName}\"", Encoding.UTF8, "application/json"));
        first.EnsureSuccessStatusCode();

        var second = await client.PostAsync("/Person", new StringContent($"\"{personName}\"", Encoding.UTF8, "application/json"));
        Assert.Equal(HttpStatusCode.BadRequest, second.StatusCode);

        var body = await ReadJsonAsync<BaseResponseContract>(second);
        Assert.NotNull(body);
        Assert.False(body!.Success);
    }

    // A Person is classified as 'Retired' when a Duty Title is 'RETIRED'. 
    // A Person's Career End Date is one day before the Retired Duty Start Date.
    [Fact]
    public async Task CreateAstronautDuty_FirstRetiredDuty_SetsCareerEndDateDayBeforeStart()
    {
        using var client = _factory.CreateClient();
        var personName = $"RetireNow-{Guid.NewGuid():N}";
        await CreatePersonAsync(client, personName);

        var startDate = new DateOnly(2025, 1, 10);
        var retiredDutyRequest = new
        {
            Name = personName,
            Rank = "CPT",
            DutyTitle = "RETIRED",
            DutyStartDate = startDate
        };
        var createDutyResponse = await client.PostAsJsonAsync("/AstronautDuty", retiredDutyRequest);
        createDutyResponse.EnsureSuccessStatusCode();

        await using var scope = _factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<StargateContext>();
        var person = db.People.Single(x => x.Name == personName);
        var detail = db.AstronautDetails.Single(x => x.PersonId == person.Id);
        Assert.Equal("RETIRED", detail.CurrentDutyTitle);
        Assert.Equal(startDate.AddDays(-1), detail.CareerEndDate);
    }

    // A Person will only ever hold one current Astronaut Duty Title, Start Date, and Rank at a time.
    [Fact]
    public async Task CreateAstronautDuty_DuplicateDutyTitleAndStartDate_ReturnsBadRequest()
    {
        using var client = _factory.CreateClient();
        var personName = $"DutyDup-{Guid.NewGuid():N}";
        await CreatePersonAsync(client, personName);

        var startDate = new DateOnly(2026, 2, 10);
        var request = new
        {
            Name = personName,
            Rank = "LT",
            DutyTitle = "Engineer",
            DutyStartDate = startDate
        };

        var first = await client.PostAsJsonAsync("/AstronautDuty", request);
        first.EnsureSuccessStatusCode();

        var second = await client.PostAsJsonAsync("/AstronautDuty", request);
        Assert.Equal(HttpStatusCode.BadRequest, second.StatusCode);
    }

    // A Person's Current Duty will not have a Duty End Date. 
    // A Person's Previous Duty End Date is set to the day before the New Astronaut Duty Start Date when a new Astronaut Duty is received for a Person.
    [Fact]
    public async Task CreateAstronautDuty_NewDuty_ClosesPreviousAndKeepsCurrentOpen()
    {
        using var client = _factory.CreateClient();
        var personName = $"DutyDates-{Guid.NewGuid():N}";
        await CreatePersonAsync(client, personName);

        var firstStart = new DateOnly(2024, 3, 1);
        var secondStart = new DateOnly(2024, 5, 15);

        var firstDuty = await client.PostAsJsonAsync("/AstronautDuty", new
        {
            Name = personName,
            Rank = "LT",
            DutyTitle = "Pilot",
            DutyStartDate = firstStart
        });
        firstDuty.EnsureSuccessStatusCode();

        var secondDuty = await client.PostAsJsonAsync("/AstronautDuty", new
        {
            Name = personName,
            Rank = "CPT",
            DutyTitle = "Commander",
            DutyStartDate = secondStart
        });
        secondDuty.EnsureSuccessStatusCode();

        await using var scope = _factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<StargateContext>();
        var person = db.People.Single(x => x.Name == personName);
        var duties = db.AstronautDuties.Where(x => x.PersonId == person.Id).OrderBy(x => x.DutyStartDate).ToList();
        Assert.Equal(2, duties.Count);
        Assert.Equal(secondStart.AddDays(-1), duties[0].DutyEndDate);
        Assert.Null(duties[1].DutyEndDate);
    }
#endregion

#region Coverage-Based Tests
    // Verifies duty chronology validation rejects same-day or earlier duty start dates.
    [Fact]
    public async Task CreateAstronautDuty_EarlierOrSameStartDate_ReturnsBadRequest()
    {
        using var client = _factory.CreateClient();
        var personName = $"Chronology-{Guid.NewGuid():N}";
        await CreatePersonAsync(client, personName);

        var firstStart = new DateOnly(2025, 8, 10);
        var firstDuty = await client.PostAsJsonAsync("/AstronautDuty", new
        {
            Name = personName,
            Rank = "LT",
            DutyTitle = "Pilot",
            DutyStartDate = firstStart
        });
        firstDuty.EnsureSuccessStatusCode();

        var sameDate = await client.PostAsJsonAsync("/AstronautDuty", new
        {
            Name = personName,
            Rank = "CPT",
            DutyTitle = "Commander",
            DutyStartDate = firstStart
        });
        Assert.Equal(HttpStatusCode.BadRequest, sameDate.StatusCode);

        var earlierDate = await client.PostAsJsonAsync("/AstronautDuty", new
        {
            Name = personName,
            Rank = "MAJ",
            DutyTitle = "Director",
            DutyStartDate = firstStart.AddDays(-1)
        });
        Assert.Equal(HttpStatusCode.BadRequest, earlierDate.StatusCode);
    }

    // Verifies request validation rejects missing required astronaut duty fields.
    [Theory]
    [InlineData("", "CPT", "Pilot")]
    [InlineData("TestPerson", "", "Pilot")]
    [InlineData("TestPerson", "CPT", "")]
    public async Task CreateAstronautDuty_RequiredFieldsMissing_ReturnsBadRequest(string name, string rank, string dutyTitle)
    {
        using var client = _factory.CreateClient();
        var seededName = $"FieldCheck-{Guid.NewGuid():N}";
        await CreatePersonAsync(client, seededName);

        var payload = new
        {
            Name = string.IsNullOrWhiteSpace(name) ? name : seededName,
            Rank = rank,
            DutyTitle = dutyTitle,
            DutyStartDate = new DateOnly(2025, 1, 1)
        };

        var response = await client.PostAsJsonAsync("/AstronautDuty", payload);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // Verifies astronaut duty creation fails when the person does not exist.
    [Fact]
    public async Task CreateAstronautDuty_PersonNotFound_ReturnsBadRequest()
    {
        using var client = _factory.CreateClient();
        var response = await client.PostAsJsonAsync("/AstronautDuty", new
        {
            Name = $"Missing-{Guid.NewGuid():N}",
            Rank = "CPT",
            DutyTitle = "Pilot",
            DutyStartDate = new DateOnly(2026, 1, 1)
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await ReadJsonAsync<BaseResponseContract>(response);
        Assert.NotNull(body);
        Assert.False(body!.Success);
    }

    // Verifies astronaut duty lookup by person name uses normalized (trimmed) input.
    [Fact]
    public async Task CreateAstronautDuty_NameWithWhitespace_ResolvesExistingPerson()
    {
        using var client = _factory.CreateClient();
        var personName = $"DutyWhitespace-{Guid.NewGuid():N}";
        await CreatePersonAsync(client, personName);

        var response = await client.PostAsJsonAsync("/AstronautDuty", new
        {
            Name = $"  {personName}  ",
            Rank = "LT",
            DutyTitle = "Navigator",
            DutyStartDate = new DateOnly(2026, 4, 1)
        });

        response.EnsureSuccessStatusCode();
    }

    // Verifies duties lookup for unknown person returns a successful empty contract response.
    [Fact]
    public async Task GetAstronautDutiesByName_NotFound_ReturnsSuccessWithNullPersonAndEmptyDuties()
    {
        using var client = _factory.CreateClient();
        var response = await client.GetAsync($"/AstronautDuty/Unknown-{Guid.NewGuid():N}");
        response.EnsureSuccessStatusCode();

        var body = await ReadJsonAsync<GetAstronautDutiesByNameContract>(response);
        Assert.NotNull(body);
        Assert.True(body!.Success);
        Assert.Null(body.Person);
        Assert.NotNull(body.AstronautDuties);
        Assert.Empty(body.AstronautDuties!);
    }

    // Verifies person lookup without assignments returns null career date fields.
    [Fact]
    public async Task GetPersonByName_NoAssignments_ReturnsPersonWithoutCareerDates()
    {
        using var client = _factory.CreateClient();
        var personName = $"NoAssignment-{Guid.NewGuid():N}";
        await CreatePersonAsync(client, personName);

        var response = await client.GetAsync($"/Person/{personName}");
        response.EnsureSuccessStatusCode();

        var body = await ReadJsonAsync<GetPersonByNameContract>(response);
        Assert.NotNull(body);
        Assert.True(body!.Success);
        Assert.NotNull(body.Person);
        Assert.Equal(personName, body.Person!.Name);
        Assert.Null(body.Person.CareerStartDate);
        Assert.Null(body.Person.CareerEndDate);
    }

    // Verifies person name input is trimmed on write and uniqueness still applies to the normalized value.
    [Fact]
    public async Task CreatePerson_TrimmedName_IsStoredTrimmedAndStillUnique()
    {
        using var client = _factory.CreateClient();
        var baseName = $"Trimmed-{Guid.NewGuid():N}";
        var create = await client.PostAsync("/Person", new StringContent($"\"  {baseName}  \"", Encoding.UTF8, "application/json"));
        create.EnsureSuccessStatusCode();

        var duplicate = await client.PostAsync("/Person", new StringContent($"\"{baseName}\"", Encoding.UTF8, "application/json"));
        Assert.Equal(HttpStatusCode.BadRequest, duplicate.StatusCode);

        await using var scope = _factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<StargateContext>();
        Assert.True(db.People.Any(x => x.Name == baseName));
    }

    // Verifies person updates work without requiring astronaut records.
    [Fact]
    public async Task UpdatePersonByName_UpdatesPersonAndKeepsAstronautRecordsEmpty()
    {
        using var client = _factory.CreateClient();
        var originalName = $"UpdateMe-{Guid.NewGuid():N}";
        var updatedName = $"Updated-{Guid.NewGuid():N}";
        await CreatePersonAsync(client, originalName);

        var updateResponse = await client.PutAsJsonAsync($"/Person/{originalName}", new
        {
            Name = $"  {updatedName}  "
        });
        updateResponse.EnsureSuccessStatusCode();

        var body = await ReadJsonAsync<BaseResponseContract>(updateResponse);
        Assert.NotNull(body);
        Assert.True(body!.Success);

        await using var scope = _factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<StargateContext>();
        var person = db.People.Single(x => x.Name == updatedName);
        Assert.Equal(updatedName, person.Name);
        Assert.False(db.AstronautDetails.Any(x => x.PersonId == person.Id));
        Assert.False(db.AstronautDuties.Any(x => x.PersonId == person.Id));
    }

    // Verifies update-only behavior returns bad request for missing person.
    [Fact]
    public async Task UpdatePersonByName_PersonNotFound_ReturnsBadRequest()
    {
        using var client = _factory.CreateClient();
        var response = await client.PutAsJsonAsync($"/Person/Missing-{Guid.NewGuid():N}", new
        {
            Name = "UpdatedName"
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await ReadJsonAsync<BaseResponseContract>(response);
        Assert.NotNull(body);
        Assert.False(body!.Success);
    }

    // Verifies updating to an existing name is rejected to preserve unique identity by Name.
    [Fact]
    public async Task UpdatePersonByName_TargetNameAlreadyExists_ReturnsBadRequest()
    {
        using var client = _factory.CreateClient();
        var existingName = $"Existing-{Guid.NewGuid():N}";
        var toUpdateName = $"ToUpdate-{Guid.NewGuid():N}";
        await CreatePersonAsync(client, existingName);
        await CreatePersonAsync(client, toUpdateName);

        var response = await client.PutAsJsonAsync($"/Person/{toUpdateName}", new
        {
            Name = existingName
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await ReadJsonAsync<BaseResponseContract>(response);
        Assert.NotNull(body);
        Assert.False(body!.Success);
    }

    // Verifies PUT validation rejects empty or whitespace name payloads.
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task UpdatePersonByName_NameMissingOrWhitespace_ReturnsBadRequest(string updatedName)
    {
        using var client = _factory.CreateClient();
        var originalName = $"NameCheck-{Guid.NewGuid():N}";
        await CreatePersonAsync(client, originalName);

        var response = await client.PutAsJsonAsync($"/Person/{originalName}", new
        {
            Name = updatedName
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await ReadJsonAsync<BaseResponseContract>(response);
        Assert.NotNull(body);
        Assert.False(body!.Success);
    }

    // Verifies process logging stores both success and error entries in UTC.
    [Fact]
    public async Task LogsPersistedInUtc_ForSuccessAndException()
    {
        using var client = _factory.CreateClient();

        var okResponse = await client.GetAsync("/Person");
        okResponse.EnsureSuccessStatusCode();

        var badResponse = await client.PostAsJsonAsync("/AstronautDuty", new
        {
            Name = "NotARealPerson",
            Rank = "CPT",
            DutyTitle = "Pilot",
            DutyStartDate = new DateOnly(2026, 1, 1)
        });
        Assert.Equal(HttpStatusCode.BadRequest, badResponse.StatusCode);

        var badBodyText = await badResponse.Content.ReadAsStringAsync();
        var badBody = JsonSerializer.Deserialize<BaseResponseContract>(badBodyText, _jsonOptions);
        Assert.NotNull(badBody);
        Assert.False(badBody!.Success);

        await using var scope = _factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<StargateContext>();
        var logs = db.ProcessLogs.OrderByDescending(x => x.Id).Take(5).ToList();
        Assert.NotEmpty(logs);
        Assert.Contains(logs, x => x.Level == "Information");
        Assert.Contains(logs, x => x.Level == "Error");
        Assert.All(logs, x => Assert.Equal(TimeSpan.Zero, x.OccurredAtUtc.Offset));
    }

    // Verifies non-exception framework 400 responses are logged as Warning instead of Information.
    [Fact]
    public async Task LogsWarning_ForFrameworkGeneratedBadRequest()
    {
        using var client = _factory.CreateClient();

        using var content = new StringContent("{\"unexpected\":\"shape\"}", Encoding.UTF8, "application/json");
        var response = await client.PostAsync("/Person", content);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        await using var scope = _factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<StargateContext>();
        var path = "/Person";
        var warningLog = db.ProcessLogs
            .OrderByDescending(x => x.Id)
            .FirstOrDefault(x => x.Path == path && x.Method == "POST" && x.StatusCode == (int)HttpStatusCode.BadRequest);

        Assert.NotNull(warningLog);
        Assert.Equal("Warning", warningLog!.Level);
    }
#endregion

#region Helper Methods
    private static async Task CreatePersonAsync(HttpClient client, string personName)
    {
        var response = await client.PostAsync("/Person", new StringContent($"\"{personName}\"", Encoding.UTF8, "application/json"));
        response.EnsureSuccessStatusCode();
    }

    private async Task<T?> ReadJsonAsync<T>(HttpResponseMessage response)
    {
        var text = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<T>(text, _jsonOptions);
    }
#endregion

#region Contract Classes
    private class BaseResponseContract
    {
        public bool Success { get; set; }
    }

    private sealed class GetAstronautDutiesByNameContract : BaseResponseContract
    {
        public PersonContract? Person { get; set; }
        public List<AstronautDutyContract>? AstronautDuties { get; set; }
    }

    private sealed class GetPersonByNameContract : BaseResponseContract
    {
        public PersonContract? Person { get; set; }
    }

    private sealed class PersonContract
    {
        public string Name { get; set; } = string.Empty;
        public DateOnly? CareerStartDate { get; set; }
        public DateOnly? CareerEndDate { get; set; }
    }

    private sealed class AstronautDutyContract
    {
        public int Id { get; set; }
    }
}
#endregion