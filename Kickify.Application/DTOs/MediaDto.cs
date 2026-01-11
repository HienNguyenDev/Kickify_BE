using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kickify.Application.DTOs
{
    public record MediaDto(Guid Id, string Url, string Type);
}
