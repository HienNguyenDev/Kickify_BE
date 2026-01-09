using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.OTP;
using Kickify.Application.Abstractions.Persistence;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Common;
using Kickify.Domain.Errors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kickify.Application.Features.Auth.Commands.VerifyMail
{
    public class VerifyMailCommandHandler : ICommandHandler<VerifyMailCommand, VerifyMailCommandResponse>
    {
        private readonly IRedisOtpStore _otpStore;
        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;

        public VerifyMailCommandHandler(IRedisOtpStore otpStore, IUserRepository userRepository, IUnitOfWork unitOfWork)
        {
            _otpStore = otpStore;
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<VerifyMailCommandResponse>> Handle(VerifyMailCommand command, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByIdAsync(command.UserId);
            if (user == null)
                return Result.Failure<VerifyMailCommandResponse>(UserErrors.NotFound(command.UserId));

            var otp = await _otpStore.GetAsync(command.UserId, cancellationToken);
            if (otp is null)
                return Result.Failure<VerifyMailCommandResponse>(UserErrors.OtpExpired);
            if (otp != command.Otp)
                return Result.Failure<VerifyMailCommandResponse>(UserErrors.WrongOtp);

            await _otpStore.RemoveAsync(command.UserId, cancellationToken);
            user.IsEmailVerified = true;
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var response = new VerifyMailCommandResponse
            {
                UserId = user.UserId,
            };
            return Result.Success(response);
        }
    }
}
