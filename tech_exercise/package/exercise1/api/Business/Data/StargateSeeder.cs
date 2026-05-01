using Microsoft.EntityFrameworkCore;

namespace StargateAPI.Business.Data
{
    public static class StargateSeeder
    {
        public static async Task SeedAsync(IServiceProvider services, CancellationToken cancellationToken = default)
        {
            await using var scope = services.CreateAsyncScope();
            var context = scope.ServiceProvider.GetRequiredService<StargateContext>();

            await context.Database.MigrateAsync(cancellationToken);

            //Only run seed if the database is empty
            if (await context.People.AnyAsync(cancellationToken))
            {
                return;
            }

            var peopleSeed = new[]
            {
                new Person { Name = "Noah Bennett" },
                new Person { Name = "Emma Reyes" },
                new Person { Name = "Mason Clarke" },
                new Person { Name = "Olivia Hart" },
                new Person { Name = "Ethan Brooks" },
                new Person { Name = "Amelia Stone" },
                new Person { Name = "Logan Wright" },
                new Person { Name = "Sophia Chen" },
                new Person { Name = "Jackson Cole" },
                new Person { Name = "Isabella Cruz" },
                new Person { Name = "Aiden Walker" },
                new Person { Name = "Mia Foster" },
                new Person { Name = "Henry Powell" },
                new Person { Name = "Charlotte Diaz" },
                new Person { Name = "Lucas Kim" }
            };

            await context.People.AddRangeAsync(peopleSeed, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);

            var peopleByName = peopleSeed.ToDictionary(person => person.Name, person => person);

            var detailSeed = new[]
            {
                new AstronautDetail
                {
                    PersonId = peopleByName["Amelia Stone"].Id,
                    CurrentRank = "CPT",
                    CurrentDutyTitle = "Commander",
                    CareerStartDate = new DateOnly(2012, 1, 10)
                },
                new AstronautDetail
                {
                    PersonId = peopleByName["Logan Wright"].Id,
                    CurrentRank = "MAJ",
                    CurrentDutyTitle = "Pilot",
                    CareerStartDate = new DateOnly(2014, 2, 14)
                },
                new AstronautDetail
                {
                    PersonId = peopleByName["Sophia Chen"].Id,
                    CurrentRank = "CPT",
                    CurrentDutyTitle = "Mission Specialist",
                    CareerStartDate = new DateOnly(2020, 3, 1)
                },
                new AstronautDetail
                {
                    PersonId = peopleByName["Jackson Cole"].Id,
                    CurrentRank = "LTC",
                    CurrentDutyTitle = "Flight Engineer",
                    CareerStartDate = new DateOnly(2011, 4, 20)
                },
                new AstronautDetail
                {
                    PersonId = peopleByName["Isabella Cruz"].Id,
                    CurrentRank = "CPT",
                    CurrentDutyTitle = "Payload Specialist",
                    CareerStartDate = new DateOnly(2021, 6, 5)
                },
                new AstronautDetail
                {
                    PersonId = peopleByName["Aiden Walker"].Id,
                    CurrentRank = "MAJ",
                    CurrentDutyTitle = "Mission Specialist",
                    CareerStartDate = new DateOnly(2016, 8, 12)
                },
                new AstronautDetail
                {
                    PersonId = peopleByName["Mia Foster"].Id,
                    CurrentRank = "CPT",
                    CurrentDutyTitle = "Pilot",
                    CareerStartDate = new DateOnly(2022, 1, 15)
                },
                new AstronautDetail
                {
                    PersonId = peopleByName["Henry Powell"].Id,
                    CurrentRank = "COL",
                    CurrentDutyTitle = "RETIRED",
                    CareerStartDate = new DateOnly(2010, 2, 1),
                    CareerEndDate = new DateOnly(2022, 3, 31)
                },
                new AstronautDetail
                {
                    PersonId = peopleByName["Charlotte Diaz"].Id,
                    CurrentRank = "LTC",
                    CurrentDutyTitle = "RETIRED",
                    CareerStartDate = new DateOnly(2011, 5, 10),
                    CareerEndDate = new DateOnly(2023, 8, 14)
                },
                new AstronautDetail
                {
                    PersonId = peopleByName["Lucas Kim"].Id,
                    CurrentRank = "MAJ",
                    CurrentDutyTitle = "RETIRED",
                    CareerStartDate = new DateOnly(2012, 9, 1),
                    CareerEndDate = new DateOnly(2024, 1, 19)
                }
            };
            await context.AstronautDetails.AddRangeAsync(detailSeed, cancellationToken);

            var dutySeed = new[]
            {
                new AstronautDuty
                {
                    PersonId = peopleByName["Amelia Stone"].Id,
                    Rank = "1LT",
                    DutyTitle = "Payload Specialist",
                    DutyStartDate = new DateOnly(2012, 1, 10),
                    DutyEndDate = new DateOnly(2014, 12, 31)
                },
                new AstronautDuty
                {
                    PersonId = peopleByName["Amelia Stone"].Id,
                    Rank = "CPT",
                    DutyTitle = "Mission Specialist",
                    DutyStartDate = new DateOnly(2015, 1, 1),
                    DutyEndDate = new DateOnly(2017, 12, 31)
                },
                new AstronautDuty
                {
                    PersonId = peopleByName["Amelia Stone"].Id,
                    Rank = "MAJ",
                    DutyTitle = "Pilot",
                    DutyStartDate = new DateOnly(2018, 1, 1),
                    DutyEndDate = new DateOnly(2020, 12, 31)
                },
                new AstronautDuty
                {
                    PersonId = peopleByName["Amelia Stone"].Id,
                    Rank = "CPT",
                    DutyTitle = "Commander",
                    DutyStartDate = new DateOnly(2021, 1, 1)
                },
                new AstronautDuty
                {
                    PersonId = peopleByName["Logan Wright"].Id,
                    Rank = "1LT",
                    DutyTitle = "Mission Specialist",
                    DutyStartDate = new DateOnly(2014, 2, 14),
                    DutyEndDate = new DateOnly(2016, 6, 30)
                },
                new AstronautDuty
                {
                    PersonId = peopleByName["Logan Wright"].Id,
                    Rank = "CPT",
                    DutyTitle = "Flight Engineer",
                    DutyStartDate = new DateOnly(2016, 7, 1),
                    DutyEndDate = new DateOnly(2019, 2, 13)
                },
                new AstronautDuty
                {
                    PersonId = peopleByName["Logan Wright"].Id,
                    Rank = "MAJ",
                    DutyTitle = "Pilot",
                    DutyStartDate = new DateOnly(2019, 2, 14)
                },
                new AstronautDuty
                {
                    PersonId = peopleByName["Sophia Chen"].Id,
                    Rank = "CPT",
                    DutyTitle = "Mission Specialist",
                    DutyStartDate = new DateOnly(2020, 3, 1)
                },
                new AstronautDuty
                {
                    PersonId = peopleByName["Jackson Cole"].Id,
                    Rank = "MAJ",
                    DutyTitle = "Payload Specialist",
                    DutyStartDate = new DateOnly(2011, 4, 20),
                    DutyEndDate = new DateOnly(2013, 12, 31)
                },
                new AstronautDuty
                {
                    PersonId = peopleByName["Jackson Cole"].Id,
                    Rank = "MAJ",
                    DutyTitle = "Mission Specialist",
                    DutyStartDate = new DateOnly(2014, 1, 1),
                    DutyEndDate = new DateOnly(2017, 4, 19)
                },
                new AstronautDuty
                {
                    PersonId = peopleByName["Jackson Cole"].Id,
                    Rank = "LTC",
                    DutyTitle = "Flight Engineer",
                    DutyStartDate = new DateOnly(2017, 4, 20)
                },
                new AstronautDuty
                {
                    PersonId = peopleByName["Isabella Cruz"].Id,
                    Rank = "CPT",
                    DutyTitle = "Payload Specialist",
                    DutyStartDate = new DateOnly(2021, 6, 5)
                },
                new AstronautDuty
                {
                    PersonId = peopleByName["Aiden Walker"].Id,
                    Rank = "CPT",
                    DutyTitle = "Payload Specialist",
                    DutyStartDate = new DateOnly(2016, 8, 12),
                    DutyEndDate = new DateOnly(2019, 8, 11)
                },
                new AstronautDuty
                {
                    PersonId = peopleByName["Aiden Walker"].Id,
                    Rank = "MAJ",
                    DutyTitle = "Mission Specialist",
                    DutyStartDate = new DateOnly(2019, 8, 12)
                },
                new AstronautDuty
                {
                    PersonId = peopleByName["Mia Foster"].Id,
                    Rank = "CPT",
                    DutyTitle = "Pilot",
                    DutyStartDate = new DateOnly(2022, 1, 15)
                },
                new AstronautDuty
                {
                    PersonId = peopleByName["Henry Powell"].Id,
                    Rank = "MAJ",
                    DutyTitle = "Payload Specialist",
                    DutyStartDate = new DateOnly(2010, 2, 1),
                    DutyEndDate = new DateOnly(2012, 12, 31)
                },
                new AstronautDuty
                {
                    PersonId = peopleByName["Henry Powell"].Id,
                    Rank = "MAJ",
                    DutyTitle = "Mission Specialist",
                    DutyStartDate = new DateOnly(2013, 1, 1),
                    DutyEndDate = new DateOnly(2016, 12, 31)
                },
                new AstronautDuty
                {
                    PersonId = peopleByName["Henry Powell"].Id,
                    Rank = "LTC",
                    DutyTitle = "Commander",
                    DutyStartDate = new DateOnly(2017, 1, 1),
                    DutyEndDate = new DateOnly(2022, 3, 31)
                },
                new AstronautDuty
                {
                    PersonId = peopleByName["Henry Powell"].Id,
                    Rank = "COL",
                    DutyTitle = "RETIRED",
                    DutyStartDate = new DateOnly(2022, 4, 1)
                },
                new AstronautDuty
                {
                    PersonId = peopleByName["Charlotte Diaz"].Id,
                    Rank = "CPT",
                    DutyTitle = "Mission Specialist",
                    DutyStartDate = new DateOnly(2011, 5, 10),
                    DutyEndDate = new DateOnly(2014, 12, 31)
                },
                new AstronautDuty
                {
                    PersonId = peopleByName["Charlotte Diaz"].Id,
                    Rank = "MAJ",
                    DutyTitle = "Payload Specialist",
                    DutyStartDate = new DateOnly(2015, 1, 1),
                    DutyEndDate = new DateOnly(2018, 12, 31)
                },
                new AstronautDuty
                {
                    PersonId = peopleByName["Charlotte Diaz"].Id,
                    Rank = "MAJ",
                    DutyTitle = "Flight Engineer",
                    DutyStartDate = new DateOnly(2019, 1, 1),
                    DutyEndDate = new DateOnly(2023, 8, 14)
                },
                new AstronautDuty
                {
                    PersonId = peopleByName["Charlotte Diaz"].Id,
                    Rank = "LTC",
                    DutyTitle = "RETIRED",
                    DutyStartDate = new DateOnly(2023, 8, 15)
                },
                new AstronautDuty
                {
                    PersonId = peopleByName["Lucas Kim"].Id,
                    Rank = "CPT",
                    DutyTitle = "Mission Specialist",
                    DutyStartDate = new DateOnly(2012, 9, 1),
                    DutyEndDate = new DateOnly(2015, 12, 31)
                },
                new AstronautDuty
                {
                    PersonId = peopleByName["Lucas Kim"].Id,
                    Rank = "CPT",
                    DutyTitle = "Flight Engineer",
                    DutyStartDate = new DateOnly(2016, 1, 1),
                    DutyEndDate = new DateOnly(2019, 12, 31)
                },
                new AstronautDuty
                {
                    PersonId = peopleByName["Lucas Kim"].Id,
                    Rank = "CPT",
                    DutyTitle = "Payload Specialist",
                    DutyStartDate = new DateOnly(2020, 1, 1),
                    DutyEndDate = new DateOnly(2024, 1, 19)
                },
                new AstronautDuty
                {
                    PersonId = peopleByName["Lucas Kim"].Id,
                    Rank = "MAJ",
                    DutyTitle = "RETIRED",
                    DutyStartDate = new DateOnly(2024, 1, 20)
                }
            };
            await context.AstronautDuties.AddRangeAsync(dutySeed, cancellationToken);

            await context.SaveChangesAsync(cancellationToken);
        }
    }
}
