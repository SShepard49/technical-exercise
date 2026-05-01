using MediatR;
using MediatR.Pipeline;
using Microsoft.EntityFrameworkCore;
using StargateAPI.Business.Data;
using StargateAPI.Controllers;
using StargateAPI.Infrastructure.Exceptions;

namespace StargateAPI.Business.Commands
{

    //Added some defensive coding

    public class CreatePerson : IRequest<CreatePersonResult>
    {
        public required string Name { get; set; } = string.Empty;
    }

    public class CreatePersonPreProcessor : IRequestPreProcessor<CreatePerson>
    {
        private readonly StargateContext _context;
        public CreatePersonPreProcessor(StargateContext context)
        {
            _context = context;
        }
        public Task Process(CreatePerson request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                throw new ClientInputException("Name is required.");
            }

            request.Name = request.Name.Trim();

            var person = _context.People.AsNoTracking().FirstOrDefault(z => z.Name == request.Name);

            if (person is not null)
            {
                throw new ClientInputException("A person with this name already exists.");
            }

            return Task.CompletedTask;
        }
    }

    public class CreatePersonHandler : IRequestHandler<CreatePerson, CreatePersonResult>
    {
        private readonly StargateContext _context;

        public CreatePersonHandler(StargateContext context)
        {
            _context = context;
        }
        
        public async Task<CreatePersonResult> Handle(CreatePerson request, CancellationToken cancellationToken)
        {
            var newPerson = new Person()
            {
                Name = request.Name
            };

            await _context.People.AddAsync(newPerson, cancellationToken);

            try
            {
                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException)
            {
                throw new ClientInputException("A person with this name already exists.");
            }

            return new CreatePersonResult()
            {
                Id = newPerson.Id
            };
        }
    }

    public class CreatePersonResult : BaseResponse
    {
        public int Id { get; set; }
    }
}
