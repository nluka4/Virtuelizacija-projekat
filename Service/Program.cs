using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Service
{
    internal class Program
    {
        static void Main(string[] args)
        {
            using (ServiceHost host = new ServiceHost(typeof(SmartGridService)))
            {
                host.Open();

                Console.WriteLine("SmartGrid servis je pokrenut.");
                Console.WriteLine("Adresa: net.tcp://localhost:4000/SmartGrid");
                Console.WriteLine("Pritisni ENTER za zaustavljanje servisa...");
                Console.ReadLine();

                host.Close();
            }
        }
    }
}
