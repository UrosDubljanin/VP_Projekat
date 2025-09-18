using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using Common;

namespace Server
{
    internal class Program
    {
        static void Main(string[] args)
        {
            PMSMService service = new PMSMService();

            service.OnTransferStarted += (s, e) =>
            {
                Console.WriteLine($"[EVENT] Transfer started. Profile={e.Meta?.Profile_ID}, PM0={e.Meta?.PM}, Ambient={e.Meta?.Ambient}");
            };
            service.OnSampleReceived += (s, e) =>
            {
                Console.WriteLine($"[EVENT] Sample received. PM={e.Sample.PM:F3}, SW={e.Sample.Stator_Winding:F3}, ST={e.Sample.Stator_Tooth:F3}");
            };
            service.OnWarningRaised += (s, e) =>
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"[WARN] {e.Kind} {e.Direction} | Value={e.Value:F3} Ref={e.Reference:F3}");
                Console.ResetColor();
            };
            service.OnTransferCompleted += (s, e) =>
            {
                Console.WriteLine($"[EVENT] Transfer completed.");
            };

            using (ServiceHost host = new ServiceHost(service))
            {
                host.Open();
                Console.WriteLine("Servis pokrenut. Pritisnite bilo koji taster za izlaz...");
                Console.ReadKey();
                host.Close();
            }
            Console.WriteLine("Servis zaustavljen.");
        }
    }
}
