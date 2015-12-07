using System;
using System.Net;

namespace CompassionConnectClient
{
    public class RestServiceException : Exception
    {
        public HttpStatusCode ResponseStatusCode { get; set; }

        public string ResponseContent { get; set; }

        public string NonHttpErrorMessage { get; set; }

        public RestServiceException(HttpStatusCode httpStatusCode, string nonHttpErrorMessage, string content, Exception innerException)
            : base(GenerateMessage(httpStatusCode, nonHttpErrorMessage, content), innerException)
        {
            ResponseStatusCode = httpStatusCode;
            ResponseContent = content;
            NonHttpErrorMessage = nonHttpErrorMessage;
        }

        private static string GenerateMessage(HttpStatusCode httpStatusCode, string nonHttpErrorMessage, string content)
        {
            return string.Format(
                "Request Error ({0} {1}{2})\nResponse Content:\n{3}",
                (int)httpStatusCode,
                httpStatusCode.ToString(),
                nonHttpErrorMessage != null ? " - " + nonHttpErrorMessage : string.Empty,
                content);
        }
    }
    }