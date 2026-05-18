using System.Runtime.Serialization;

namespace Common
{
    [DataContract]
    public class ValidationFault
    {
        public ValidationFault()
        {
        }

        public ValidationFault(string message, string fieldName = "", string invalidValue = "")
        {
            Message = message;
            FieldName = fieldName;
            InvalidValue = invalidValue;
        }

        [DataMember]
        public string Message { get; set; }

        [DataMember]
        public string FieldName { get; set; }

        [DataMember]
        public string InvalidValue { get; set; }
    }

    [DataContract]
    public class DataFormatFault
    {
        public DataFormatFault()
        {
        }

        public DataFormatFault(string message, string expectedFormat = "", string receivedFormat = "")
        {
            Message = message;
            ExpectedFormat = expectedFormat;
            ReceivedFormat = receivedFormat;
        }

        [DataMember]
        public string Message { get; set; }

        [DataMember]
        public string ExpectedFormat { get; set; }

        [DataMember]
        public string ReceivedFormat { get; set; }
    }
}