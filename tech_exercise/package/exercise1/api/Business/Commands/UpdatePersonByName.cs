using MediatR;
using MediatR.Pipeline;
using Microsoft.EntityFrameworkCore;
using StargateAPI.Business.Data;
using StargateAPI.Controllers;
using StargateAPI.Infrastructure.Exceptions;

namespace StargateAPI.Business.Commands
{
    public class UpdatePersonByName : IRequest<UpdatePersonByNameResult>
    {
        public string RouteName { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }

    public class UpdatePersonByNamePreProcessor : IRequestPreProcessor<UpdatePersonByName>
    {
        public Task Process(UpdatePersonByName request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.RouteName))
            {
                throw new ClientInputException("Route name is required.");
            }

            if (string.IsNullOrWhiteSpace(request.Name))
            {
                throw new ClientInputException("Name is required.");
            }

            request.RouteName = request.RouteName.Trim();
            request.Name = request.Name.Trim();

            return Task.CompletedTask;
        }
    }

    public class UpdatePersonByNameHandler : IRequestHandler<UpdatePersonByName, UpdatePersonByNameResult>
    {
        private readonly StargateContext _context;

        public UpdatePersonByNameHandler(StargateContext context)
        {
            _context = context;
        }

        public async Task<UpdatePersonByNameResult> Handle(UpdatePersonByName request, CancellationToken cancellationToken)
        {
            var person = await _context.People.FirstOrDefaultAsync(z => z.Name == request.RouteName, cancellationToken);

            if (person is null)
            {
                throw new ClientInputException("Person not found.");
            }

            var duplicate = await _context.People
                .AsNoTracking()
                .AnyAsync(z => z.Name == request.Name && z.Id != person.Id, cancellationToken);
            if (duplicate)
            {
                throw new ClientInputException("A person with this name already exists.");
            }

            person.Name = request.Name;

            try
            {
                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException)
            {
                throw new ClientInputException("A person with this name already exists.");
            }

            return new UpdatePersonByNameResult();
        }
    }

    public class UpdatePersonByNameResult : BaseResponse
    {
    }
}
