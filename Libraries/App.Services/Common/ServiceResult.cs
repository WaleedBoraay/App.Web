using System.Collections.Generic;

namespace App.Services.Common
{
    public class ServiceResult
    {
        public bool Success { get; private set; }
        public string Message { get; private set; }
        public string Error { get; private set; }
        public IDictionary<string, string> Errors { get; private set; }

        protected ServiceResult() { }

        protected ServiceResult(bool success, string message = null, string error = null, IDictionary<string, string> errors = null)
        {
            Success = success;
            Message = message;
            Error = error;
            Errors = errors;
        }

        #region Factory methods

        public static ServiceResult SuccessResult(string message = null)
            => new ServiceResult(true, message);

        public static ServiceResult Failed(string error)
            => new ServiceResult(false, null, error);

        public static ServiceResult Failed(IDictionary<string, string> errors)
            => new ServiceResult(false, null, null, errors);

        #endregion
    }

    public class ServiceResult<T> : ServiceResult
    {
        public T Data { get; private set; }

        private ServiceResult(T data, string message = null)
            : base(true, message)
        {
            Data = data;
        }

        private ServiceResult(string error)
            : base(false, null, error) { }

        private ServiceResult(IDictionary<string, string> errors)
            : base(false, null, null, errors) { }

        #region Factory methods

        public static ServiceResult<T> Success(T data, string message = null)
            => new ServiceResult<T>(data, message);

        public static new ServiceResult<T> Failed(string error)
            => new ServiceResult<T>(error);

        public static new ServiceResult<T> Failed(IDictionary<string, string> errors)
            => new ServiceResult<T>(errors);

        #endregion
    }
}
