using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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

        public TResponse Get<TResponse>(string baseUrl, string resource, IDictionary<string, string> requestParameters) where TResponse : new()
        {
            return MakeCall<TResponse>(baseUrl, resource, null, requestParameters, Method.GET, false);
        }

        public TResponse Post<TResponse>(string baseUrl, string resource, object body, IDictionary<string, string> requestParameters) where TResponse : new()
        {
            return MakeCall<TResponse>(baseUrl, resource, body, requestParameters, Method.POST, false);
        }

        private TResponse MakeCall<TResponse>(string baseUrl, string resource, object body, IDictionary<string, string> requestParameters, Method httpMethod, bool secondAttempt) where TResponse : new()
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
            request.AddHeader("Accept", "application/json");

            if (body != null)
            {
                request.RequestFormat = DataFormat.Json;
                request.AddBody(body);
            }

            var response = client.Execute(request);

            if (response.StatusCode == HttpStatusCode.Unauthorized && !secondAttempt)
            {
                AcquireToken(tokenToUse);
                return MakeCall<TResponse>(baseUrl, resource, body, requestParameters, httpMethod, true);
            }

            if (response.ErrorException != null)
                throw new RestServiceException(response.StatusCode, response.ErrorMessage, response.Content, response.ErrorException);

            var rawResult = Encoding.UTF8.GetString(response.RawBytes);

            try
            {
                if (response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Created)
                {
                    return JsonConvert.DeserializeObject<TResponse>(rawResult);
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