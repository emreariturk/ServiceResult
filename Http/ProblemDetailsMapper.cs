using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#if NET8_0_OR_GREATER
using Microsoft.AspNetCore.Http;
using ServiceResult.Core;

namespace ServiceResult.Http
{
    internal static class ProblemDetailsMapper
    {
        public static IResult ToProblemResult(Error error)
        {
            var (statusCode, title) = error.Type switch
            {
                ErrorType.Validation => (StatusCodes.Status400BadRequest, "Validation Error"),
                ErrorType.NotFound => (StatusCodes.Status404NotFound, "Resource Not Found"),
                ErrorType.Conflict => (StatusCodes.Status409Conflict, "Conflict"),
                ErrorType.Unauthorized => (StatusCodes.Status401Unauthorized, "Unauthorized"),
                ErrorType.Forbidden => (StatusCodes.Status403Forbidden, "Forbidden"),
                ErrorType.Failure => (StatusCodes.Status400BadRequest, "Bad Request"),
                _ => (StatusCodes.Status500InternalServerError, "Internal Server Error")
            };

            var extensions = new Dictionary<string, object?>
            {
                ["errorCode"] = error.Code
            };

            if (error.ValidationErrors is { Count: > 0 })
            {
                extensions["errors"] = error.ValidationErrors;
            }

            return Results.Problem(
                detail: error.Message,
                statusCode: statusCode,
                title: title,
                extensions: extensions);
        }

        public static IResult MapEnvelope(IResultEnvelope envelope, int successStatusCode)
        {
            if (envelope.IsFailure)
            {
                return ToProblemResult(envelope.Error);
            }

            var value = envelope.GetValue();
            if (value is null)
            {
                return Results.NoContent();
            }

            return successStatusCode == StatusCodes.Status201Created
                ? Results.Created(string.Empty, value)
                : Results.Json(value, statusCode: successStatusCode);
        }
    }
}
#endif
