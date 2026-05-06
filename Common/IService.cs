using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    [ServiceContract]
    public interface IService
    {
        [OperationContract]
        void PushSample(Sample sample);

        [OperationContract]
        void StartSession(Sample meta);
        
        [OperationContract]
        void EndSession();
    }
}
