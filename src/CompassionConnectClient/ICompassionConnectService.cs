using System.Collections.Generic;
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
    }
}
