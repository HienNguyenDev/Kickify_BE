using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kickify.Domain.Enums
{
    public enum PaymentStatus
    {
        Pending,
        Completed,
        Failed,
        Expired,
        Cancelled
    }
}
