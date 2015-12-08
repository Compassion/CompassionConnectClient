using System.Collections.Generic;
using System.IO;

namespace CompassionConnectClient
{
    internal interface IRestService
    {
        TResponse Get<TResponse>(string baseUrl, string resource, IDictionary<string, string> requestParameters) where TResponse : class, new();

        string Get(string baseUrl, string resource, IDictionary<string, string> requestParameters);

        TResponse Post<TResponse>(string baseUrl, string resource, object body, IDictionary<string, string> requestParameters) where TResponse : class, new();

        string PostFile(string baseUrl, string resource, Stream fileData, string contentType, IDictionary<string, string> requestParameters);
    }
}