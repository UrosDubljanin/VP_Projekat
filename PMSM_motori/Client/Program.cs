using Common;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Client
{
    internal class Program
    {
        static void Main(string[] args)
        {
            ChannelFactory<IPMSMService> factory = new ChannelFactory<IPMSMService> ("PMSMService");
            IPMSMService proxy = factory.CreateChannel();

            int number = 0;

            do
            {
                number = PrintMeni();

                switch (number)
                {
                    case 0:
                        Console.WriteLine("Izabrali ste nepostojanu opciju");
                        break;
                    case 1:
                        SlanjePodataka(proxy);
                        break;
                }


            } while (number != 2);

            return;
        }
        public static int PrintMeni()
        {
            Console.WriteLine("Izaberite opciju:");
            Console.WriteLine("1. Zapocnite sesiju.");
            Console.WriteLine("2. Izadjite.");

            try
            {
                int number = int.Parse(Console.ReadLine());
                if (number > 0 && number <= 2)
                {
                    return number;
                }
                else
                {
                    return 0;
                }

            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        public static void SlanjePodataka(IPMSMService proxy)
        {

            try
            {
                string relativePath = ConfigurationManager.AppSettings["DataPath"];
                string fullPath = Path.GetFullPath(relativePath);

                using (StreamReader sr = new StreamReader(fullPath))
                {

                    for (int i = 0; i < 102; i++)
                    {

                        string linija = sr.ReadLine();
                        if (i == 1)
                        {
                            string[] delovi = linija.Split(',');
                            MetaData meta = new MetaData(Double.Parse(delovi[2]), Double.Parse(delovi[4]), Double.Parse(delovi[9]), Double.Parse(delovi[8]), int.Parse(delovi[12]), Double.Parse(delovi[10]), Double.Parse(delovi[11]));
                            Results result = proxy.StartSession(meta);
                            Console.WriteLine($"Poruka: {result.Poruka}, Status: {result.Status},Acknowledgement: {result.Acknowledgement}");
                        }
                        else if (i > 1)
                        {
                            string[] delovi = linija.Split(',');
                            MotorSample sample = new MotorSample(Double.Parse(delovi[2]), Double.Parse(delovi[4]), Double.Parse(delovi[9]), Double.Parse(delovi[8]), int.Parse(delovi[12]), Double.Parse(delovi[10]), Double.Parse(delovi[11]));
                            Results result = proxy.PushSample(sample);
                            Console.WriteLine($"Poruka: {result.Poruka}, Status: {result.Status},Acknowledgement: {result.Acknowledgement}");
                        }


                    }
                    Results r = proxy.EndSession();
                    Console.WriteLine($"Poruka: {r.Poruka}, Status: {r.Status},Acknowledgement: {r.Acknowledgement}");
                }
            
            }catch (Exception e) 
            { 
                Console.WriteLine(e.Message);
            }
        }
    }
}
