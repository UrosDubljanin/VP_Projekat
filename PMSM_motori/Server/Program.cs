using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    internal class Program
    {
        static void Main(string[] args)
        {
            using (ServiceHost host = new ServiceHost(typeof(FileHandlingService)))
            {
                host.Open();
                Console.WriteLine("Konekcija je uspesno otvorena, pritisni bilo koji taste za zatvaranje");
                Console.ReadKey();
                host.Close();
            }
            Console.WriteLine("Servis je zatvoren");
            Console.ReadKey();
        }
    }
}
