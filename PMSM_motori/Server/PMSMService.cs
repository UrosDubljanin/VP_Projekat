using Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public class PMSMService : IPMSMService
    {
        private static bool SessiaUToku = false;
        private static string validniPath = "measurements_session.csv";
        private static string nevalidniPath = "rejects.csv";
        private static StreamWriter validniPodaci;
        private static StreamWriter nevalidniPodaci;
        public Results EndSession()
        {
            try
            {
                if (!SessiaUToku) {
                    Results r = new Results();
                    r.Status = StatusType.COMPLETED;
                    r.Poruka = "Sesija je vec zavrsena.";
                    return r;
                }
                if (validniPodaci != null)
                {
                    validniPodaci.Flush();
                    validniPodaci.Close();
                    validniPodaci.Dispose();
                    validniPodaci = null;
                }
                if (nevalidniPodaci != null)
                {
                    nevalidniPodaci.Flush();
                    nevalidniPodaci.Close();
                    nevalidniPodaci.Dispose();
                    nevalidniPodaci = null;
                }

                SessiaUToku = false;

                Results result = new Results();
                result.Status = StatusType.COMPLETED;
                result.Poruka = "Sesija je vec zavrsena.";
                return result;


            }
            catch (Exception ex)
            {
                Results result = new Results();
                result.Status = StatusType.COMPLETED;
                result.Poruka = "Greska prilikom zavrsavanja sesije.";
                return result;
            }
        }

        public Results PushSample(MotorSample sample)
        {
            try
            {
                if (!SessiaUToku)
                {
                    Results result = new Results();
                    result.Status = StatusType.COMPLETED;
                    result.Poruka = "Sesija nije pocela, mora poceti da bi se pushovali samplovi.";
                    return result;
                }
                if (validniPath == null)
                {
                    Results result = new Results();
                    result.Status = StatusType.COMPLETED;
                    result.Poruka = "Fajl za validne podatke nije otvoren.";
                    return result;
                }
                if (nevalidniPath == null)
                {
                    Results result = new Results();
                    result.Status = StatusType.COMPLETED;
                    result.Poruka = "Fajl za nevalidne podatke nije otvoren.";
                    return result;
                }

                List<string> greske = ProveraValidnosti(sample);

                if (greske.Count == 0)
                {
                    validniPodaci.WriteLine($"{sample.Stator_Winding},{sample.Stator_Tooth},{sample.Stator_Yoke},{sample.PM},{sample.Profile_ID},{sample.Ambient},{sample.Torque}");
                    validniPodaci.Flush();

                    Results result = new Results();
                    result.Acknowledgement = AcknowledgementType.ACK;
                    result.Status = StatusType.IN_PROGRESS;
                    result.Poruka = "Red je validan i uspesno pushovan.";
                    return result;

                }
                else
                {
                    nevalidniPodaci.WriteLine($"{sample.Stator_Winding},{sample.Stator_Tooth},{sample.Stator_Yoke},{sample.PM},{sample.Profile_ID},{sample.Ambient},{sample.Torque}");
                    nevalidniPodaci.Flush();

                    Results result = new Results();
                    result.Acknowledgement = AcknowledgementType.NACK;
                    result.Status = StatusType.IN_PROGRESS;
                    result.Poruka = $"Red je nevalidan: {string.Join(";",greske)}";
                    return result;
                }
            }
            catch(Exception ex) 
            {
                Results result = new Results();
                result.Status = StatusType.COMPLETED;
                result.Poruka = "Greska prilikom pushovanja samplova.";
                return result;
            }

        }

        public Results StartSession(MotorSample meta)
        {
            try
            {
                if (SessiaUToku)
                {
                    Results r= new Results();
                    r.Status = StatusType.IN_PROGRESS;
                    r.Poruka = "Sesija je vec pokrenuta.";
                    return r;
                }

                validniPodaci = new StreamWriter(validniPath, false);
                nevalidniPodaci = new StreamWriter(nevalidniPath, false);

                validniPodaci.WriteLine("Stator_Winding,Stator_Tooth,Stator_Yoke,PM,Profile_Id,Ambient,Torque");
                nevalidniPodaci.WriteLine("Stator_Winding,Stator_Tooth,Stator_Yoke,PM,Profile_Id,Ambient,Torque");

                validniPodaci.Flush();
                nevalidniPodaci.Flush();

                SessiaUToku = true;

                validniPodaci.WriteLine($"{meta.Stator_Winding},{meta.Stator_Tooth},{meta.Stator_Yoke},{meta.PM},{meta.Profile_ID},{meta.Ambient},{meta.Torque}");
                validniPodaci.Flush();

                Results result = new Results();
                result.Acknowledgement = AcknowledgementType.ACK;
                result.Status = StatusType.IN_PROGRESS;
                result.Poruka = "Sesija je uspesno pokrenuta.";

                return result;
            }catch(Exception e)
            {
                Results result = new Results();
                result.Status = StatusType.COMPLETED;
                result.Poruka = "Greska prilikom pokretanja sesije.";
                return result;
            }
        }
        private List<string> ProveraValidnosti(MotorSample sample)
        {
            List<string> greske= new List<string>();

            if (sample.PM <= 0)
            {
                greske.Add("PM mora biti > 0");
            }
            
            return greske;
        }
    }
}
