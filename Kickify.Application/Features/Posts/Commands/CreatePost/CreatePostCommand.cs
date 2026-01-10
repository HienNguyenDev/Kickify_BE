using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kickify.Application.Features.Posts.Commands.CreatePost
{
    public class CreatePostCommand : ICommand<CreatePostCommandResponse>
    {
        public string Content { get; set; } = string.Empty;
        public List<FileUploadRequest> Files { get; set; } = new();
    }
}
