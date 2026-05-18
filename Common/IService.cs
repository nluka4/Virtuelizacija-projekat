using System.ServiceModel;

namespace Common
{
    [ServiceContract(SessionMode = SessionMode.Required)]
    public interface IService
    {
        [OperationContract]
        [FaultContract(typeof(ValidationFault))]
        [FaultContract(typeof(DataFormatFault))]
        Response StartSession(SessionMeta meta);

        [OperationContract]
        [FaultContract(typeof(ValidationFault))]
        [FaultContract(typeof(DataFormatFault))]
        Response PushSample(Sample sample);

        [OperationContract]
        [FaultContract(typeof(ValidationFault))]
        Response EndSession();
    }
}