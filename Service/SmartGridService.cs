using System;
using System.IO;
using System.Linq;
using System.Globalization;
using System.ServiceModel;
using Common;
using CsvHelper;
using System.Configuration;
using Microsoft.Diagnostics.Tracing.Parsers;
using Microsoft.Diagnostics.Tracing.TraceUtilities.FilterQueryExpression;
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
        private string logDocPath;

        [Obsolete]
        private double fThreshold = Double.Parse(ConfigurationSettings.AppSettings["F_threshold"]);

        [Obsolete]
        private double fftThreshold = Double.Parse(ConfigurationSettings.AppSettings["FFT_threshold"]);
        
        [Obsolete] 
        private double averageDeviationPercent = Double.Parse(ConfigurationSettings.AppSettings["AverageDeviationPercent"]);

        [Obsolete] 
        private string serverDataPath = ConfigurationSettings.AppSettings["ServerDataPath"];

        private double Ftotal = 0;
        private double Fmean = 0;
        private double Fcount = 0;


        Sample previousSample = null;
        Sample currentSample = null;

        public Response StartSession(SessionMeta meta)
        {
            if (meta == null)
            {
                return new Response(false, "NACK", "FAILED", "", "Session metadata is missing.");
            }

            string[] expectedFormat = {"Timestamp", "Power Usage (kW)", "Frequency (Hz)", "FFT_1", "FFT_2", "FFT_3", "FFT_4"};
            if (meta.Format == null || meta.Format.Length == 0 || !meta.Format.SequenceEqual(expectedFormat))
            {
                return new Response(false, "NACK", "FAILED", "", "Session format is invalid.");
            }


            sessionID = GenerateSessionId();



            //AppDomain.CurrentDomain.BaseDirectory vraca ti putanju gde ti se nalazi application domain, to ti je folder iz kog se izvrsava aplikacija
            string baseFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "serverData");
            sessionFolderPath = Path.Combine(baseFolderPath, sessionID);

            measurementsPath = Path.Combine(sessionFolderPath, "measurement_session.csv");
            rejectsPath = Path.Combine(sessionFolderPath, "rejects.csv");
            logDocPath = Path.Combine(sessionFolderPath, "log.txt");

            Directory.CreateDirectory(sessionFolderPath);

            WriteHeader(measurementsPath, meta.Format, includeRejectReason: false);
            WriteHeader(rejectsPath, meta.Format, includeRejectReason: true);

            sessionStarted = true;


            //OnTransferStarted
            Receiver receiver = new Receiver();

            string onTransferStartedConsole = "Transfer started";
            string onTransferStartedLog = $"[{DateTime.Now.ToString()}] Transfer started. SessionId = {sessionID}, FileName={meta.FileName}, Format={meta.Format.ToString()}";

            string []messages = {onTransferStartedConsole, onTransferStartedLog};

            receiver.onMessageReceived += SessionLog;

            receiver.Receive(messages,logDocPath);

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

            //Console.WriteLine($"Sample received: Frequency={sample.Frequency}, Power={sample.Power}");

            Receiver receiver = new Receiver();
            string onSampleReceivedConsole = "Sample Received"; 
            string onSampleReceivedLog = $"[{DateTime.Now.ToString()}] Sample received. SessionId ={sessionID}, Frequency={String.Format("{0:0.00}", sample.Frequency)}, Power={String.Format("{0:0.00}", sample.Power)}, FFT1={String.Format("{0:0.00}", sample.FFT1)}, FFT2={String.Format("{0:0.00}", sample.FFT2)}, FFT3={String.Format("{0:0.00}", sample.FFT3)}, FFT4={String.Format("{0:0.00}", sample.FFT4)}";

            //OnSampleReceived
            receiver.onMessageReceived += SessionLog;

            string[] messages = { onSampleReceivedConsole, onSampleReceivedLog };
            receiver.Receive(messages,logDocPath);

            Ftotal += sample.Frequency;
            Fcount++;

            Fmean = Ftotal / Fcount;

            double lower_boundary = Fmean * (1 - averageDeviationPercent / 100);
            double upper_boundary = Fmean * (1 + averageDeviationPercent / 100);

            if (sample.Frequency < lower_boundary)
            {
                string onWarningRaisedConsole = "Warning: OutOfBandWarning detected.";

                string onWarningRaisedLog =
                    $"[{DateTime.Now}] WARNING OutOfBandWarning. " +
                    $"SessionId={sessionID}, " +
                    $"CurrentFrequency={sample.Frequency}, " +
                    $"RunningMean={Fmean}, " +
                    $"LowerBoundary={lower_boundary}, " +
                    $"UpperBoundary={upper_boundary}, " +
                    $"DeviationPercent={averageDeviationPercent}, " +
                    $"Direction= below the expected value";

                messages[0] = onWarningRaisedConsole;
                messages[1] = onWarningRaisedLog;
                receiver.Receive(messages, logDocPath);
            }
            else if(sample.Frequency > upper_boundary)
            {
                string onWarningRaisedConsole = "Warning: OutOfBandWarning detected.";

                string onWarningRaisedLog =
                    $"[{DateTime.Now}] WARNING OutOfBandWarning. " +
                    $"SessionId={sessionID}, " +
                    $"CurrentFrequency={sample.Frequency}, " +
                    $"RunningMean={Fmean}, " +
                    $"LowerBoundary={lower_boundary}, " +
                    $"UpperBoundary={upper_boundary}, " +
                    $"DeviationPercent={averageDeviationPercent}, " +
                    $"Direction= above the expected value";

                messages[0] = onWarningRaisedConsole;
                messages[1] = onSampleReceivedLog;
                receiver.Receive(messages, logDocPath);
            }

            double deltaF =0, deltaFFT =0; 

            if(previousSample == null)
            {
                previousSample = sample;
            }

            double FFTn = 0;
            double FFTn_1 = 0;
            if (previousSample != null)
            {
                deltaF = sample.Frequency - previousSample.Frequency;
                FFTn = (sample.FFT1 + sample.FFT2 + sample.FFT3 + sample.FFT4) / 4;
                FFTn_1 = (previousSample.FFT1 + previousSample.FFT2 + previousSample.FFT3 + previousSample.FFT4) / 4;

                deltaFFT = FFTn - FFTn_1;

            }

            if(Math.Abs(deltaF) > fThreshold)
            {
                RaiseWarning("FrequencySpike","Warning: Frequency spike detected",sample.Frequency,previousSample.Frequency,deltaF,fThreshold,"Frequency","deltaF","fThreshold",receiver,logDocPath);
            }
           
            if(Math.Abs(deltaFFT)> fftThreshold)
            {
                RaiseWarning("FFTSpike","Warning: Fast Fourier Transform (FFT) spike detected",FFTn,FFTn_1,deltaFFT,fftThreshold,"FFT","deltaFFT","fftThreshold",receiver,logDocPath);
            }


            previousSample = sample;

            //Console.WriteLine("Prosecna frekvencija je: " +  Fmean);
            return new Response(
                true,
                "ACK",
                "IN_PROGRESS",
                sessionID,
                "Sample received and written successfully."
            );
        }


        private void RaiseWarning(
    string warningType,
    string consoleMessage,
    double currentValue,
    double previousValue,
    double delta,
    double threshold,
    string valueName,
    string deltaName,
    string thresholdName,
    Receiver receiver,
    string logDocPath)
        {
            string direction = delta > 0 ? "above expected value" : "below expected value";

            string warningConsole = consoleMessage;

            string warningLog =
                $"[{DateTime.Now}] WARNING {warningType}. " +
                $"SessionId={sessionID}, " +
                $"Current {valueName}={currentValue}, " +
                $"Previous {valueName}={previousValue}, " +
                $"{deltaName}={delta}, " +
                $"{thresholdName}={threshold}, " +
                $"Direction={direction}";

            string[] messages = { warningConsole, warningLog };

            receiver.Receive(messages, logDocPath);
        }





        public void SessionLog(string[] message, string path)
        {
            Console.WriteLine(message[0]);

            using(StreamWriter writer = new StreamWriter(path, append: true))
            {
                writer.WriteLine(message[1]);
            }
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

            Receiver receiver = new Receiver();

            string OnTransferCompletedConsole = "Transfer terminated";
            string OnTransferCompletedLog = $"[{DateTime.Now.ToString()}] Transfer terminated. SessionId = {sessionID}";

            string[] messages = { OnTransferCompletedConsole, OnTransferCompletedLog };

            receiver.onMessageReceived += SessionLog;

            receiver.Receive(messages, logDocPath);
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