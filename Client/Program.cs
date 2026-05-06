using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using Common;

namespace Client
{
    internal class Program
    {
        static void Main(string[] args)
        {
            ChannelFactory<IService> factory = new ChannelFactory<IService>("SmartGrid");
            IService proxy = factory.CreateChannel();

        }

    }
}
