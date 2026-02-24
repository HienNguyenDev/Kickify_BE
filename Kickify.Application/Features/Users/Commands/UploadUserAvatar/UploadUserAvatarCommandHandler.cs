using Kickify.Application.Abstractions.Authentication;
using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Persistence;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Application.Abstractions.Services;
using Kickify.Domain.Common;
using Kickify.Domain.Errors;

namespace Kickify.Application.Features.Users.Commands.UploadUserAvatar;

public class UploadUserAvatarCommandHandler : ICommandHandler<UploadUserAvatarCommand, UploadUserAvatarCommandResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IStorageService _storageService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserContext _userContext;

    public UploadUserAvatarCommandHandler(IUserRepository userRepository, IStorageService storageService, IUnitOfWork unitOfWork, IUserContext userContext)
    {
        _userRepository = userRepository;
        _storageService = storageService;
        _unitOfWork = unitOfWork;
        _userContext = userContext;
    }

    public async Task<Result<UploadUserAvatarCommandResponse>> Handle(UploadUserAvatarCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(_userContext.UserId);
        if (user is null)
        {
            return Result.Failure<UploadUserAvatarCommandResponse>(UserErrors.NotFound(_userContext.UserId));
        }

        if (!string.IsNullOrEmpty(user.AvatarUrl))
        {
            var oldObjectName = ExtractObjectNameFromUrl(user.AvatarUrl);
            if (!string.IsNullOrEmpty(oldObjectName))
            {
                await _storageService.DeleteAsync(oldObjectName, cancellationToken);
            }
        }

        var uploadResult = await _storageService.UploadAsync(request.File.Stream, request.File.FileName, request.File.ContentType, cancellationToken);
        if (!uploadResult.Success)
        {
            return Result.Failure<UploadUserAvatarCommandResponse>(UserErrors.AvatarUploadFailed(uploadResult.ErrorMessage ?? "Unknown error"));
        }

        user.AvatarUrl = uploadResult.PublicUrl;
        _userRepository.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var response = new UploadUserAvatarCommandResponse
        {
            UserId = user.UserId,
            AvatarUrl = user.AvatarUrl
        };

        return Result.Success(response);
    }

    private static string? ExtractObjectNameFromUrl(string url)
    {
        var uri = new Uri(url);
        var path = uri.AbsolutePath.TrimStart('/');
        var bucketEndIndex = path.IndexOf('/');
        return bucketEndIndex >= 0 ? path[(bucketEndIndex + 1)..] : null;
    }
}
