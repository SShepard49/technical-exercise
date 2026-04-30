using Dapper;
using MediatR;
using MediatR.Pipeline;
using Microsoft.EntityFrameworkCore;
using StargateAPI.Business.Data;
using StargateAPI.Controllers;
using StargateAPI.Infrastructure.Exceptions;

namespace StargateAPI.Business.Commands
{
    public class CreateAstronautDuty : IRequest<CreateAstronautDutyResult>
    {
        public required string Name { get; set; }

        public required string Rank { get; set; }

        public required string DutyTitle { get; set; }

        public DateOnly DutyStartDate { get; set; }
    }

    public class CreateAstronautDutyPreProcessor : IRequestPreProcessor<CreateAstronautDuty>
    {
        private readonly StargateContext _context;

        public CreateAstronautDutyPreProcessor(StargateContext context)
        {
            _context = context;
        }

        public Task Process(CreateAstronautDuty request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                throw new ClientInputException("Name is required.");
            }

            if (string.IsNullOrWhiteSpace(request.Rank))
            {
                throw new ClientInputException("Rank is required.");
            }

            if (string.IsNullOrWhiteSpace(request.DutyTitle))
            {
                throw new ClientInputException("DutyTitle is required.");
            }

            request.Name = request.Name.Trim();
            var person = _context.People.AsNoTracking().FirstOrDefault(z => z.Name == request.Name);

            if (person is null)
            {
                throw new ClientInputException("Person not found.");
            }

            //A Person will only ever hold one current Astronaut Duty Title, Start Date, and Rank at a time.
            var verifyNoPreviousDuty = _context.AstronautDuties.AsNoTracking().FirstOrDefault(z =>
                z.PersonId == person.Id &&
                z.DutyTitle == request.DutyTitle &&
                z.DutyStartDate == request.DutyStartDate);

            if (verifyNoPreviousDuty is not null)
            {
                throw new ClientInputException("This astronaut duty already exists for the person.");
            }

            return Task.CompletedTask;
        }
    }

    public class CreateAstronautDutyHandler : IRequestHandler<CreateAstronautDuty, CreateAstronautDutyResult>
    {
        private readonly StargateContext _context;

        public CreateAstronautDutyHandler(StargateContext context)
        {
            _context = context;
        }
        public async Task<CreateAstronautDutyResult> Handle(CreateAstronautDuty request, CancellationToken cancellationToken)
        {
            var query = @"SELECT * FROM [Person] WHERE @Name = Name";

            var person = await _context.Connection.QueryFirstOrDefaultAsync<Person>(query, new { Name = request.Name });

            if (person == null)
            {
                throw new ClientInputException("Person not found.");
            }

            // Wrap in a transaction to make sure the Duty Dates are updated correctly
            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

            query = @"SELECT * FROM [AstronautDetail] WHERE @PersonId = PersonId";
            var astronautDetail = await _context.Connection.QueryFirstOrDefaultAsync<AstronautDetail>(query, new { PersonId = person.Id });

            if (astronautDetail == null)
            {
                astronautDetail = new AstronautDetail();
                astronautDetail.PersonId = person.Id;
                astronautDetail.CurrentDutyTitle = request.DutyTitle;
                astronautDetail.CurrentRank = request.Rank;
                astronautDetail.CareerStartDate = request.DutyStartDate;
                if (request.DutyTitle == "RETIRED")
                {
                    //A Person's Career End Date is one day before the Retired Duty Start Date.
                    astronautDetail.CareerEndDate = request.DutyStartDate.AddDays(-1);
                }

                await _context.AstronautDetails.AddAsync(astronautDetail, cancellationToken);

            }
            else
            {
                astronautDetail.CurrentDutyTitle = request.DutyTitle;
                astronautDetail.CurrentRank = request.Rank;
                if (request.DutyTitle == "RETIRED")
                {
                    //A Person's Career End Date is one day before the Retired Duty Start Date.
                    astronautDetail.CareerEndDate = request.DutyStartDate.AddDays(-1);
                }
                _context.AstronautDetails.Update(astronautDetail);
            }

            query = @"SELECT * FROM [AstronautDuty] WHERE PersonId = @PersonId Order By DutyStartDate Desc";

            var astronautDuty = await _context.Connection.QueryFirstOrDefaultAsync<AstronautDuty>(query, new { PersonId = person.Id });

            if (astronautDuty != null)
            {
                if (request.DutyStartDate <= astronautDuty.DutyStartDate)
                {
                    throw new ClientInputException("DutyStartDate must be later than the current duty start date.");
                }

                //A Person's Previous Duty End Date is set to the day before the New Astronaut Duty Start Date when a new Astronaut Duty is received for a Person.
                astronautDuty.DutyEndDate = request.DutyStartDate.AddDays(-1);
                _context.AstronautDuties.Update(astronautDuty);
            }

            var newAstronautDuty = new AstronautDuty()
            {
                PersonId = person.Id,
                Rank = request.Rank,
                DutyTitle = request.DutyTitle,
                DutyStartDate = request.DutyStartDate,
                DutyEndDate = null
            };

            await _context.AstronautDuties.AddAsync(newAstronautDuty, cancellationToken);

            try
            {
                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException)
            {
                throw new ClientInputException("Duty update conflicted with existing records. Please retry.");
            }

            await transaction.CommitAsync(cancellationToken);
            //Exceptions rollback commits on dispose.

            return new CreateAstronautDutyResult()
            {
                Id = newAstronautDuty.Id
            };
        }
    }

    public class CreateAstronautDutyResult : BaseResponse
    {
        public int? Id { get; set; }
    }
}
