using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    [ServiceContract]
    public interface IPMSMService
    {
        [OperationContract]
        Results StartSession(MetaData meta);
        [OperationContract]
        Results PushSample(MotorSample sample);
        [OperationContract]
        Results EndSession();

    }
}
