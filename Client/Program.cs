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

            //Reading data 
            using(var streamReader = new StreamReader("C:\\FAKS\\3 GODINA\\virtuelizacija\\projekat\\Client\\smart_grid_dataset.csv"))
            {
                using (var csvReader = new CsvReader(streamReader, CultureInfo.InvariantCulture))
                {
                    var records = csvReader.GetRecords<Sample>().ToList();
                    foreach(var record in records)
                    {
                        Console.WriteLine(record);
                    }
                }
            }
            Console.WriteLine("Ovaj mene malo jebe");
            Console.ReadLine();
        }

    }
}
