using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace Common
{
    [DataContract]
    public class Sample
    {
        private DateTime timeStamp;
        private double power;
        private double frequency;
        private double fft1;
        private double fft2;
        private double fft3;
        private double fft4;

        public Sample(DateTime timeStamp, double power, double frequency, double fft1, double fft2, double fft3, double fft4)
        {
            this.timeStamp = timeStamp;
            this.power = power;
            this.frequency= frequency;
            this.fft1= fft1;
            this.fft2 = fft2;
            this.fft3 = fft3;
            this.fft4 = fft4;
        }

        [DataMember]
       public DateTime TimeStamp
        {
            get { return timeStamp; }
            set { timeStamp = value; }
        }

        
        [DataMember]
        public double Power
        {
            get { return power; }
            set { power = value; }
        }
        
        [DataMember]
        public double Frequency
        {
            get { return frequency; }
            set { frequency = value; }
        }
        
       
        [DataMember]
        public double FFT1
        {
            get { return fft1; }
            set { fft1 = value; }
        }
        
        [DataMember]
        public double FFT2
        {
            get { return fft2; }
            set { fft2 = value; }
        }

        [DataMember]
        public double FFT3
        {
            get { return fft3; }
            set { fft3 = value; }
        }

        [DataMember]
        public double FFT4
        {
            get { return fft4; }
            set { fft4 = value; }
        }
    }
}