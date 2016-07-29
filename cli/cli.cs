using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace server
{
    class server
    {
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct ChatProtocol
        {
            [MarshalAs(UnmanagedType.I1)]
            public byte command;    //256 possible commands  

            [MarshalAs(UnmanagedType.U2)]
            public ushort valueA;     //unsigned short custom-value  

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
            public byte[] valueB;     //Variable sized value  
        }

            static void Main(string[] args)
        {
            IPAddress serip = IPAddress.Parse("127.0.0.1");
            IPEndPoint serep = new IPEndPoint(serip, 50001);

            Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            Console.WriteLine("소켓생성");

            sock.Connect(serep);
            Console.WriteLine("Connddect");

            while (true)
            {
                //string input = Console.ReadLine();
                //byte[] data = Encoding.UTF8.GetBytes(input);

                //Room ro1 = new Room();
                //ro1.CreateRoom(110, null);
                //byte[] data = StructureToByte(ro1);

                ChatProtocol CP = new ChatProtocol();
                CP.command = 0;
                CP.valueA = 10;
                CP.valueB = null;
                byte[] data = StructureToByte(CP);

                sock.Send(data, 0, data.Length, SocketFlags.None);


            }

            sock.Close();
        }
        public static byte[] StructureToByte(object obj)
        {
            int datasize = Marshal.SizeOf(obj);//((PACKET_DATA)obj).TotalBytes; // 구조체에 할당된 메모리의 크기를 구한다.
            IntPtr buff = Marshal.AllocHGlobal(datasize); // 비관리 메모리 영역에 구조체 크기만큼의 메모리를 할당한다.
            Marshal.StructureToPtr(obj, buff, false); // 할당된 구조체 객체의 주소를 구한다.
            byte[] data = new byte[datasize]; // 구조체가 복사될 배열
            Marshal.Copy(buff, data, 0, datasize); // 구조체 객체를 배열에 복사
            Marshal.FreeHGlobal(buff); // 비관리 메모리 영역에 할당했던 메모리를 해제함
            return data; // 배열을 리턴
        }
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
