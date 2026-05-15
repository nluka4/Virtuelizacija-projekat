using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using System.Runtime.Serialization;
namespace Common
{
    [DataContract]
    public class Response
    {
        public Response(bool ack, string code, string status, string session_id, string message = "")
        {
            Ack = ack;
            if (code == "ACK" || code == "NACK")
            {
                Code = code;
            }
            else
            {
                Code = "NACK";
            }
            if(status == "IN_PROGRESS" || status == "COMPLETED" || status == "FAILED")
            {

            Status = status;
            }
            else
            {
                Status = "FAILED";
            }

            if (Status == "IN_PROGRESS")
            {
                Message = "Communication is in progress. The server is ready to receive data.";
            }
            else if (Status == "COMPLETED")
            {
                Message = "Communication completed successfully. All data has been processed.";
            }
            else
            {
                Message = "Communication failed. The request could not be completed.";
            }

            SessionId = session_id;
        }

        [DataMember]
        public bool Ack { get; set; }
        [DataMember]
        public string Code { get; set; }
        [DataMember]
        public string Status { get; set; }
        [DataMember]
        public string Message { get; set;  }
        [DataMember]
        public string SessionId { get; set; }
    }
}
