using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#if NET8_0_OR_GREATER
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using ServiceResult.Core;
namespace ServiceResult.Http
{
    public static class EndpointExtensions
    {
        public static RouteGroupBuilder WithResultMapping(this RouteGroupBuilder group)
        {
            return group.AddEndpointFilter<ResultEndpointFilter>();
        }

        public static RouteHandlerBuilder WithResultMapping(this RouteHandlerBuilder builder)
        {
            return builder.AddEndpointFilter<ResultEndpointFilter>();
        }

        public static IResult ToHttpResult(this Result result)
        {
            return ProblemDetailsMapper.MapEnvelope(result, StatusCodes.Status204NoContent);
        }

        public static IResult ToHttpResult<T>(this Result<T> result, int successStatusCode = StatusCodes.Status200OK)
        {
            return ProblemDetailsMapper.MapEnvelope(result, successStatusCode);
        }
    }
}
#endif
