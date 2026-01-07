using Kickify.Api.Extensions;
using Kickify.Api.Requests;
using Kickify.Application.Features.Auth.Commands.Login;
using Kickify.Domain.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Kickify.Api.Controllers
{
    [Route("api/")]
    [ApiController]
    public class AuthenticateController : ControllerBase
    {
        private readonly ISender _mediator;

        public AuthenticateController(ISender mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("auth/email/login")]
        public async Task<IResult> LoginUser([FromBody] LoginRequest request, CancellationToken cancellationToken)
        {
            LoginCommand command = new LoginCommand
            {
                Email = request.Email,
                Password = request.Password
            };
            Result<LoginCommandResponse> result = await _mediator.Send(command, cancellationToken);
            return result.MatchOk();
        }
    }
}
