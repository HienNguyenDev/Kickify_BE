using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kickify.Domain.Common
{
    public interface ISoftDeletable
    {
        DateTime? DeletedAt { get; set; }
    }
}
