using System.Collections.Generic;
using System.IO;

namespace CompassionConnectClient
{
    internal interface IRestService
    {
        TResponse Get<TResponse>(string baseUrl, string resource, IDictionary<string, string> requestParameters) where TResponse : class, new(); 

        TResponse Post<TResponse>(string baseUrl, string resource, object body, IDictionary<string, string> requestParameters) where TResponse : class, new();

        string PostData(string baseUrl, string resource, Stream dataStream, string contentType, IDictionary<string, string> requestParameters);

        Stream GetData(string baseUrl, string resource, IDictionary<string, string> requestParameters);
    }
}