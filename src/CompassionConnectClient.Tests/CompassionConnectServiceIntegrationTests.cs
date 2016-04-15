using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Text;
using System.Threading.Tasks;
using CompassionConnectModels.Sbc;
using NUnit.Framework;

namespace CompassionConnectClient.Tests
{
    [TestFixture]
    [Ignore("Integration Tests")]
    public class CompassionConnectServiceIntegrationTests
    {
        const string BaseUrl = "https://api2.compassion.com/test/ci/v2/";
        const string TestUrl = "https://api2.compassion.com/TEST/CI/1/";
        const string AuthUrl = "https://api2.compassion.com/core/connect/";
        const string ApiKey = "";
        const string OAuthClientId = "";
        const string AuthClientSecret = "";

        private CompassionConnectService compassionConnectService;

        [SetUp]
        public void SetUp()
        {
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; }; // So Fiddler can be used without getting SSL errors
            compassionConnectService = new CompassionConnectService(BaseUrl, AuthUrl, TestUrl, ApiKey, OAuthClientId, AuthClientSecret);
        }

        [Test]
        public void TestOAuthProtectedResource()
        {
            var result = compassionConnectService.Test();
        }

        [Test]
        public void TestCommKitCreate()
        {
            var commKit = new CommunicationKit
            {
                Beneficiary = new Beneficiary
                {
                    LocalId = "EC5500279"
                },

                GlobalPartner = new GlobalPartner { Id = "AU" },
                Direction = "Supporter To Beneficiary",
                Pages = new List<Page>
                {
                    new Page { EnglishTranslatedText = new List<string> { "Hello", "Write to me soon please" }, OriginalText = new List<string>() { "Bon jour", "écrivez-moi s'il vous plaît bientôt" } },
                    new Page { EnglishTranslatedText = new List<string> { "Hello", "Send pictures please" }, OriginalText = new List<string>() { "Bon jour", "Envoyer des photos s'il vous plaît" } },
                },
                RelationshipType = "Sponsor",
                SBCGlobalStatus = "Received in the system",
                GlobalPartnerSBCId = "B537A557-04A4-4622-A233-E3628BC445F0",
                OriginalLanguage = "French",
                OriginalLetterURL = "https://api2.compassion.com/stage/ci/v2/images/321YZDQ_0107KC642000001/page/321YZDQ_0107KC642000005.tif",
                SourceSystem = "CRM",
                Template = "FR-A-1S11-1",
                Supporter = new Supporter { CompassConstituentId = "16-1101861", GlobalId = null },
            };

            var result = compassionConnectService.CreateCommunicationKit(commKit);
        }

        //[Test]
        //public void TestGetCommKit()
        //{
        //    var result = compassionConnectService.GetCommunicationKit("C0000001527");
        //}

        [Test]
        public void UploadImage()
        {
            var result = compassionConnectService.ImageUpload(@"", UploadFormat.Pdf);
        }

        [Test]
        public void GetImageFromDocIdAndPageId()
        {
            const string filePath = @"";
            if (File.Exists(filePath))
                File.Delete(filePath);
            compassionConnectService.ImageDownloadToFile(filePath, "321YZDG_00XYX9QVB00005V", "321YZDG_00XZ29R9Y00001T.tif", DownloadFormat.Pdf);
        }

        [Test]
        public void GetImageFromCompleteUrl()
        {
            const string filePath = @"";
            if (File.Exists(filePath))
                File.Delete(filePath);
            compassionConnectService.ImageDownloadToFile(filePath, "", DownloadFormat.Pdf);
        }
    }
}
