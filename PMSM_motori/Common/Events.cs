using System;

namespace Common
{
    public enum Direction
    {
        Below,
        Above
    }

    public class TransferEventArgs : EventArgs
    {
        public MetaData Meta { get; }
        public TransferEventArgs(MetaData meta) => Meta = meta;
    }

    public class SampleEventArgs : EventArgs
    {
        public MotorSample Sample { get; }
        public SampleEventArgs(MotorSample sample) => Sample = sample;
    }

    public class WarningEventArgs : EventArgs
    {
        public string Kind { get; }            // "OutOfBand", "PMSpike", "StatorSpikeW", "StatorSpikeT"
        public Direction Direction { get; }    // Above / Below
        public double Value { get; }           // trenutna vrednost
        public double Reference { get; }       // referenca (threshold ili mean)
        public WarningEventArgs(string kind, Direction direction, double value, double reference)
        {
            Kind = kind; Direction = direction; Value = value; Reference = reference;
        }
    }

    // Delegates
    public delegate void TransferStartedHandler(object sender, TransferEventArgs e);
    public delegate void SampleReceivedHandler(object sender, SampleEventArgs e);
    public delegate void TransferCompletedHandler(object sender, EventArgs e);
    public delegate void WarningRaisedHandler(object sender, WarningEventArgs e);
}
