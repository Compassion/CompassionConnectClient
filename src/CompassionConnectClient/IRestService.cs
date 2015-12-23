using System.Collections.Generic;
using System.IO;

namespace CompassionConnectClient
{
    internal interface IRestService
    {
        TResponse Get<TResponse>(string url, IDictionary<string, string> requestParameters) where TResponse : class, new(); 

        TResponse Post<TResponse>(string url, object body, IDictionary<string, string> requestParameters) where TResponse : class, new();

        string PostData(string url, Stream dataStream, string contentType, IDictionary<string, string> requestParameters);

        Stream GetData(string url, IDictionary<string, string> requestParameters);
    }
}