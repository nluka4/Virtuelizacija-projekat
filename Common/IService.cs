using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    [ServiceContract(SessionMode = SessionMode.Required)]
    public interface IService
    {
        [OperationContract]
        Response StartSession(SessionMeta meta);

        [OperationContract]
        Response PushSample(Sample sample);

        [OperationContract]
        Response EndSession();
    }
}
