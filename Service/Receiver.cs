using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service
{
    public class Receiver
    {
        public delegate void MessageReceivedHandler(string []message, string path = "");    
        public event MessageReceivedHandler onMessageReceived;

        public void Receive(string []message, string path = "")
        {
            onMessageReceived?.Invoke(message, path);
        }
    }
}
