using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    [DataContract]
    public enum ResultType
    {
        [EnumMember]
        Success,
        [EnumMember]
        Warning,
        [EnumMember]
        Failed
    }
    public class FileManipulationResults : IDisposable
    {
        public FileManipulationResults()
        {
            ResultType = ResultType.Success;
            MemoryStreamCollection = new Dictionary<string, MemoryStream>();
        }
        [DataMember]
        public string ResultMessage {  get; set; }

        [DataMember]
        public ResultType ResultType { get; set; }
        [DataMember]
        public Dictionary<string, MemoryStream> MemoryStreamCollection { get; set; }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
