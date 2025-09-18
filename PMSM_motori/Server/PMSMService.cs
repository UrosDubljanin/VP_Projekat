using Common;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;

namespace Server
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class PMSMService : IPMSMService
    {
        private static bool SessiaUToku = false;
        private static StreamWriter validniPodaci;
        private static StreamWriter nevalidniPodaci;
        private int brojac = 0;

        // Događaji
        public event TransferStartedHandler OnTransferStarted;
        public event SampleReceivedHandler OnSampleReceived;
        public event TransferCompletedHandler OnTransferCompleted;
        public event WarningRaisedHandler OnWarningRaised;

        // Stanje za analitiku ΔT i out-of-band
        private double _runningMeanPM = 0.0;
        private long _countPM = 0;
        private double? _prevPM = null;
        private double? _prevStatorW = null;
        private double? _prevStatorT = null;

        // Pragovi
        private readonly double _pmThreshold = ReadDouble("PM_threshold", 3.0);
        private readonly double _statorWThres = ReadDouble("Stator_w_threshold", 4.0);
        private readonly double _statorTThres = ReadDouble("Stator_t_threshold", 4.0);
        private readonly double _oobPercent = ReadDouble("OutOfBandPercent", 0.25);

        public Results EndSession()
        {
            try
            {
                if (!SessiaUToku)
                {
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

                // Kraj prenosa (event)
                OnTransferCompleted?.Invoke(this, EventArgs.Empty);

                Results result = new Results();
                result.Status = StatusType.COMPLETED;
                result.Poruka = "Sesija je uspesno zavrsena.";
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
                if (validniPodaci == null)
                {
                    Results result = new Results();
                    result.Status = StatusType.COMPLETED;
                    result.Poruka = "Fajl za validne podatke nije otvoren.";
                    return result;
                }
                if (nevalidniPodaci == null)
                {
                    Results result = new Results();
                    result.Status = StatusType.COMPLETED;
                    result.Poruka = "Fajl za nevalidne podatke nije otvoren.";
                    return result;
                }

                ValidationFault greska = ProveraValidnosti(sample);

                if (!greska.jeste)
                {
                    // upis u validni CSV
                    validniPodaci.WriteLine($"{sample.Stator_Winding},{sample.Stator_Tooth},{sample.Stator_Yoke},{sample.PM},{sample.Profile_ID},{sample.Ambient},{sample.Torque}");
                    validniPodaci.Flush();
                    brojac++;

                    OnSampleReceived?.Invoke(this, new SampleEventArgs(sample));

                    // ΔT_pm
                    if (_prevPM.HasValue)
                    {
                        double dPM = sample.PM - _prevPM.Value;
                        if (Math.Abs(dPM) > _pmThreshold)
                        {
                            OnWarningRaised?.Invoke(
                                this,
                                new WarningEventArgs(
                                    kind: "PMSpike",
                                    direction: dPM >= 0 ? Direction.Above : Direction.Below,
                                    value: sample.PM,
                                    reference: _pmThreshold
                                )
                            );
                        }
                    }

                    // running mean + out-of-band
                    _countPM++;
                    _runningMeanPM = _runningMeanPM + (sample.PM - _runningMeanPM) / _countPM;
                    if (_countPM >= 5 && _oobPercent > 0)
                    {
                        double low = (1.0 - _oobPercent) * _runningMeanPM;
                        double high = (1.0 + _oobPercent) * _runningMeanPM;

                        if (sample.PM < low)
                            OnWarningRaised?.Invoke(this, new WarningEventArgs("OutOfBand", Direction.Below, sample.PM, low));
                        else if (sample.PM > high)
                            OnWarningRaised?.Invoke(this, new WarningEventArgs("OutOfBand", Direction.Above, sample.PM, high));
                    }
                    _prevPM = sample.PM;

                    // ΔT stator winding
                    if (_prevStatorW.HasValue)
                    {
                        double dW = sample.Stator_Winding - _prevStatorW.Value;
                        if (Math.Abs(dW) > _statorWThres)
                        {
                            OnWarningRaised?.Invoke(
                                this,
                                new WarningEventArgs(
                                    kind: "StatorSpikeW",
                                    direction: dW >= 0 ? Direction.Above : Direction.Below,
                                    value: sample.Stator_Winding,
                                    reference: _statorWThres
                                )
                            );
                        }
                    }
                    _prevStatorW = sample.Stator_Winding;

                    // ΔT stator tooth
                    if (_prevStatorT.HasValue)
                    {
                        double dT = sample.Stator_Tooth - _prevStatorT.Value;
                        if (Math.Abs(dT) > _statorTThres)
                        {
                            OnWarningRaised?.Invoke(this, new WarningEventArgs(kind: "StatorSpikeT", direction: dT >= 0 ? Direction.Above : Direction.Below, value: sample.Stator_Tooth, reference: _statorTThres
                                )
                            );
                        }
                    }
                    _prevStatorT = sample.Stator_Tooth;

                    Results result = new Results();
                    result.validationFault = greska;
                    result.Acknowledgement = AcknowledgementType.ACK;
                    result.Status = StatusType.IN_PROGRESS;
                    result.Poruka = "Red je validan i uspesno pushovan.";
                    return result;

                }
                else
                {
                    // upis u rejects CSV
                    nevalidniPodaci.WriteLine($"{sample.Stator_Winding},{sample.Stator_Tooth},{sample.Stator_Yoke},{sample.PM},{sample.Profile_ID},{sample.Ambient},{sample.Torque}");
                    nevalidniPodaci.Flush();

                    Results result = new Results();
                    result.validationFault = greska;
                    result.Acknowledgement = AcknowledgementType.NACK;
                    result.Status = StatusType.IN_PROGRESS;
                    result.Poruka = $"Red je nevalidan";
                    return result;
                }
            }
            catch (Exception ex)
            {
                Results result = new Results();
                result.Status = StatusType.COMPLETED;
                result.Poruka = "Greska prilikom pushovanja samplova.";
                return result;
            }

        }

        public Results StartSession(MetaData meta)
        {
            try
            {
                if (SessiaUToku)
                {
                    Results r = new Results();
                    r.Status = StatusType.IN_PROGRESS;
                    r.Poruka = "Sesija je vec pokrenuta.";
                    return r;
                }

                validniPodaci = new StreamWriter(ConfigurationManager.AppSettings["DataPath1"], false);
                nevalidniPodaci = new StreamWriter(ConfigurationManager.AppSettings["DataPath2"], false);

                validniPodaci.WriteLine("Stator_Winding,Stator_Tooth,Stator_Yoke,PM,Profile_Id,Ambient,Torque");
                nevalidniPodaci.WriteLine("Stator_Winding,Stator_Tooth,Stator_Yoke,PM,Profile_Id,Ambient,Torque");

                validniPodaci.Flush();
                nevalidniPodaci.Flush();

                SessiaUToku = true;

                validniPodaci.WriteLine($"{meta.Stator_Winding},{meta.Stator_Tooth},{meta.Stator_Yoke},{meta.PM},{meta.Profile_ID},{meta.Ambient},{meta.Torque}");
                validniPodaci.Flush();
                brojac++;

                _runningMeanPM = 0.0;
                _countPM = 0;
                _prevPM = null;
                _prevStatorW = null;
                _prevStatorT = null;

                // start prenosa (event)
                OnTransferStarted?.Invoke(this, new TransferEventArgs(meta));

                Results result = new Results();
                result.Acknowledgement = AcknowledgementType.ACK;
                result.Status = StatusType.IN_PROGRESS;
                result.Poruka = "Sesija je uspesno pokrenuta.";

                return result;
            }
            catch (Exception e)
            {
                Results result = new Results();
                result.Status = StatusType.COMPLETED;
                result.Poruka = "Greska prilikom pokretanja sesije.";
                return result;
            }
        }

        private ValidationFault ProveraValidnosti(MotorSample sample)
        {
            ValidationFault greska = new ValidationFault();
            greska.jeste = false;

            if (sample.PM <= 0)
            {
                greska.jeste = true;
                greska.Poruka = greska.Poruka + "PM mora biti veci od 0 ;";
                greska.Polje = greska.Polje + "PM ;";
            }

            return greska;
        }

        private static double ReadDouble(string key, double def)
        {
            var s = ConfigurationManager.AppSettings[key];
            double v;
            if (double.TryParse(s, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out v)) return v;
            if (double.TryParse(s, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.CurrentCulture, out v)) return v;
            return def;
        }
    }
}
