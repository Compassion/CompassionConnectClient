using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;

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

        public TResponse Get<TResponse>(string url, IDictionary<string, string> requestParameters) where TResponse : class, new()
        {
            return TryTwiceIfUnauthorised(tokenToUse => MakeCall<TResponse>(url, null, requestParameters, "GET", tokenToUse));
        }

        public TResponse Post<TResponse>(string url, object body, IDictionary<string, string> requestParameters) where TResponse : class, new()
        {
            return TryTwiceIfUnauthorised(tokenToUse => MakeCall<TResponse>(url, body, requestParameters, "POST", tokenToUse));
        }

        public string PostData(string url, Stream dataStream, string contentType, IDictionary<string, string> requestParameters)
        {
            return TryTwiceIfUnauthorised(tokenToUse => SendFile(url, requestParameters, tokenToUse, dataStream, contentType));
        }

        public Stream GetData(string url, IDictionary<string, string> requestParameters) 
        {
            return TryTwiceIfUnauthorised(tokenToUse => GetFile(url, requestParameters, tokenToUse));
        }

        private TResponse MakeCall<TResponse>(string url, object body, IDictionary<string, string> requestParameters, string httpMethod, string tokenToUse) where TResponse : class 
        {
            
            var request = CreateOAuthProtectedRequest(url, requestParameters, tokenToUse, httpMethod, null);

            if (body != null)
            {
                request.ContentType = "application/json";
                var serialisedBody = JsonConvert.SerializeObject(body, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore });
                WriteBody(request, serialisedBody);
            }

            var response = DoRequest(request);
            var result = GetBody<TResponse>(response);
            response.Close();
            return result;
        }

        private void WriteBody(WebRequest request, string serialisedBody)
        {
            var bodyData = Encoding.UTF8.GetBytes(serialisedBody);
            request.ContentLength = bodyData.Length;
            var requestStream = request.GetRequestStream();
            requestStream.Write(bodyData, 0, bodyData.Length);
        }

        private string SendFile(string url, IDictionary<string, string> requestParameters, string oAuthToken, Stream fileStream, string contentType)
        {
            var request = CreateOAuthProtectedRequest(url, requestParameters, oAuthToken, "POST", contentType);

            // if this is the second attempt, restart stream
            if (fileStream.CanSeek && fileStream.Position != 0)
                fileStream.Seek(0, SeekOrigin.Begin);

            //var base64Stream = new CryptoStream(fileStream, new ToBase64Transform(), CryptoStreamMode.Read);
            //base64Stream.CopyTo(request.GetRequestStream());
            fileStream.CopyTo(request.GetRequestStream());

            var response = DoRequest(request);
            var fileUrl = GetBody<string>(response);
            response.Close();
            return fileUrl;
        }

        private Stream GetFile(string url, IDictionary<string, string> requestParameters, string tokenToUse)
        {
            var request = CreateOAuthProtectedRequest(url, requestParameters, tokenToUse, "GET", null);
            var response = DoRequest(request);
            try
            {
                //var decodedBase64Stream = new CryptoStream(response.GetResponseStream(), new FromBase64Transform(), CryptoStreamMode.Read);
                //return decodedBase64Stream;
                return response.GetResponseStream();
            }
            catch(Exception e)
            {
                response.Close();
                throw new RestServiceException(response.StatusCode, response.StatusDescription, null, e);
            }
        }

        private WebRequest CreateOAuthProtectedRequest(string url, IDictionary<string, string> requestParameters, string oAuthToken, string method, string contentType)
        {
            var uri = GetUri(url, requestParameters);
            var request = WebRequest.Create(uri);
            request.ContentType = contentType;
            request.Headers.Add("Authorization", string.Format("Bearer " + oAuthToken));
            request.Method = method;
            return request;
        }

        private string GetUri(string url, IDictionary<string, string> requestParameters)
        {
            var parameters = requestParameters != null ? new Dictionary<string, string>(requestParameters) : new Dictionary<string, string>();
            parameters.Add("api_key", apiKey);
            var paramString = string.Join("&", parameters.Select(kvp => string.Format("{0}={1}", kvp.Key, kvp.Value)));
            return string.Format("{0}?{1}", url, paramString);
        }

        private TResult TryTwiceIfUnauthorised<TResult>(Func<string, TResult> call, bool secondAttempt = false)
        {
            // get token on first call
            if (token == null)
                AcquireToken(token);

            var tokenToUse = token; // Save a copy of token in case it later changes by another thread requesting a new token. Need to know this when aquiring a token.

            try
            {
                return call(tokenToUse);
            }
            catch (RestServiceException rse)
            {
                if (rse.ResponseStatusCode == HttpStatusCode.Unauthorized && !secondAttempt)
                {
                    AcquireToken(tokenToUse);
                    return TryTwiceIfUnauthorised(call, true);
                }
                throw;
            }
        }

        private void AcquireToken(string previousTokenThatWasUsed)
        {
            // Simple attempt at thread-safety. Don't want multiple simultaneous token requests/updates. 
            lock (tokenUpdateLock)
            {
                // Only try to get a new token if we didn't use an out of date copy.
                if (previousTokenThatWasUsed == token)
                {
                    var request = WebRequest.Create(string.Format("{0}token", oAuthUrl));
                    request.ContentType = "application/x-www-form-urlencoded";
                    var authString = Convert.ToBase64String(Encoding.UTF8.GetBytes(string.Format("{0}:{1}", oAuthClientId, oAuthClientSecret)));
                    request.Headers.Add("Authorization", string.Format("Basic {0}", authString));
                    request.Method = "POST";
                    WriteBody(request, "grant_type=client_credentials&scope=read+write");

                    var response = DoRequest(request);
                    var tokenResponse = GetBody<TokenResponse>(response);
                    token = tokenResponse.AccessToken;
                }
            }
        }

        private HttpWebResponse DoRequest(WebRequest request)
        {
            try
            {
                return (HttpWebResponse) request.GetResponse();
            }
            catch (WebException we)
            {
                var response = we.Response as HttpWebResponse;
                
                if (we.Status != WebExceptionStatus.ProtocolError || response == null)
                {
                    if (response != null)
                        response.Close();
                    throw;
                }

                string responseBody = null;
                try
                {
                    responseBody = GetRawBody(response);
                    if (response.StatusCode == HttpStatusCode.BadRequest || response.StatusCode == HttpStatusCode.InternalServerError)
                    {
                        var result = JsonConvert.DeserializeObject<ErrorResponse>(responseBody);
                        throw new CompassionConnectException(result.Error);
                    }
                }
                catch (CompassionConnectException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    throw new RestServiceException(response.StatusCode, response.StatusDescription, responseBody, e);
                }
                finally 
                {
                    response.Close();
                }
                response.Close();
                throw new RestServiceException(response.StatusCode, response.StatusDescription, null, null);
            }
        }

        private string GetRawBody(HttpWebResponse response)
        {
            var streamReader = new StreamReader(response.GetResponseStream());
            var responseBody = streamReader.ReadToEnd();
            response.Close();
            return responseBody;
        }

        private TResult GetBody<TResult>(HttpWebResponse response) where TResult : class 
        {
            string responseBody = null;
            try
            {
                responseBody = GetRawBody(response);
                //if (response.StatusCode != HttpStatusCode.OK && response.StatusCode != HttpStatusCode.Created)
                //    throw new RestServiceException(response.StatusCode, response.StatusDescription, responseBody, null);
                TResult result;
                if (typeof (TResult) != typeof (string))
                    result = JsonConvert.DeserializeObject<TResult>(responseBody);
                else
                    result = responseBody as TResult; // just return string
                return result;
            }
            catch (Exception e)
            {
                throw new RestServiceException(response.StatusCode, response.StatusDescription, responseBody, e);
            }
            finally
            {
                response.Close();
            }
        }
    }
}