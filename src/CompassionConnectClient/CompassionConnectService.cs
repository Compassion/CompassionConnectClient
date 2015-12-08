using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography;
using System.Text;
using CompassionConnectModels.Sbc;

namespace CompassionConnectClient
{
    public class CompassionConnectService : ICompassionConnectService
    {
        private readonly IRestService restService;

        private readonly string baseUrl;
        
        private readonly string testUrl;
        
        public CompassionConnectService(string baseUrl = null, string tokenUrl = null, string testUrl = null, string apiKey = null, string oAuthClientId = null, string oAuthClientSecret = null)
        {
            this.baseUrl = baseUrl ?? ConfigurationManager.AppSettings["CompassionConnectServiceUrl"];
            this.testUrl = testUrl ?? ConfigurationManager.AppSettings["CompassionConnectServiceTestUrl"];
            tokenUrl = tokenUrl ?? ConfigurationManager.AppSettings["CompassionConnectServiceTokenUrl"];
            apiKey = apiKey ?? ConfigurationManager.AppSettings["CompassionConnectServiceApiKey"];
            oAuthClientId = oAuthClientId ?? ConfigurationManager.AppSettings["CompassionConnectServiceOAuthClientId"];
            oAuthClientSecret = oAuthClientSecret ?? ConfigurationManager.AppSettings["CompassionConnectServiceOAuthClientSecret"];
            restService = new RestService(tokenUrl, apiKey, oAuthClientId, oAuthClientSecret);
        }

        internal CompassionConnectService(IRestService restService)
        {
            this.restService = restService;
        }

        public TestResponse Test()
        {
            return restService.Get<TestResponse>(testUrl, "Test", null);
        }

        public CommunicationKitCreateResponse CreateCommunicationKit(CommunicationKit commKit)
        {
            if (commKit.ImplementingChurchPartner == null)
                commKit.ImplementingChurchPartner = new ImplementingChurchPartner();
            if (commKit.FieldOffice == null)
                commKit.FieldOffice = new FieldOffice();

            return restService.Post<CommunicationKitCreateResponses>(baseUrl, "communications", commKit, null).Responses[0];
        }

        public string ImageUpload(Stream fileData, string imageType)
        {
            return restService.PostFile(baseUrl, "images", fileData, imageType, new Dictionary<string, string> { { "doctype", "s2bletter" } });
        }

        //public CommunicationKit GetCommunicationKit(string compassionSbcId)
        //{
        //    return restService.Get<CommunicationKit>(baseUrl, string.Format("communications/{0}", compassionSbcId), null);
        //}

        public byte[] GetImage(string docId, string pageId, string format, int? page)
        {
            var requestParameters = new Dictionary<string, string>();
            if (format != null)
                requestParameters.Add("format", format);
            if (page.HasValue)
                requestParameters.Add("pg", page.ToString());
            var base64EncodedResult = restService.Get(baseUrl, string.Format("images/{0}/page/{1}", docId, pageId), requestParameters);
            var data = Convert.FromBase64String(base64EncodedResult);
            return data;
        }
    }
}