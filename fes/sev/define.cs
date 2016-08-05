using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Timers;

namespace def
{
    public class Client
    {
        string ID;
        Socket socket;
        int msgcount;

        public Client(Socket s, string id)
        {
            socket = s;
            ID = id;
        }

        public Socket GetSocket()
        {
            return socket;
        }

        public string GetID()
        {
            return ID;
        }

        public int GetMsgCount()
        {
            return msgcount;
        }
        public void SetMsgCount(int c)
        {
            msgcount = c;
        }
    }
     
    public class Functions
    {
        public static Client FindClient(List<Client> clients, Socket inputsock)
        {
            for (int i = 0; i < clients.Count; i++)
            {
                if (clients[i].GetSocket() == inputsock)
                    return clients[i];

            }//for
            return null;
        }//FindeClient


        public static Client FindClient(List<Client> clients, string ID)
        {
            for (int i = 0; i < clients.Count; i++)
            {
                if (clients[i].GetID() == ID)
                    return clients[i];

            }//for
            return null;
        }//FindeClient

        
        public static void Log(string str)
        {
            Console.WriteLine(str);
        }
    }
}
