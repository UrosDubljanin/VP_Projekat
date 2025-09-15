using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    [DataContract]
    public enum AcknowledgementType
    {
        [EnumMember]
        ACK,
        [EnumMember]
        NACK
    }
    public enum StatusType
    {
        [EnumMember]
        IN_PROGRESS,
        [EnumMember]
        COMPLETED
    }
    public class Results:IDisposable
    {
        public Results()
        {
            ValidationFault validationFault = new ValidationFault();
            AcknowledgementType Acknowledgement = AcknowledgementType.NACK;
            StatusType Status = StatusType.COMPLETED;
            string Poruka = "";
        }
        [DataMember]
        public ValidationFault validationFault {  get; set; }
        [DataMember]
        public AcknowledgementType Acknowledgement { get; set; }
        [DataMember]
        public StatusType Status { get; set; }
        [DataMember]
        public string Poruka { get; set; }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
