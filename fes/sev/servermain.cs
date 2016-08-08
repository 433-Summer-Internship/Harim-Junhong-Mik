using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Servernamespace;
using def;

namespace sev
{
    class servermain
    {
        static void Main(string[] args)
        {

            Server server = new Server(50001,1);
            server.Start();
        }
    }
}
