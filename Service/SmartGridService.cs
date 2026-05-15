using System;
using System.IO;
using System.Linq;
using System.Globalization;
using System.ServiceModel;
using Common;
using CsvHelper;

namespace Service
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession)]
    public class SmartGridService : IService
    {
        private string sessionID;
        private bool sessionStarted = false;

        private string sessionFolderPath;
        private string measurementsPath;
        private string rejectsPath;

        public Response StartSession(SessionMeta meta)
        {
            if (meta == null)
            {
                return new Response(false, "NACK", "FAILED", "", "Session metadata is missing.");
            }

            if (meta.Format == null || meta.Format.Length == 0)
            {
                return new Response(false, "NACK", "FAILED", "", "Session format is missing.");
            }

            sessionID = GenerateSessionId();

            string baseFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "serverData");
            sessionFolderPath = Path.Combine(baseFolderPath, sessionID);

            measurementsPath = Path.Combine(sessionFolderPath, "measurement_session.csv");
            rejectsPath = Path.Combine(sessionFolderPath, "rejects.csv");

            Directory.CreateDirectory(sessionFolderPath);

            WriteHeader(measurementsPath, meta.Format, includeRejectReason: false);
            WriteHeader(rejectsPath, meta.Format, includeRejectReason: true);

            sessionStarted = true;

            Console.WriteLine("Session started.");
            Console.WriteLine("Session ID: " + sessionID);
            Console.WriteLine("File name: " + meta.FileName);
            Console.WriteLine("Time: " + meta.TimeStamp);
            Console.WriteLine("Status: transfer in progress...");
            Console.Write("Format: ");

            foreach (string column in meta.Format)
            {
                Console.Write($"{column},");
            }

            Console.WriteLine();

            return new Response(
                true,
                "ACK",
                "IN_PROGRESS",
                sessionID,
                "Session started successfully. The server is ready to receive samples."
            );
        }

        public Response PushSample(Sample sample)
        {
            if (!sessionStarted)
            {
                return new Response(
                    false,
                    "NACK",
                    "FAILED",
                    sessionID,
                    "Session has not been started. Call StartSession before sending samples."
                );
            }

            string validationError = ValidateSample(sample);

            if (validationError != null)
            {
                WriteRejectedSample(sample, validationError);

                Console.WriteLine("Rejected sample: " + validationError);

                return new Response(
                    false,
                    "NACK",
                    "IN_PROGRESS",
                    sessionID,
                    validationError
                );
            }

            WriteValidSample(sample);

            Console.WriteLine($"Sample received: Frequency={sample.Frequency}, Power={sample.Power}");

            return new Response(
                true,
                "ACK",
                "IN_PROGRESS",
                sessionID,
                "Sample received and written successfully."
            );
        }

        public Response EndSession()
        {
            if (!sessionStarted)
            {
                return new Response(
                    false,
                    "NACK",
                    "FAILED",
                    sessionID,
                    "Session cannot be completed because it was not started."
                );
            }

            sessionStarted = false;

            Console.WriteLine("Transfer completed.");
            Console.WriteLine("Session completed.");
            Console.WriteLine("Session ID: " + sessionID);

            return new Response(
                true,
                "ACK",
                "COMPLETED",
                sessionID,
                "Session completed successfully. All received samples were processed."
            );
        }

        private string ValidateSample(Sample sample)
        {
            if (sample == null)
            {
                return "Sample is null.";
            }

            if (sample.TimeStamp == default(DateTime))
            {
                return "Timestamp is missing or invalid.";
            }

            if (!IsValidNumber(sample.Power))
            {
                return "Power usage is not a valid number.";
            }

            if (sample.Power < 0)
            {
                return "Power usage cannot be negative.";
            }

            if (!IsValidNumber(sample.Frequency))
            {
                return "Frequency is not a valid number.";
            }

            if (sample.Frequency <= 0)
            {
                return "Frequency must be greater than zero.";
            }

            if (!IsValidNumber(sample.FFT1))
            {
                return "FFT1 is not a valid number.";
            }

            if (!IsValidNumber(sample.FFT2))
            {
                return "FFT2 is not a valid number.";
            }

            if (!IsValidNumber(sample.FFT3))
            {
                return "FFT3 is not a valid number.";
            }

            if (!IsValidNumber(sample.FFT4))
            {
                return "FFT4 is not a valid number.";
            }

            return null;
        }

        private bool IsValidNumber(double value)
        {
            return !double.IsNaN(value) && !double.IsInfinity(value);
        }

        private void WriteHeader(string path, string[] format, bool includeRejectReason)
        {
            using (var writer = new StreamWriter(path, false))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                foreach (var column in format)
                {
                    csv.WriteField(column);
                }

                if (includeRejectReason)
                {
                    csv.WriteField("RejectReason");
                }

                csv.NextRecord();
            }
        }

        private void WriteValidSample(Sample sample)
        {
            using (var writer = new StreamWriter(measurementsPath, true))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                WriteSampleFields(csv, sample);
                csv.NextRecord();
            }
        }

        private void WriteRejectedSample(Sample sample, string reason)
        {
            using (var writer = new StreamWriter(rejectsPath, true))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                if (sample != null)
                {
                    WriteSampleFields(csv, sample);
                }
                else
                {
                    csv.WriteField("");
                    csv.WriteField("");
                    csv.WriteField("");
                    csv.WriteField("");
                    csv.WriteField("");
                    csv.WriteField("");
                    csv.WriteField("");
                }

                csv.WriteField(reason);
                csv.NextRecord();
            }
        }

        private void WriteSampleFields(CsvWriter csv, Sample sample)
        {
            csv.WriteField(sample.TimeStamp);
            csv.WriteField(sample.Power);
            csv.WriteField(sample.Frequency);
            csv.WriteField(sample.FFT1);
            csv.WriteField(sample.FFT2);
            csv.WriteField(sample.FFT3);
            csv.WriteField(sample.FFT4);
        }

        private string GenerateSessionId()
        {
            const string allChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            Random random = new Random();

            return new string(
                Enumerable.Repeat(allChars, 8)
                    .Select(chars => chars[random.Next(chars.Length)])
                    .ToArray()
            );
        }
    }
}