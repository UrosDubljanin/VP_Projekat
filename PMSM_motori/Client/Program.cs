using Common;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    internal class Program
    {
        static void Main(string[] args)
        {
            ChannelFactory<IFileHandling> channelFactory = new ChannelFactory<IFileHandling>("FileHandlingService");
            IFileHandling proxy = channelFactory.CreateChannel();

            int number = 0;
            do
            {
                number = PrintMeni();
                switch (number)
                {
                    case 0:
                        Console.WriteLine("Izaberite drugi broj opcija nije validna");
                        break;
                    case 1:
                        SendFile(proxy);
                        break;


                }
            } while (number != 3);

        }
        public static int PrintMeni()
        {
            Console.WriteLine("Opcije:");
            Console.WriteLine("Opcija 1: Posalji fajl");
            Console.WriteLine("Opcija 2: Primi fajlove");
            Console.WriteLine("Opcija 3: Izadji");
            Console.Write("Unesi opciju:");
            try
            {
                int number = Int32.Parse(Console.ReadLine());
                Console.WriteLine();
                if (number >= 1 && number <= 3)
                {
                    return number;
                }
                return 0;
            }
            catch (Exception ex)
            {
                return 0;
            }
        }
        public static void SendFile(IFileHandling proxy)
        {
            Console.WriteLine("Unesi naziv trazenog fajla:");
            var fileName = Console.ReadLine();

            string path = ConfigurationManager.AppSettings["path"];
            if (path == null)
            {
                Console.WriteLine("Putanja na klijentu nije definisana");
                return;
            }
            if (!Directory.Exists(path))
            {
                Console.WriteLine("Folder na klijentu ne postoji");
                return;
            }
            string fullPath = Path.Combine(path, fileName);
            MemoryStream memoryStream = new MemoryStream();
            try
            {
                using (FileStream fs = new FileStream(fullPath, FileMode.Open, FileAccess.Read))
                {
                    fs.CopyTo(memoryStream);
                }
                FileManipulationResults result = proxy.SendFile(new FileManipulationOptions(memoryStream, fileName));
                Console.WriteLine(result.ResultMessage);

            }
            catch (IOException ex)
            {
                Console.WriteLine(ex.Message);
                return;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return;
            }

        }
    }
}
