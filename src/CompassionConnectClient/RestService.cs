using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using RestSharp;
using RestSharp.Authenticators;

namespace CompassionConnectClient
{
    internal class RestService : IRestService
    {
        private readonly string oAuthUrl;

        private readonly string apiKey;

        private readonly string oAuthClientId;
        
        private readonly string oAuthClientSecret;

        private volatile string token;

        private readonly object tokenUpdateLock;

        public RestService(string oAuthUrl, string apiKey, string oAuthClientId, string oAuthClientSecret)
        {
            this.oAuthUrl = oAuthUrl;
            this.apiKey = apiKey;
            this.oAuthClientId = oAuthClientId;
            this.oAuthClientSecret = oAuthClientSecret;

            token = null;
            tokenUpdateLock = new object();
        }

        public TResponse Get<TResponse>(string baseUrl, string resource, IDictionary<string, string> requestParameters) where TResponse : class, new()
        {
            return MakeCall<TResponse>(baseUrl, resource, null, requestParameters, Method.GET, null, null, false);
        }

        public string Get(string baseUrl, string resource, IDictionary<string, string> requestParameters) 
        {
            return MakeCall<string>(baseUrl, resource, null, requestParameters, Method.GET, null, null, false);
        }

        public TResponse Post<TResponse>(string baseUrl, string resource, object body, IDictionary<string, string> requestParameters) where TResponse : class, new()
        {
            return MakeCall<TResponse>(baseUrl, resource, body, requestParameters, Method.POST, null, null, false);
        }

        public string PostFile(string baseUrl, string resource, Stream fileData, string contentType, IDictionary<string, string> requestParameters)
        {
            var base64Stream = new CryptoStream(fileData, new ToBase64Transform(), CryptoStreamMode.Read);
            var memoryStream = new MemoryStream();
            base64Stream.CopyTo(memoryStream);
            var bytes = memoryStream.ToArray();

            return MakeCall<string>(baseUrl, resource, null, requestParameters, Method.POST, bytes, contentType, false);
        }

        private TResponse MakeCall<TResponse>(string baseUrl, string resource, object body, IDictionary<string, string> requestParameters, Method httpMethod, byte[] file, string contentType, bool secondAttempt) where TResponse : class 
        {
            // get token on first call
            if (token == null)
                AcquireToken(token);

            var parameters = requestParameters != null ? new Dictionary<string, string>(requestParameters) : new Dictionary<string, string>();
            parameters.Add("api_key", apiKey);
            var paramString = string.Join("&", parameters.Select(kvp => string.Format("{0}={1}", kvp.Key, kvp.Value)));

            var tokenToUse = token; // Save a copy of token in case it later changes by another thread requesting a new token. Need to know this when aquiring a token.
            var client = new RestClient(baseUrl)
            {
                Authenticator = new OAuth2AuthorizationRequestHeaderAuthenticator(tokenToUse)
            };

            var request = new RestRequest(string.Format("{0}?{1}", resource, paramString), httpMethod);
            client.ClearHandlers();

            if (body != null)
            {
                request.RequestFormat = DataFormat.Json;
                request.AddBody(body);
            }
            if (file != null)
                request.AddParameter(contentType, file, ParameterType.RequestBody);
            if (contentType != null)
                request.AddHeader("Content-Type", contentType);

            var response = client.Execute(request);

            if (response.StatusCode == HttpStatusCode.Unauthorized && !secondAttempt)
            {
                AcquireToken(tokenToUse);
                return MakeCall<TResponse>(baseUrl, resource, body, requestParameters, httpMethod, file, contentType, true);
            }
            
            if (response.ErrorException != null)
                throw new RestServiceException(response.StatusCode, response.ErrorMessage, response.Content, response.ErrorException);

            var rawResult = response.Content;
            try
            {
                if (response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Created)
                {
                    if (typeof(TResponse) != typeof(string))
                        return JsonConvert.DeserializeObject<TResponse>(rawResult);
                    else
                        return rawResult as TResponse;
                }
                if (response.StatusCode == HttpStatusCode.BadRequest || response.StatusCode == HttpStatusCode.InternalServerError)
                {
                    var result = JsonConvert.DeserializeObject<ErrorResponse>(rawResult);
                    throw new CompassionConnectException(result.Error);
                }
            }
            catch (CompassionConnectException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new RestServiceException(response.StatusCode, response.ErrorMessage, response.Content, e);
            }

            throw new RestServiceException(response.StatusCode, response.ErrorMessage, response.Content, null);
        }

        private void AcquireToken(string previousTokenThatWasUsed)
        {
            // Simple attempt at thread-safety. Don't want multiple simultaneous token requests/updates. 
            lock (tokenUpdateLock)
            {
                // Only try to get a new token if we didn't use an out of date copy.
                if (previousTokenThatWasUsed == token)
                {
                    var client = new RestClient(oAuthUrl)
                    {
                        Authenticator = new HttpBasicAuthenticator(oAuthClientId, oAuthClientSecret)
                    };
                    var request = new RestRequest("token", Method.POST);
                    request.AddParameter("application/x-www-form-urlencoded", "grant_type=client_credentials&scope=read+write", ParameterType.RequestBody);
                    var response = client.Execute<TokenResponse>(request);
                    if (response.StatusCode != HttpStatusCode.OK || response.ErrorException != null)
                        throw new RestServiceException(response.StatusCode, response.ErrorMessage, response.Content, response.ErrorException);
                    token = response.Data.AccessToken;
                }
            }
        }
    }
}