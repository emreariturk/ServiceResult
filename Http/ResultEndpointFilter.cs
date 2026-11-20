using ServiceResult.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#if NET8_0_OR_GREATER
using Microsoft.AspNetCore.Http;
namespace ServiceResult.Http
{
    public sealed class ResultEndpointFilter : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next)
    {
        var response = await next(context);

        if (response is IResultEnvelope envelope)
        {
            return ProblemDetailsMapper.MapEnvelope(envelope, StatusCodes.Status200OK);
        }

        return response;
    }
}
#endif