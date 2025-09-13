using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    [DataContract]
    public class FileManipulationOptions: IDisposable
    {
        public FileManipulationOptions() { }

        public FileManipulationOptions(MemoryStream memoryStream, string keyWord)
        {
            MemoryStream = memoryStream;
            KeyWord = keyWord;
        }
        [DataMember]
        public MemoryStream MemoryStream {  get; set; }
        [DataMember]
        public string KeyWord { get; set; }
        public void Dispose()
        {
            if(MemoryStream == null) return;
            MemoryStream.Dispose();
            MemoryStream = null;
        }
    }
}
