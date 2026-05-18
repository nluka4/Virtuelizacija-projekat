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
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, System.Configuration.ConfigurationManager.AppSettings["DatasetPath"]);
            string[] niz = path.Split('\\');
            string fajl = niz[niz.Length - 1];

            string[] format= { "Timestamp", "Power Usage (kW)", "Frequency (Hz)", "FFT_1", "FFT_2", "FFT_3", "FFT_4" };
            //Starting session 
            SessionMeta sessionData = new SessionMeta(fajl,format, DateTime.Now);

            Response res = proxy.StartSession(sessionData);

            
            //Reading data 
            Console.WriteLine(path);

            string clientRejectsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "client_rejects.log");

            File.WriteAllText(
                clientRejectsPath,
                $"Client rejects log started: {DateTime.Now}{Environment.NewLine}"
            );

            int rowNumber = 0;
            int sentRows = 0;
            int rejectedRows = 0;
            int skippedRows = 0;

            using (var streamReader = new StreamReader(path))
            using (var csvReader = new CsvReader(streamReader, CultureInfo.InvariantCulture))
            {
                csvReader.Read();
                csvReader.ReadHeader();

                while (csvReader.Read())
                {
                    rowNumber++;

                    string rawRow = "";

                    try
                    {
                        if (csvReader.Parser != null && csvReader.Parser.Record != null)
                        {
                            rawRow = string.Join(",", csvReader.Parser.Record);
                        }
                    }
                    catch
                    {
                        rawRow = "Raw row could not be read.";
                    }

                    // Sve preko 117 ne šaljemo serveru, nego logujemo u client_rejects.log
                    if (rowNumber > 117)
                    {
                        skippedRows++;

                        string skippedMessage =
                            $"SKIPPED EXTRA ROW | Row={rowNumber} | Reason=Only first 117 rows are sent | Data={rawRow}";

                        Console.WriteLine(skippedMessage);
                        AppendClientLog(clientRejectsPath, skippedMessage);

                        continue;
                    }

                    try
                    {
                        Sample record = csvReader.GetRecord<Sample>();

                        Response res2 = proxy.PushSample(record);

                        sentRows++;

                        Console.WriteLine(
                            $"Row {rowNumber} sent | Code={res2.Code} | Status={res2.Status} | Message={res2.Message}");

                        // Ako server vrati NACK, i to logujemo kao odbijen red
                        if (res2.Code == "NACK" || res2.Ack == false)
                        {
                            rejectedRows++;

                            string serverRejectMessage =
                                $"SERVER REJECTED ROW | Row={rowNumber} | Code={res2.Code} | Status={res2.Status} | Message={res2.Message} | Data={rawRow}";

                            AppendClientLog(clientRejectsPath, serverRejectMessage);
                        }
                    }
                    catch (Exception ex)
                    {
                        rejectedRows++;

                        string invalidMessage =
                            $"INVALID OR INCOMPLETE CSV ROW | Row={rowNumber} | Error={ex.Message} | Data={rawRow}";

                        Console.WriteLine(invalidMessage);
                        AppendClientLog(clientRejectsPath, invalidMessage);
                    }
                }
            }

            Console.WriteLine("----------------------------------------");
            Console.WriteLine($"Sent rows: {sentRows}");
            Console.WriteLine($"Rejected/invalid rows: {rejectedRows}");
            Console.WriteLine($"Skipped extra rows: {skippedRows}");
            Console.WriteLine($"Client rejects log: {clientRejectsPath}");
            Console.WriteLine("====> " + res.SessionId);
            Console.WriteLine("----------------------------------------");
            proxy.EndSession();
            Console.ReadLine();
        }

        private static void AppendClientLog(string path, string message)
        {
            File.AppendAllText(
                path,
                $"[{DateTime.Now}] {message}{Environment.NewLine}"
            );
        }

    }
}
