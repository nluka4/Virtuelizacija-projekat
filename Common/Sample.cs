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
        private DateTime _TimeStamp;
        private double _Power;
        private double _Frequency;
        private double _FFT1;
        private double _FFT2;
        private double _FFT3;
        private double _FFT4;

        public Sample(DateTime timeStamp, double power, double frequency, double fft1, double fft2, double fft3, double fft4)
        {
            _TimeStamp = timeStamp;
            _Power = power;
            _Frequency = frequency;
            _FFT1 = fft1;
            _FFT2 = fft2;
            _FFT3 = fft3;
            _FFT4 = fft4;
        }

        [DataMember]
       public DateTime TimeStamp
        {
            get { return _TimeStamp; }
            set { _TimeStamp = value; }
        }

        
        [DataMember]
        public double Power
        {
            get { return _Power; }
            set { _Power = value; }
        }
        
        [DataMember]
        public double Frequency
        {
            get { return _Frequency; }
            set { _Frequency = value; }
        }
        
       
        [DataMember]
        public double FFT1
        {
            get { return _FFT1; }
            set { _FFT1 = value; }
        }
        
        [DataMember]
        public double FFT2
        {
            get { return _FFT2; }
            set { _FFT2 = value; }
        }

        [DataMember]
        public double FFT3
        {
            get { return _FFT3; }
            set { _FFT3 = value; }
        }

        [DataMember]
        public double FFT4
        {
            get { return _FFT4; }
            set { _FFT4 = value; }
        }
    }
}