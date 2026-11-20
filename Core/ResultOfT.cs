using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceResult.Core
{
    public sealed class Result<T> : IResultEnvelope
    {
        private readonly T? _value;

        public bool IsSuccess { get; }
        public bool IsFailure => !IsSuccess;
        public Error Error { get; }

        public T Value => IsSuccess
            ? _value!
            : throw new InvalidOperationException("Başarısız bir Result nesnesinin Value özelliğine erişilemez.");

        private Result(T value)
        {
            IsSuccess = true;
            _value = value;
            Error = default;
        }

        private Result(Error error)
        {
            IsSuccess = false;
            _value = default;
            Error = error;
        }

        public static Result<T> Success(T value) => new(value);
        public static Result<T> Failure(Error error) => new(error);

        public static implicit operator Result<T>(T value) => Success(value);
        public static implicit operator Result<T>(Error error) => Failure(error);

        object? IResultEnvelope.GetValue() => _value;
    }
}
