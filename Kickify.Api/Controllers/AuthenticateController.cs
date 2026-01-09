using Kickify.Api.Extensions;
using Kickify.Api.Requests;
using Kickify.Application.Features.Auth.Commands.Login;
using Kickify.Application.Features.Auth.Commands.RegisterPlayer;
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

        [HttpPost("auth/firebase/login")]
        public async Task<IResult> LoginWithFirebase([FromBody] LoginWithFirebaseRequest request, CancellationToken cancellationToken)
        {
            LoginWithFirebaseCommand command = new LoginWithFirebaseCommand
            {
                Uid = request.Uid
            };
            Result<LoginWithFirebaseCommandResponse> result = await _mediator.Send(command, cancellationToken);
            return result.MatchOk();
        }

        [HttpPost("auth/register-player")]
        public async Task<IResult> RegisterPlayer([FromBody] RegisterPlayerRequest request, CancellationToken cancellationToken)
        {
            RegisterPlayerCommand command = new RegisterPlayerCommand
            {
                Email = request.Email,
                Password = request.Password,
                FullName = request.FullName
            };
            Result<RegisterPlayerCommandResponse> result = await _mediator.Send(command, cancellationToken);
            return result.MatchCreated(id => $"/user/{id}");
        }

        [HttpPost("auth/login-with-refresh-token")]
        public async Task<IResult> LoginWithRefreshToken([FromBody] LoginWithRefreshTokenRequest request, CancellationToken cancellationToken)
        {
            LoginWithRefreshTokenCommand command = new LoginWithRefreshTokenCommand
            {
                RefreshToken = request.RefreshToken
            };
            Result<LoginWithRefreshTokenCommandResponse> result = await _mediator.Send(command, cancellationToken);
            return result.MatchOk();
        }
    }
}
