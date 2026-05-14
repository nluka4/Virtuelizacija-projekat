using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;

namespace Service
{
    public class SmartGridService : IService
    {
        public void StartSession(SessionMeta meta)
        {
            Console.WriteLine("Ime fajla: "+meta.FileName);
            Console.WriteLine("Vreme " + meta.TimeStamp);
            Console.Write("Format: ");
            foreach(string var in meta.Format)
            {
                Console.Write($"{var},");
            }
            Console.WriteLine();
            Console.WriteLine("Status: " + meta.Status);
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
