using Common;
using CsvHelper;
using System;
using System.Globalization;
using System.IO;

namespace Service
{
    internal class SessionFileWriter : IDisposable
    {
        private readonly StreamWriter measurementsWriter;
        private readonly StreamWriter rejectsWriter;
        private readonly StreamWriter logWriter;

        private readonly CsvWriter measurementsCsv;
        private readonly CsvWriter rejectsCsv;

        private bool disposed;

        public string MeasurementsPath { get; private set; }
        public string RejectsPath { get; private set; }
        public string LogPath { get; private set; }

        public SessionFileWriter(string sessionFolderPath, string[] format)
        {
            if (string.IsNullOrWhiteSpace(sessionFolderPath))
            {
                throw new ArgumentException("Session folder path is missing.");
            }

            if (format == null || format.Length == 0)
            {
                throw new ArgumentException("CSV format is missing.");
            }

            Directory.CreateDirectory(sessionFolderPath);

            MeasurementsPath = Path.Combine(sessionFolderPath, "measurements_session.csv");
            RejectsPath = Path.Combine(sessionFolderPath, "rejects.csv");
            LogPath = Path.Combine(sessionFolderPath, "log.txt");

            measurementsWriter = new StreamWriter(MeasurementsPath, false);
            rejectsWriter = new StreamWriter(RejectsPath, false);
            logWriter = new StreamWriter(LogPath, true);

            measurementsCsv = new CsvWriter(measurementsWriter, CultureInfo.InvariantCulture);
            rejectsCsv = new CsvWriter(rejectsWriter, CultureInfo.InvariantCulture);

            WriteHeader(measurementsCsv, format, false);
            WriteHeader(rejectsCsv, format, true);

            WriteLog("[DISPOSE INIT] SessionFileWriter opened measurements, rejects and log streams.");
        }

        public void WriteValidSample(Sample sample)
        {
            ThrowIfDisposed();

            WriteSampleFields(measurementsCsv, sample);
            measurementsCsv.NextRecord();
            measurementsWriter.Flush();
        }

        public void WriteRejectedSample(Sample sample, string reason)
        {
            ThrowIfDisposed();

            if (sample != null)
            {
                WriteSampleFields(rejectsCsv, sample);
            }
            else
            {
                for (int i = 0; i < 7; i++)
                {
                    rejectsCsv.WriteField(string.Empty);
                }
            }

            rejectsCsv.WriteField(reason);
            rejectsCsv.NextRecord();
            rejectsWriter.Flush();
        }

        public void WriteLog(string message)
        {
            ThrowIfDisposed();

            logWriter.WriteLine(message);
            logWriter.Flush();
        }

        private void WriteHeader(CsvWriter csv, string[] format, bool includeRejectReason)
        {
            foreach (string column in format)
            {
                csv.WriteField(column);
            }

            if (includeRejectReason)
            {
                csv.WriteField("RejectReason");
            }

            csv.NextRecord();
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

        private void ThrowIfDisposed()
        {
            if (disposed)
            {
                throw new ObjectDisposedException("SessionFileWriter");
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
            {
                return;
            }

            if (disposing)
            {
                if (measurementsCsv != null)
                {
                    measurementsCsv.Dispose();
                }

                if (rejectsCsv != null)
                {
                    rejectsCsv.Dispose();
                }

                if (measurementsWriter != null)
                {
                    measurementsWriter.Dispose();
                }

                if (rejectsWriter != null)
                {
                    rejectsWriter.Dispose();
                }

                if (logWriter != null)
                {
                    logWriter.Dispose();
                }
            }

            disposed = true;
        }
    }
}