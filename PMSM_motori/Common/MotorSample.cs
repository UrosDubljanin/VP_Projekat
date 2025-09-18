using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    [DataContract]
    public class MotorSample:IDisposable
    {
        public MotorSample() {
            Stator_Winding = 0;
            Stator_Tooth = 0;
            Stator_Yoke = 0;
            PM = 0;
            Profile_ID = 0;
            Ambient = 0;
            Torque = 0;
        }

        public MotorSample(double stator_Winding, double stator_Tooth, double stator_Yoke, double pM, int profile_ID, double ambient, double torque)
        {
            Stator_Winding = stator_Winding;
            Stator_Tooth = stator_Tooth;
            Stator_Yoke = stator_Yoke;
            PM = pM;
            Profile_ID = profile_ID;
            Ambient = ambient;
            Torque = torque;
        }
        [DataMember] public double Stator_Winding { get; set; }
        [DataMember] public double Stator_Tooth { get; set; }
        [DataMember] public double Stator_Yoke { get; set; }
        [DataMember] public double PM { get; set; }
        [DataMember] public int Profile_ID { get; set; }
        [DataMember] public double Ambient { get; set; }
        [DataMember] public double Torque { get; set; }

        public void Dispose()
        {
            //throw new NotImplementedException();
        }
    }
}
