using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CompassionConnectModels.Sbc;

namespace CompassionConnectClient
{
    public interface ICompassionConnectService
    {
        TestResponse Test();

        CommunicationKitCreateResponse CreateCommunicationKit(CommunicationKit commKit);

        //CommunicationKit GetCommunicationKit(string compassionSbcId);

        string ImageUpload(Stream imageData, UploadFormat imageType);

        string ImageUpload(string filePath, UploadFormat imageType);

        Stream ImageDownload(string docId, string pageId, DownloadFormat? format = null, int? page = null);

        Stream ImageDownload(string imageUrl, DownloadFormat? format = null, int? page = null);

        void ImageDownloadToFile(string filePath, string docId, string pageId, DownloadFormat? format = null, int? page = null);

        void ImageDownloadToFile(string filePath, string imageUrl, DownloadFormat? format = null, int? page = null);
    }
}
