using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Net;
using System.Net.Sockets;

namespace def
{
    [Serializable]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class Client
    {
        string ID;
        string PW;
        //[MarshalAs(UnmanagedType.I4)]
        Socket socket;
        [MarshalAs(UnmanagedType.I4)]
        int msgcount;
        public Client(Socket s, string[] idpw)
        {
            socket = s;
            ID = idpw[0];
            PW = idpw[1];
        }

        public Socket GetSocket()
        {
            return socket;
        }
         
    }

    public class Functions
    {
        public static Client FindeClient(List<Client> clients, Socket inputsock)
        {
            for (int i = 0; i < clients.Count; i++)
            {
                if (clients[i].GetSocket() == inputsock)
                    return clients[i];
                    
            }//for
            return null;
        }//FindeClient
    }
}
