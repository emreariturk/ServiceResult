using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceResult.Core
{
    public readonly record struct Error(
        string Code,
        string Message,
        ErrorType Type = ErrorType.Failure,
        IReadOnlyDictionary<string, string[]>? ValidationErrors = null)
    {
        public static Error Failure(string code, string message) =>
            new(code, message, ErrorType.Failure);

        public static Error NotFound(string code, string message) =>
            new(code, message, ErrorType.NotFound);

        public static Error Conflict(string code, string message) =>
            new(code, message, ErrorType.Conflict);

        public static Error Unauthorized(string code, string message) =>
            new(code, message, ErrorType.Unauthorized);

        public static Error Forbidden(string code, string message) =>
            new(code, message, ErrorType.Forbidden);

        public static Error Validation(string code, string message, IReadOnlyDictionary<string, string[]>? errors = null) =>
            new(code, message, ErrorType.Validation, errors);

        public static Error Unexpected(string code, string message) =>
            new(code, message, ErrorType.Unexpected);
    }
}