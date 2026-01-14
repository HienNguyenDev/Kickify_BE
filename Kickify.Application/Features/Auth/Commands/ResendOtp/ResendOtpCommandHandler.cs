using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.OTP;
using Kickify.Application.Abstractions.Persistence;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Common;
using Kickify.Domain.Errors;
using Kickify.Domain.Event;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kickify.Application.Features.Auth.Commands.ResendOtp
{
    public class ResendOtpCommandHandler : ICommandHandler<ResendOtpCommand, ResendOtpCommandResponse>
    {
        private readonly IUserRepository _userRepository;
        private readonly IRedisOtpStore _otpStore;
        private readonly IOtpGenerator _otpGen;
        private readonly IUnitOfWork _unitOfWork;

        public ResendOtpCommandHandler(IUserRepository userRepository, IRedisOtpStore otpStore, IOtpGenerator otpGen, IUnitOfWork unitOfWork)
        {
            _userRepository = userRepository;
            _otpStore = otpStore;
            _otpGen = otpGen;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<ResendOtpCommandResponse>> Handle(ResendOtpCommand command, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByIdAsync(command.UserId);

            if (user == null)
                return Result.Failure<ResendOtpCommandResponse>(UserErrors.NotFound(command.UserId));
            if (user.IsEmailVerified)
                return Result.Failure<ResendOtpCommandResponse>(UserErrors.UserAlreadyVerified);

            var otp = _otpGen.Generate6Digits();
            await _otpStore.StoreAsync(user.UserId, otp, TimeSpan.FromMinutes(5), cancellationToken);

            user.Raise(new ResendOtpDomainEvent(user.UserId, user.Email, otp));
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new ResendOtpCommandResponse
            {
                UserId = user.UserId,
            };
        }
    }
}
