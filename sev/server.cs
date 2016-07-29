using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace sev
{
    static class Constants
    {
        public const ushort maximumRoomMember = 5;
    }

    [Serializable]
    [StructLayout(LayoutKind.Sequential,Pack =1)]
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

 
    class server
    {
        
        
        public static object ByteToStructure(byte[] data, Type type)
        {
            IntPtr buff = Marshal.AllocHGlobal(data.Length); // 배열의 크기만큼 비관리 메모리 영역에 메모리를 할당한다.
            Marshal.Copy(data, 0, buff, data.Length); // 배열에 저장된 데이터를 위에서 할당한 메모리 영역에 복사한다.
            object obj = Marshal.PtrToStructure(buff, type); // 복사된 데이터를 구조체 객체로 변환한다.
            Marshal.FreeHGlobal(buff); // 비관리 메모리 영역에 할당했던 메모리를 해제함

            
            if (Marshal.SizeOf(obj) != data.Length)// (((PACKET_DATA)obj).TotalBytes != data.Length) // 구조체와 원래의 데이터의 크기 비교
            {
                return null; // 크기가 다르면 null 리턴
            }
            return obj; // 구조체 리턴
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

        static List<Client> clientList = new List<Client>();
        static void Main(string[] args)
        {
            Socket listenSock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            Socket clientSock = null;

            Console.WriteLine("소켓생성, 바인드");
            listenSock.Bind(new IPEndPoint(IPAddress.Any, 50001));
            listenSock.Listen(5);

            while (true)
            {
                try
                {   //접속을 받아서 클라이언트 리스트에 추가
                    clientSock = listenSock.Accept();
                    Client client = new Client(clientSock);
                    clientList.Add(client);
                    //clientSock = null;

                    Console.WriteLine("Accept");
                    Console.WriteLine(clientList[clientList.Count - 1].GetSocket().RemoteEndPoint.ToString());

                    byte[] data = new byte[Marshal.SizeOf(typeof(ChatProtocol))];

                    clientSock.Receive(data, data.Length, SocketFlags.None);
                    
                    ChatProtocol pt = (ChatProtocol)ByteToStructure(data, typeof(ChatProtocol));
                    //Room r2 = (Room)ByteToStructure(data, typeof(Room));

                    Console.WriteLine(pt.command + "," + pt.valueA + "," + Encoding.Default.GetString(pt.valueB));
                }
                catch (SocketException e)
                {
                    Console.WriteLine(e.ToString());
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                    Console.WriteLine(((IPEndPoint)(clientSock.RemoteEndPoint)).Port + " OUT");
                    clientSock.Close();
                }
                
            }
        }

        void GetRoomList()
        {
           
        }
    }
}