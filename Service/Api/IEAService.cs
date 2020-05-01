using System.Collections.Generic;
using System.IO;
using System.ServiceModel;

namespace Service.Api
{
    [ServiceContract(CallbackContract = typeof(IEAServiceCallback))]
    public interface IEAService
    {
        [OperationContract]
        void Connect();

        [OperationContract(IsOneWay = true)]
        void Disconnect();
    }
}