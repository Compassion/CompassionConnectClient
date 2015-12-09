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

        internal CompassionConnectService(IRestService restService, string baseUrl, string testUrl)
        {
            this.baseUrl = baseUrl;
            this.testUrl = testUrl;
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

        public string ImageUpload(Stream imageData, UploadFormat imageType)
        {
            var contentType = "image/" + imageType.ToString().ToLower();
            return restService.PostData(baseUrl, "images", imageData, contentType, new Dictionary<string, string> { { "doctype", "s2bletter" } });
        }

        public string ImageUpload(string filePath, UploadFormat imageType)
        {
            using (var stream = File.OpenRead(filePath))
            {
                return ImageUpload(stream, imageType);
            }
        }

        //public CommunicationKit GetCommunicationKit(string compassionSbcId)
        //{
        //    return restService.Get<CommunicationKit>(baseUrl, string.Format("communications/{0}", compassionSbcId), null);
        //}

        public Stream ImageDownload(string docId, string pageId, DownloadFormat? format = null, int? page = null)
        {
            var requestParameters = new Dictionary<string, string>();
            if (format.HasValue)
            {
                var formatString = format.ToString().ToLower().Replace("tiff", "tif");
                requestParameters.Add("format", formatString);
            }
            if (page.HasValue)
                requestParameters.Add("pg", page.ToString());
            return restService.GetData(baseUrl, string.Format("images/{0}/page/{1}", docId, pageId), requestParameters);
        }

        public void ImageDownloadToFile(string filePath, string docId, string pageId, DownloadFormat? format = null, int? page = null)
        {
            using (var imageStream = ImageDownload(docId, pageId, format, page))
            {
                using (var fileStream = File.Open(filePath, FileMode.CreateNew, FileAccess.Write))
                {
                    imageStream.CopyTo(fileStream);
                }
            }
        }
    }
}