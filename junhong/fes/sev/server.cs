using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using def;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
namespace sev
{
    class tes
    {
        Socket d;

        public tes(Socket v)
        {
            d = v;
        }

    }
    class server
    {
        static void Main(string[] args)
        {
            Room r1 = new Room(50001);
            Thread roomAccept = new Thread(r1.RoomOpen);
            Thread roomBroadcast = new Thread(r1.Broadcast);
            roomAccept.Start();
            roomBroadcast.Start();

            //Socket listenSock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //Socket listenSock2 = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            //List<tes> test = new List<tes>();
            //test.Add(new tes(listenSock));
            //test.Add(new tes(listenSock2));

            //Console.WriteLine(test.Count);
            //test.Remove(new tes(new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)));
            //Console.WriteLine(test.Count);

        }//main
    }//server
}