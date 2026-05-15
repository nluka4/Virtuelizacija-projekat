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

        public SessionMeta(string fileName, string[] format, DateTime timeStamp)
        {
            FileName = fileName;
            Format = format;
            TimeStamp = timeStamp;
        }

        [DataMember]
        public string FileName { get; set; }

        [DataMember]
        public string[] Format { get; set; }

        [DataMember]
        public DateTime TimeStamp { get; set; }

    }
}