using Kickify.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kickify.Application.Features.Posts.Commands.CreatePost
{
    public class CreatePostCommandResponse
    {
        public bool Success { get; set; }
        public Guid? PostId { get; set; }
        public List<MediaDto>? Media { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
    }
}
