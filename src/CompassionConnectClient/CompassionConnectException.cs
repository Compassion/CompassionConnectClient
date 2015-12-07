using System;

namespace CompassionConnectClient
{
    public class CompassionConnectException : Exception
    {
        public CompassionConnectException()
        {
        }

        public CompassionConnectException(CompassionConnectError error) 
            : base(string.Format("{0} - {1}", error.ErrorCategory, error.ErrorMessage))
        {
            ErrorId = error.ErrorId;
            ErrorTimestamp = error.ErrorTimestamp;
            ErrorClass = error.ErrorClass;
            ErrorCategory = error.ErrorCategory;
            ErrorCode = error.ErrorCode;
            ErrorMessage = error.ErrorMessage;
            ErrorRetryable = error.ErrorRetryable;
            ErrorModule = error.ErrorModule;
            ErrorSubModule = error.ErrorSubModule;
            ErrorMethod = error.ErrorMethod;
            ErrorLoggedInUser = error.ErrorLoggedInUser;
            RelatedRecordId = error.RelatedRecordId;
        }

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