using System;
using System.Runtime.Serialization;
using CsvHelper.Configuration.Attributes;

namespace Common
{
    [DataContract]
    public class Sample
    {
        // Potreban CsvHelper-u da bi mogao da napravi objekat
        public Sample()
        {
        }

        public Sample(DateTime timeStamp, double power, double frequency,
                      double fft1, double fft2, double fft3, double fft4)
        {
            TimeStamp = timeStamp;
            Power = power;
            Frequency = frequency;
            FFT1 = fft1;
            FFT2 = fft2;
            FFT3 = fft3;
            FFT4 = fft4;
        }

        [DataMember]
        [Name("Timestamp")]
        public DateTime TimeStamp { get; set; }

        [DataMember]
        [Name("Power Usage (kW)")]
        public double Power { get; set; }

        [DataMember]
        [Name("Frequency (Hz)")]
        public double Frequency { get; set; }

        [DataMember]
        [Name("FFT_1")]
        public double FFT1 { get; set; }

        [DataMember]
        [Name("FFT_2")]
        public double FFT2 { get; set; }

        [DataMember]
        [Name("FFT_3")]
        public double FFT3 { get; set; }

        [DataMember]
        [Name("FFT_4")]
        public double FFT4 { get; set; }
    }
}