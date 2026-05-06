using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service
{
    public class SmartGridService : IService
    {
        public void StartSession(Sample meta)
        {
            Console.WriteLine("Sesija je pokrenuta.");
        }

        public void PushSample(Sample sample)
        {
            Console.WriteLine($"Primljen sample: Frequency={sample.Frequency}, Power={sample.Power}");
        }

        public void EndSession()
        {
            Console.WriteLine("Sesija je završena.");
        }
    }
}
