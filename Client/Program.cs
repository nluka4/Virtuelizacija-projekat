using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using Common;
using System.IO;
using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration.Attributes;

namespace Client
{
    internal class Program
    {
        static void Main(string[] args)
        {

            //As i understood ChannelFactory is good to be used when you 
            //Have an implemented interface (contract) so you only need to share assemblies between 
            //client and a server 
            ChannelFactory<IService> factory = new ChannelFactory<IService>("SmartGrid");

            //Create proxy from channel factory
            IService proxy = factory.CreateChannel();


            //data 
            string path = "C:\\FAKS\\3 GODINA\\virtuelizacija\\projekat\\Client\\smart_grid_dataset.csv";
            string[] niz = path.Split('\\');
            string fajl = niz[niz.Length - 1];

            string[] format= { "Timestamp", "Power Usage (kW)", "Frequency (Hz)", "FFT_1", "FFT_2", "FFT_3", "FFT_4" };
            //Starting session 
            SessionMeta sessionData = new SessionMeta(fajl,format, DateTime.Now);

            Response res = proxy.StartSession(sessionData);

            Console.WriteLine("====> " + res.SessionId);
            //Reading data 
            using (var streamReader = new StreamReader(path))
            {
                using (var csvReader = new CsvReader(streamReader, CultureInfo.InvariantCulture))
                {
                    var records = csvReader.GetRecords<Sample>().Take(117).ToList();
                    foreach(var record in records)
                    {
                        Response res2 = proxy.PushSample(record);
                        Console.WriteLine("Code: "+res2.Code);
                    }
                }
            }
            proxy.EndSession();
            //Console.WriteLine("Ovaj mi malo gura dildo. svidja mi se :)");
            Console.ReadLine();
        }

    }
}
