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

            var personOne = new Person { Name = "John Doe" };
            var personTwo = new Person { Name = "Jane Doe" };
            await context.People.AddRangeAsync(new[] { personOne, personTwo }, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);

            var johnCurrentStart = new DateOnly(2022, 1, 1);
            var janeCareerStart = new DateOnly(2020, 1, 1);
            var janeRetiredStart = new DateOnly(2021, 6, 1);
            var janePreviousEnd = janeRetiredStart.AddDays(-1);

            var detailSeed = new[]
            {
                new AstronautDetail
                {
                    PersonId = personOne.Id,
                    CurrentRank = "1LT",
                    CurrentDutyTitle = "Commander",
                    CareerStartDate = johnCurrentStart
                },
                new AstronautDetail
                {
                    PersonId = personTwo.Id,
                    CurrentRank = "COL",
                    CurrentDutyTitle = "RETIRED",
                    CareerStartDate = janeCareerStart,
                    CareerEndDate = janePreviousEnd
                }
            };
            await context.AstronautDetails.AddRangeAsync(detailSeed, cancellationToken);

            var dutySeed = new[]
            {
                new AstronautDuty
                {
                    PersonId = personOne.Id,
                    Rank = "1LT",
                    DutyTitle = "Commander",
                    DutyStartDate = johnCurrentStart
                },
                new AstronautDuty
                {
                    PersonId = personTwo.Id,
                    Rank = "MAJ",
                    DutyTitle = "Pilot",
                    DutyStartDate = janeCareerStart,
                    DutyEndDate = janePreviousEnd
                },
                new AstronautDuty
                {
                    PersonId = personTwo.Id,
                    Rank = "COL",
                    DutyTitle = "RETIRED",
                    DutyStartDate = janeRetiredStart
                }
            };
            await context.AstronautDuties.AddRangeAsync(dutySeed, cancellationToken);

            await context.SaveChangesAsync(cancellationToken);
        }
    }
}
