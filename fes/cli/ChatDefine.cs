using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Net;
using System.Net.Sockets;

namespace ChatDefine
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct ChatProtocol
    {
        [MarshalAs(UnmanagedType.I1)]
        public byte command;    //256 possible commands  

        [MarshalAs(UnmanagedType.U2)]
        public ushort valueA;     //unsigned short custom-value  

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1024)]
        public byte[] valueB;     //Variable sized value  
    }



    [Serializable]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    class Room
    {
        [MarshalAs(UnmanagedType.U2)]
        ushort number;                                                   //방번호

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
        Client[] roomMember = new Client[5];   //맴버


        public void CreateRoom(int num, Client first)              //방 번호 초기화, 방장(첫입장)
        {
            number = (ushort)num;
            roomMember[0] = first;
        }

        public void leaveRoom()
        {
        }

        public int joinRoom()
        {
            return -1;
        }
        public ushort Getnumber()
        {
            return number;
        }

    }


    [Serializable]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    class Client
    {
        [MarshalAs(UnmanagedType.I4)]
        Socket socket;
        [MarshalAs(UnmanagedType.I4)]
        int msgcount;
        public Client(Socket s)
        {
            socket = s;
        }

        public Socket GetSocket()
        {
            return socket;
        }
    }
}
