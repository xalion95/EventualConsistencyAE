using System.Collections.Generic;
using System.ServiceModel;
using Service.Model;

namespace Service.Api
{
    [ServiceContract]
    public interface IEAService
    {
        [OperationContract(IsOneWay = true)]
        void Connect(int clientPort);

        [OperationContract(IsOneWay = true)]
        void Disconnect();

        [OperationContract(IsOneWay = true)]
        void AddPersons(List<Person> persons);
    }
}