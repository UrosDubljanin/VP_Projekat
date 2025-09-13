using Common;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public class FileHandlingService : IFileHandling
    {
        [OperationBehavior(AutoDisposeParameters=true)]
        public FileManipulationResults GetFile(FileManipulationOptions options)
        {
            throw new NotImplementedException();
        }
        [OperationBehavior(AutoDisposeParameters = true)]
        public FileManipulationResults SendFile(FileManipulationOptions options)
        {
            FileManipulationResults results=new FileManipulationResults();
            try
            {
                var path = ConfigurationManager.AppSettings["pat"];
                if (path == null)
                {
                    results.ResultMessage = "Nije definisana putanja.";
                    results.ResultType = ResultType.Warning;
                    return results;
                }
                if (!Directory.Exists(path))
                {
                    results.ResultMessage = "Folder ne postoji";
                    results.ResultType = ResultType.Warning;
                    return results;
                }
                if (options.MemoryStream == null|| options.MemoryStream.Length==0)
                {
                    results.ResultMessage = "Fajl je prazan";
                    results.ResultType = ResultType.Warning;
                    return results;
                }
                var fullPath = Path.Combine(path, options.KeyWord);
                using(FileStream fs =new FileStream(fullPath, FileMode.Create, FileAccess.Write))
                {
                    options.MemoryStream.WriteTo(fs);
                }
                results.ResultMessage = "Fajl je uspesno sacuvan.";
                results.ResultType = ResultType.Success;
                return results;
            }
            catch (Exception ex) {
                results.ResultMessage = ex.Message;
                results.ResultType= ResultType.Failed;
                return results;
            }
        }
    }
}
