using Kickify.Api.Extensions;
using Kickify.Api.Requests;
using Kickify.Application.Features.Auth.Commands.ChangePassword;
using Kickify.Application.Features.Auth.Commands.ForgotPassword;
using Kickify.Application.Features.Auth.Commands.Login;
using Kickify.Application.Features.Auth.Commands.RegisterPlayer;
using Kickify.Application.Features.Auth.Commands.RegisterVenueOwner;
using Kickify.Application.Features.Auth.Commands.ResendOtp;
using Kickify.Application.Features.Auth.Commands.VerifyMail;
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
            var command = new LoginCommand
            {
                Email = request.Email,
                Password = request.Password
            };
            Result<LoginCommandResponse> result = await _mediator.Send(command, cancellationToken);
            return result.MatchOk();
        }

        [HttpPost("auth/firebase/venue-owner/login")]
        public async Task<IResult> LoginVenueOwnerWithFirebase([FromBody] LoginWithFirebaseForVenueOwnerRequest request, CancellationToken cancellationToken)
        {
            var command = new LoginWithFirebaseForVenueOwnerCommand
            {
                Uid = request.Uid
            };
            Result<LoginWithFirebaseForVenueOwnerCommandResponse> result = await _mediator.Send(command, cancellationToken);
            return result.MatchOk();
        }

        [HttpPost("auth/firebase/login")]
        public async Task<IResult> LoginWithFirebase([FromBody] LoginWithFirebaseRequest request, CancellationToken cancellationToken)
        {
            var command = new LoginWithFirebaseCommand
            {
                Uid = request.Uid
            };
            Result<LoginWithFirebaseCommandResponse> result = await _mediator.Send(command, cancellationToken);
            return result.MatchOk();
        }

        [HttpPost("auth/register-player")]
        public async Task<IResult> RegisterPlayer([FromBody] RegisterPlayerRequest request, CancellationToken cancellationToken)
        {
            var command = new RegisterPlayerCommand
            {
                Email = request.Email,
                Password = request.Password,
                FullName = request.FullName
            };
            Result<RegisterPlayerCommandResponse> result = await _mediator.Send(command, cancellationToken);
            return result.MatchCreated(id => $"/user/{id}");
        }

        [HttpPost("auth/register-venue-owner")]
        public async Task<IResult> RegisterVenueOwner([FromBody] RegisterVenueOwnerRequest request, CancellationToken cancellationToken)
        {
            var command = new RegisterVenueOwnerCommand
            {
                Email = request.Email,
                Password = request.Password,
                FullName = request.FullName
            };
            Result<RegisterVenueOwnerCommandResponse> result = await _mediator.Send(command, cancellationToken);
            return result.MatchCreated(id => $"/user/{id}");
        }

        [HttpPost("auth/login-with-refresh-token")]
        public async Task<IResult> LoginWithRefreshToken([FromBody] LoginWithRefreshTokenRequest request, CancellationToken cancellationToken)
        {
            var command = new LoginWithRefreshTokenCommand
            {
                RefreshToken = request.RefreshToken
            };
            Result<LoginWithRefreshTokenCommandResponse> result = await _mediator.Send(command, cancellationToken);
            return result.MatchOk();
        }

        [HttpPost("auth/verify-mail")]
        public async Task<IResult> VerifyMail([FromBody] VerifyMailRequest request, CancellationToken cancellationToken)
        {
            var command = new VerifyMailCommand
            {
                UserId = request.UserId,
                Otp = request.Otp
            };
            Result<VerifyMailCommandResponse> result = await _mediator.Send(command, cancellationToken);
            return result.MatchOk();
        }

        [HttpPost("auth/resend-otp")]
        public async Task<IResult> ResendOtp([FromBody] ResendOtpRequest request, CancellationToken cancellationToken)
        {
            var command = new ResendOtpCommand
            {
                UserId = request.UserId
            };
            Result<ResendOtpCommandResponse> result = await _mediator.Send(command, cancellationToken);
            return result.MatchOk();
        }

        [HttpPost("auth/forgot-password")]
        public async Task<IResult> ForgotPassword([FromBody] ForgotPasswordRequest request, CancellationToken cancellationToken)
        {
            var command = new ForgotPasswordCommand
            {
                Email = request.Email
            };
            Result<ForgotPasswordCommandResponse> result = await _mediator.Send(command, cancellationToken);
            return result.MatchOk();
        }

        [HttpPost("auth/change-password")]
        public async Task<IResult> ChangePassword([FromBody] ChangePasswordRequest request, CancellationToken cancellationToken)
        {
            var command = new ChangePasswordCommand
            {
                Email = request.Email,
                CurrentPassword = request.CurrentPassword,
                NewPassword = request.NewPassword
            };
            Result<ChangePasswordCommandResponse> result = await _mediator.Send(command, cancellationToken);
            return result.MatchOk();
        }
    }
}
