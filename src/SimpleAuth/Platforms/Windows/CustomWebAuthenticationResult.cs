using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Security.Authentication.Web;

namespace SimpleAuth.UWP
{
    /// <summary>
    /// Indicates the result of the authentication operation.
    /// </summary>
    // https://github.com/inthehand/Authful/blob/master/WebAuthenticationResult.cs
    public sealed class WebAuthenticationResult
    {
        internal WebAuthenticationResult(string response, uint errorDetail, WebAuthenticationStatus responseStatus)
        {
            ResponseData = response;
            ResponseErrorDetail = errorDetail;
            ResponseStatus = responseStatus;
        }

        /// <summary>
        /// Contains the protocol data when the operation successfully completes.
        /// </summary>
        public string ResponseData { get; private set; }

        /// <summary>
        /// Returns the HTTP error code when <see cref="ResponseStatus"/> is equal to <see cref="WebAuthenticationStatus.ErrorHttp"/>. 
        /// This is only available if there is an error.
        /// </summary>
        public uint ResponseErrorDetail { get; private set; }

        /// <summary>
        /// Contains the status of the asynchronous operation when it completes.
        /// </summary>
        public WebAuthenticationStatus ResponseStatus { get; private set; }
    }
}
