using System;

namespace CompassionConnectClient
{
    public class CompassionConnectError
    {
        public string ErrorId { get; set; }

        public DateTime ErrorTimestamp { get; set; }

        public string ErrorClass { get; set; }

        public string ErrorCategory { get; set; }

        public string ErrorCode { get; set; }

        public string ErrorMessage { get; set; }

        public bool ErrorRetryable { get; set; }

        public string ErrorModule { get; set; }

        public string ErrorSubModule { get; set; }

        public string ErrorMethod { get; set; }

        public string ErrorLoggedInUser { get; set; }

        public string RelatedRecordId { get; set; }
    }
}