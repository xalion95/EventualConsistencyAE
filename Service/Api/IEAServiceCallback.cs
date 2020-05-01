using System.Collections.Generic;
using System.ServiceModel;
using Service.Model;

namespace Service.Api
{
    [ServiceContract]
    public interface IEAServiceCallback
    {
        [OperationContract(IsOneWay = true)]
        void Disconnect();

        [OperationContract(IsOneWay = true)]
        void Synchronize(List<Person> updatedPersons);
    }
}