using System;
using System.IO;
using System.Linq;
using Common;
using CsvHelper;
using System.Globalization;
using System.ServiceModel;

namespace Service
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession)]
    public class SmartGridService : IService
    {
        private string sessionID;

        public Response StartSession(SessionMeta meta)
        {
            Console.WriteLine("Ime fajla: " + meta.FileName);
            Console.WriteLine("Vreme " + meta.TimeStamp);

            Console.Write("Format: ");
            foreach (string var in meta.Format)
            {
                Console.Write($"{var},");
            }

            Directory.CreateDirectory("serverData");

            using (var writer = new StreamWriter(@"serverData\measurement_session.csv"))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                foreach (var column in meta.Format)
                {
                    csv.WriteField(column);
                }

                csv.NextRecord();
            }

            using (var garbageWriter = new StreamWriter(@"serverData\rejects.csv"))
            using (var csv = new CsvWriter(garbageWriter, CultureInfo.InvariantCulture))
            {
                foreach (var column in meta.Format)
                {
                    csv.WriteField(column);
                }

                csv.NextRecord();
            }

            var allChar = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();

            var resultToken = new string(
                Enumerable.Repeat(allChar, 8)
                .Select(token => token[random.Next(token.Length)])
                .ToArray()
            );

            sessionID = resultToken;

            return new Response(true, "ACK", "IN_PROGRESS", sessionID, "Session started successfully.");
        }

        public Response PushSample(Sample sample)
        {
            Console.WriteLine($"Primljen sample: Frequency={sample.Frequency}, Power={sample.Power}");

            return new Response(true, "ACK", "IN_PROGRESS", sessionID, "Sample received successfully.");
        }

        public void EndSession()
        {
            Console.WriteLine("Sesija je završena.");

            //return new Response(true, "ACK", "COMPLETED", sessionID, "Session completed successfully.");
        }
    }
}