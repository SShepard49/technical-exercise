using MediatR;
using Microsoft.AspNetCore.Mvc;
using StargateAPI.Business.Commands;
using StargateAPI.Business.Dtos;
using StargateAPI.Business.Queries;

namespace StargateAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PersonController : ControllerBase
    {
        private readonly IMediator _mediator;
        public PersonController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("")]
        public async Task<IActionResult> GetPeople()
        {
            var result = await _mediator.Send(new GetPeople()
            {
            });
            //Moved exception handling to middleware

            return this.GetResponse(result);
        }

        [HttpGet("{name}")]
        public async Task<IActionResult> GetPersonByName(string name)
        {
            var result = await _mediator.Send(new GetPersonByName()
            {
                Name = name
            });
            //Moved exception handling to middleware

            return this.GetResponse(result);
        }

        [HttpPost("")]
        public async Task<IActionResult> CreatePerson([FromBody] string name)
        {
            var result = await _mediator.Send(new CreatePerson()
            {
                Name = name
            });
            //Moved exception handling to middleware
            return this.GetResponse(result);
        }

        [HttpPut("{name}")]
        public async Task<IActionResult> UpdatePersonByName(string name, [FromBody] UpdatePersonRequest request)
        {
            //Created missing UpdatePersonByName command
            var command = new UpdatePersonByName
            {
                RouteName = name,
                Name = request.Name
            };

            var result = await _mediator.Send(command);

            return this.GetResponse(result);
        }
    }
}