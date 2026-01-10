using Kickify.Application.Abstractions.Authentication;
using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Persistence;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Application.Abstractions.Services;
using Kickify.Application.DTOs;
using Kickify.Domain.Common;
using Kickify.Domain.Entities;
using Kickify.Domain.Enums;
using Kickify.Domain.Errors;

namespace Kickify.Application.Features.Posts.Commands.CreatePost;

public class CreatePostCommandHandler : ICommandHandler<CreatePostCommand, CreatePostCommandResponse>
{
    private readonly IStorageService _storageService;
    private readonly IPostRepository _postRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserContext _userContext;

    public CreatePostCommandHandler(IStorageService storageService, IPostRepository postRepository, IUnitOfWork unitOfWork, IUserContext userContext)
    {
        _storageService = storageService;
        _postRepository = postRepository;
        _unitOfWork = unitOfWork;
        _userContext = userContext;
    }

    public async Task<Result<CreatePostCommandResponse>> Handle(CreatePostCommand request, CancellationToken cancellationToken)
    {
        var postId = Guid.NewGuid();
        var mediaList = new List<PostMedia>();

        if (request.Files.Count > 0)
        {
            var uploadResults = await _storageService.UploadMultipleAsync(request.Files, cancellationToken);

            var failedUploads = uploadResults.Where(r => !r.Success).ToList();
            if (failedUploads.Count > 0)
            {
                var successfulUploads = uploadResults.Where(r => r.Success).ToList();
                foreach (var upload in successfulUploads)
                {
                    await _storageService.DeleteAsync(upload.ObjectName, cancellationToken);
                }

                var errors = string.Join(", ", failedUploads.Select(f => f.ErrorMessage));
                return Result.Failure<CreatePostCommandResponse>(PostErrors.UploadFailed(errors));
            }

            var displayOrder = 0;
            foreach (var (file, upload) in request.Files.Zip(uploadResults))
            {
                mediaList.Add(new PostMedia
                {
                    MediaId = Guid.NewGuid(),
                    PostId = postId,
                    FileName = file.FileName,
                    StoragePath = upload.ObjectName,
                    PublicUrl = upload.PublicUrl,
                    ContentType = file.ContentType,
                    FileSize = upload.FileSize,
                    MediaType = file.ContentType.StartsWith("video/") ? MediaType.Video : MediaType.Image,
                    DisplayOrder = displayOrder++
                });
            }
        }

        var post = new Post
        {
            PostId = postId,
            UserId = _userContext.UserId,
            Content = request.Content,
            TotalMedia = mediaList.Count,
            PostMedia = mediaList
        };

        await _postRepository.AddAsync(post);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var response = new CreatePostCommandResponse
        {
            Success = true,
            PostId = post.PostId,
            Media = mediaList.Select(m => new MediaDto(
                m.MediaId,
                m.PublicUrl,
                m.MediaType.ToString()
            )).ToList()
        };

        return Result.Success(response);
    }
}
