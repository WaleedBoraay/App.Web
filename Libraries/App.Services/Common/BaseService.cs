using System.Collections.Generic;

namespace App.Services.Common
{
    /// <summary>
    /// Provides common helpers for service implementations
    /// </summary>
    public abstract class BaseService
    {
        #region Non-generic results

        protected ServiceResult Success(string message = null)
            => ServiceResult.SuccessResult(message);

        protected ServiceResult Failed(string error)
            => ServiceResult.Failed(error);

        protected ServiceResult Failed(IDictionary<string, string> errors)
            => ServiceResult.Failed(errors);

        #endregion

        #region Generic results

        protected ServiceResult<T> Success<T>(T data, string message = null)
            => ServiceResult<T>.Success(data, message);

        protected ServiceResult<T> Failed<T>(string error)
            => ServiceResult<T>.Failed(error);

        protected ServiceResult<T> Failed<T>(IDictionary<string, string> errors)
            => ServiceResult<T>.Failed(errors);

        #endregion
    }
}
