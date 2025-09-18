using Common;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
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
            bool vidiPoruke = false;
            do
            {
                number = PrintMeni();

                switch (number)
                {
                    case 0:
                        Console.WriteLine("Izabrali ste nepostojanu opciju");
                        break;
                    case 1:
                        SlanjePodataka(proxy,vidiPoruke);
                        break;
                    case 2:
                        if (!vidiPoruke)
                        {
                            vidiPoruke = true;
                            Console.ForegroundColor = ConsoleColor.Blue;
                            Console.WriteLine("Poruke CE biti vidljive u toku prenosa.");
                            Console.ResetColor();
                        }
                        else
                        {
                            vidiPoruke = false;
                            Console.ForegroundColor = ConsoleColor.Blue;
                            Console.WriteLine("Poruke NECE biti vidljive u toku prenosa.");
                            Console.ResetColor();
                        }
                        break;
                }


            } while (number != 3);

            return;
        }
        public static int PrintMeni()
        {
            Console.WriteLine("Izaberite opciju:");
            Console.WriteLine("1. Zapocnite sesiju.");
            Console.WriteLine("2. Vidi sve poruke");
            Console.WriteLine("3. Izadjite.");
            try

            {
                int number = int.Parse(Console.ReadLine());
                if (number > 0 && number <= 3)
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

        public static void SlanjePodataka(IPMSMService proxy,bool vidiPoruke)
        {

            try
            {
                string relativePath = ConfigurationManager.AppSettings["DataPath"];
                string fullPath = Path.GetFullPath(relativePath);
                int metaIspisan = 0;

                using (StreamReader sr = new StreamReader(fullPath))
                {
                    Console.WriteLine("=== PMSM Monitoring ===");
                    Console.Write("Status: ");
                    int statusLine = Console.CursorTop;

                    Console.SetCursorPosition(8, statusLine);
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("Slanje u toku...");
                    Console.ResetColor();

                    for (int i = 0; i < 100; i++)
                    {
                        Thread.Sleep(200);

                        string linija = sr.ReadLine();
                        if (i >= 1 && metaIspisan==0)
                        {
                            string poruka = "";
                            MetaData meta;
                            string[] delovi = linija.Split(',');
                            if (TryCreateMeta(delovi,out meta,out poruka))
                            {
                                Results result = proxy.StartSession(meta);
                                if(vidiPoruke)Console.WriteLine($"Poruka: {result.Poruka}, Status: {result.Status},Acknowledgement: {result.Acknowledgement}");
                                metaIspisan = 1;
                            }
                            else
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine($"DataFormatFault: {poruka}");
                                Console.ResetColor();
                            }
                        }
                        else if (i > 1 && metaIspisan==1)
                        {
                            string poruka = "";
                            MotorSample sample;
                            string[] delovi = linija.Split(',');
                            if (TryCreateSample(delovi, out sample, out poruka))
                            {
                                Results result = proxy.PushSample(sample);
                                if (result.validationFault.jeste)
                                {
                                    Console.ForegroundColor = ConsoleColor.Red;
                                    Console.WriteLine($"ValidationFault: Polja: {result.validationFault.Polje}");
                                    Console.WriteLine($"                 Poruke: {result.validationFault.Poruka}");
                                    Console.ResetColor();
                                }
                                if(vidiPoruke)Console.WriteLine($"Poruka: {result.Poruka}, Status: {result.Status},Acknowledgement: {result.Acknowledgement}");
                            }
                            else
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine($"DataFormatFault: {poruka}");
                                Console.ResetColor();
                            }                        
                        }


                    }
                    int nastavak = Console.CursorTop;
                    Console.SetCursorPosition(8, statusLine);
                    Console.Write(new string(' ', 20));
                    Console.SetCursorPosition(8, statusLine);
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Zavrsen prenos!");
                    Console.ResetColor();

                    Console.SetCursorPosition(0, nastavak);
                    Console.WriteLine("");


                    Results r = proxy.EndSession();
                    if(vidiPoruke)Console.WriteLine($"Poruka: {r.Poruka}, Status: {r.Status},Acknowledgement: {r.Acknowledgement}");
                }
            
            }catch (Exception e) 
            { 
                Console.WriteLine(e.Message);
            }
        }
        public static bool TryCreateMeta(string[] delovi, out MetaData meta, out string poruka)
        {
            meta = null;
            poruka = "";

            try
            {
                meta = new MetaData
                {
                    Stator_Winding = double.Parse(delovi[2], CultureInfo.InvariantCulture),
                    Stator_Tooth = double.Parse(delovi[4], CultureInfo.InvariantCulture),
                    Stator_Yoke = double.Parse(delovi[9], CultureInfo.InvariantCulture),
                    PM = double.Parse(delovi[8], CultureInfo.InvariantCulture),
                    Profile_ID = int.Parse(delovi[12]),
                    Ambient = double.Parse(delovi[10], CultureInfo.InvariantCulture),
                    Torque = double.Parse(delovi[11], CultureInfo.InvariantCulture)
                };
                return true;
            }
            catch (FormatException ex)
            {
                poruka = $"Format error: {ex.Message}";
                return false;
            }
            catch (Exception ex)
            {
                poruka = $"Unexpected error: {ex.Message}";
                return false;
            }
        }
        public static bool TryCreateSample(string[] delovi, out MotorSample sample, out string poruka)
        {
            sample = null;
            poruka = "";

            try
            {
                sample = new MotorSample
                {
                    Stator_Winding = double.Parse(delovi[2], CultureInfo.InvariantCulture),
                    Stator_Tooth = double.Parse(delovi[4], CultureInfo.InvariantCulture),
                    Stator_Yoke = double.Parse(delovi[9], CultureInfo.InvariantCulture),
                    PM = double.Parse(delovi[8], CultureInfo.InvariantCulture),
                    Profile_ID = int.Parse(delovi[12]),
                    Ambient = double.Parse(delovi[10], CultureInfo.InvariantCulture),
                    Torque = double.Parse(delovi[11], CultureInfo.InvariantCulture)
                };
                return true;
            }
            catch (FormatException ex)
            {
                poruka = $"Format error: {ex.Message}";
                return false;
            }
            catch (Exception ex)
            {
                poruka = $"Unexpected error: {ex.Message}";
                return false;
            }
        }
    }
}
