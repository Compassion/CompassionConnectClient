using System.Collections.Generic;
using System.IO;
using CompassionConnectModels.Sbc;
using NUnit.Framework;
using Rhino.Mocks;

namespace CompassionConnectClient.Tests
{
    [TestFixture]
    public class CompassionConnectTests
    {
        private const string BaseUrl = "https://base.url.com/";

        private const string TestUrl = "https://test.url.com/";

        [Test]
        public void Test()
        {
            var expectedResponse = new TestResponse();

            var restService = MockRepository.GenerateMock<IRestService>();
            restService.Expect(r => r.Get<TestResponse>(TestUrl + "Test", null))
                .Return(expectedResponse);

            var compassionConnectService = new CompassionConnectService(restService, BaseUrl, TestUrl);
            var response = compassionConnectService.Test();

            Assert.That(response, Is.EqualTo(expectedResponse));
            restService.VerifyAllExpectations();
        }

        [Test]
        public void CreateCommunicationKit()
        {
            var commKit = new CommunicationKit();
            var expectedResponse = new CommunicationKitCreateResponses { Responses = new List<CommunicationKitCreateResponse>() { new CommunicationKitCreateResponse() }};

            var restService = MockRepository.GenerateMock<IRestService>();
            restService.Expect(r => r.Post<CommunicationKitCreateResponses>(BaseUrl + "communications", commKit, null))
                .Return(expectedResponse);

            var compassionConnectService = new CompassionConnectService(restService, BaseUrl, TestUrl);
            var response = compassionConnectService.CreateCommunicationKit(commKit);

            Assert.That(response, Is.EqualTo(expectedResponse.Responses[0]));
            restService.VerifyAllExpectations();
        }

        [Test]
        public void ImageUpload_Tiff()
        {
            const string expectedResponse = "https://base.url.com/new/file";
            Dictionary<string, string> requestParameters = null;

            var stream = MockRepository.GenerateStub<Stream>();
            var restService = MockRepository.GenerateMock<IRestService>();
            restService.Expect(r => r.PostData(Arg<string>.Is.Equal(BaseUrl + "images"), Arg<Stream>.Is.Equal(stream), Arg<string>.Is.Equal("image/tiff"), Arg<Dictionary<string, string>>.Is.Anything))
                .WhenCalled(m => requestParameters = m.Arguments[3] as Dictionary<string, string>)
                .Return(expectedResponse);

            var compassionConnectService = new CompassionConnectService(restService, BaseUrl, TestUrl);
            var response = compassionConnectService.ImageUpload(stream, UploadFormat.Tiff);

            Assert.That(response, Is.EqualTo(expectedResponse));
            Assert.That(requestParameters.Count, Is.EqualTo(1));
            Assert.That(requestParameters["doctype"], Is.EqualTo("s2bletter"));
            restService.VerifyAllExpectations();
        }

        [Test]
        public void ImageUpload_Pdf()
        {
            const string expectedResponse = "https://base.url.com/new/file";
            Dictionary<string, string> requestParameters = null;

            var stream = MockRepository.GenerateStub<Stream>();
            var restService = MockRepository.GenerateMock<IRestService>();
            restService.Expect(r => r.PostData(Arg<string>.Is.Equal(BaseUrl + "images"), Arg<Stream>.Is.Equal(stream), Arg<string>.Is.Equal("image/pdf"), Arg<Dictionary<string, string>>.Is.Anything))
                .WhenCalled(m => requestParameters = m.Arguments[3] as Dictionary<string, string>)
                .Return(expectedResponse);

            var compassionConnectService = new CompassionConnectService(restService, BaseUrl, TestUrl);
            var response = compassionConnectService.ImageUpload(stream, UploadFormat.Pdf);

            Assert.That(response, Is.EqualTo(expectedResponse));
            Assert.That(requestParameters.Count, Is.EqualTo(1));
            Assert.That(requestParameters["doctype"], Is.EqualTo("s2bletter"));
            restService.VerifyAllExpectations();
        }

        [Test]
        public void ImageDownload_NoOptionalParameters()
        {
            const string docId = "docId";
            const string pageId = "pageId";

            Dictionary<string, string> requestParameters = null;

            var stream = MockRepository.GenerateStub<Stream>();
            var restService = MockRepository.GenerateStrictMock<IRestService>();
            restService.Expect(r => r.GetData(Arg<string>.Is.Equal(BaseUrl + "images/docId/page/pageId"), Arg<Dictionary<string, string>>.Is.Anything))
                .WhenCalled(m => requestParameters = m.Arguments[1] as Dictionary<string, string>)
                .Return(stream);

            var compassionConnectService = new CompassionConnectService(restService, BaseUrl, TestUrl);
            var response = compassionConnectService.ImageDownload(docId, pageId);

            restService.VerifyAllExpectations();
            Assert.That(response, Is.EqualTo(stream));
            Assert.That(requestParameters.Count, Is.EqualTo(0));
        }

        [Test]
        public void ImageDownload_WithOptionalParameters()
        {
            const string docId = "docId";
            const string pageId = "pageId";

            Dictionary<string, string> requestParameters = null;

            var stream = MockRepository.GenerateStub<Stream>();
            var restService = MockRepository.GenerateMock<IRestService>();
            restService.Expect(r => r.GetData(Arg<string>.Is.Equal(BaseUrl + "images/docId/page/pageId"), Arg<Dictionary<string, string>>.Is.Anything))
                .WhenCalled(m => requestParameters = m.Arguments[1] as Dictionary<string, string>)
                .Return(stream);

            var compassionConnectService = new CompassionConnectService(restService, BaseUrl, TestUrl);
            var response = compassionConnectService.ImageDownload(docId, pageId, DownloadFormat.Png, 3);

            Assert.That(response, Is.EqualTo(stream));
            Assert.That(requestParameters.Count, Is.EqualTo(2));
            Assert.That(requestParameters["format"], Is.EqualTo("png"));
            Assert.That(requestParameters["pg"], Is.EqualTo("3"));
            restService.VerifyAllExpectations();
        }

        [Test]
        public void ImageDownload_WithOptionalParameters_TiffReplacedWithTif()
        {
            const string docId = "docId";
            const string pageId = "pageId";

            Dictionary<string, string> requestParameters = null;

            var stream = MockRepository.GenerateStub<Stream>();
            var restService = MockRepository.GenerateMock<IRestService>();
            restService.Expect(r => r.GetData(Arg<string>.Is.Equal(BaseUrl + "images/docId/page/pageId"), Arg<Dictionary<string, string>>.Is.Anything))
                .WhenCalled(m => requestParameters = m.Arguments[1] as Dictionary<string, string>)
                .Return(stream);

            var compassionConnectService = new CompassionConnectService(restService, BaseUrl, TestUrl);
            var response = compassionConnectService.ImageDownload(docId, pageId, DownloadFormat.Tiff, 3);

            Assert.That(response, Is.EqualTo(stream));
            Assert.That(requestParameters.Count, Is.EqualTo(2));
            Assert.That(requestParameters["format"], Is.EqualTo("tif"));
            Assert.That(requestParameters["pg"], Is.EqualTo("3"));
            restService.VerifyAllExpectations();
        }
    }
}