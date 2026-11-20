using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceResult.Core
{
    internal interface IResultEnvelope
    {
        bool IsSuccess { get; }
        Error Error { get; }
        object? GetValue();
    }
}
