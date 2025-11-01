using DataObjects;
using System.Diagnostics.CodeAnalysis;

namespace DataObjects
{
    /// <summary>
    /// Represents the result of an operation that can succeed or fail
    /// </summary>
    public class OperationResult
    {
        public bool IsSuccess { get; protected set; }
        public string? ErrorMessage { get; protected set; }
        public Exception? Exception { get; protected set; }
        public DateTime Timestamp { get; protected set; } = DateTime.UtcNow;

        protected OperationResult(bool isSuccess, string? errorMessage = null, Exception? exception = null)
        {
            IsSuccess = isSuccess;
            ErrorMessage = errorMessage;
            Exception = exception;
        }

        public static OperationResult Success() => new(true);
        public static OperationResult Failure(string errorMessage) => new(false, errorMessage);
        public static OperationResult Failure(Exception exception) => new(false, exception.Message, exception);
        public static OperationResult Failure(string errorMessage, Exception exception) => new(false, errorMessage, exception);

        [MemberNotNullWhen(false, nameof(ErrorMessage))]
        public bool IsFailure => !IsSuccess;
    }

    /// <summary>
    /// Represents the result of an operation that returns data
    /// </summary>
    /// <typeparam name="T">The type of data returned on success</typeparam>
    public class OperationResult<T> : OperationResult
    {
        public T? Data { get; private set; }

        private OperationResult(bool isSuccess, T? data = default, string? errorMessage = null, Exception? exception = null)
            : base(isSuccess, errorMessage, exception)
        {
            Data = data;
        }

        public static OperationResult<T> Success(T data) => new(true, data);
        public static new OperationResult<T> Failure(string errorMessage) => new(false, default, errorMessage);
        public static new OperationResult<T> Failure(Exception exception) => new(false, default, exception.Message, exception);
        public static new OperationResult<T> Failure(string errorMessage, Exception exception) => new(false, default, errorMessage, exception);

        [MemberNotNullWhen(true, nameof(Data))]
        public bool HasData => IsSuccess && Data != null;
    }
}