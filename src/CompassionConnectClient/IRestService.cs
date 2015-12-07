using System.Collections.Generic;

namespace CompassionConnectClient
{
    internal interface IRestService
    {
        TResponse Get<TResponse>(string baseUrl, string resource, IDictionary<string, string> requestParameters) where TResponse : new();

        TResponse Post<TResponse>(string baseUrl, string resource, object body, IDictionary<string, string> requestParameters) where TResponse : new();
    }
}