using System;
using System.Runtime.Serialization;

namespace Common
{
    [DataContract]
    public class SessionMeta
    {
        public SessionMeta()
        {
        }

        public SessionMeta(string fileName, string[] format, DateTime timeStamp, string status)
        {
            FileName = fileName;
            Format = format;
            TimeStamp = timeStamp;
            Status = status;
        }

        [DataMember]
        public string FileName { get; set; }

        [DataMember]
        public string[] Format { get; set; }

        [DataMember]
        public DateTime TimeStamp { get; set; }

        [DataMember]
        public string Status { get; set; }
    }
}