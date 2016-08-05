using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;

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

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1024)]
            public byte[] valueB;     //Variable sized value  

        }
        static Socket sock;
        static void Main(string[] args)
        {
            IPAddress serip = IPAddress.Parse("127.0.0.1");
            IPEndPoint serep = new IPEndPoint(serip, 50001);

            sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            Console.WriteLine("소켓생성");

            sock.Connect(serep);
            Console.WriteLine("Connddect");

            ChatProtocol CP = new ChatProtocol();
            CP.command = 40;
            CP.valueA = 5;
            Thread recv = new Thread(Receive);
            recv.Start();

            while (true)
            {
                Console.Write("command_Number : ");
                string com = Console.ReadLine();
                CP.command = (byte)Int32.Parse(com);

                Console.Write("ValueA : ");
                string va = Console.ReadLine();
                CP.valueA = (ushort)Int32.Parse(va);

                Console.Write("ValueB : ");
                string vb = Console.ReadLine();
                //CP.valueB = (byte)Int32.Parse(com);

                
                byte[] data = new byte[1024];
                Encoding.UTF8.GetBytes(vb).CopyTo(data, 0);
                CP.valueB = data;


                byte[] result = StructureToByte(CP);
                sock.Send(result, 0, result.Length, SocketFlags.None);
                Thread.Sleep(200);
            }

            sock.Close();
        }

        static void Receive()
        {
            while (true)
            {
                
                byte[] recevData = new byte[Marshal.SizeOf(typeof(ChatProtocol))];                      //프로토콜 크기만큼 메모리 할당

                int rc = sock.Receive(recevData);

                ChatProtocol pt = (ChatProtocol)ByteToStructure(recevData, typeof(ChatProtocol));       // 프로토콜 분해
 
                string message = Encoding.UTF8.GetString(pt.valueB, 0, pt.valueA);
                Console.WriteLine("Server Return : " + message);
                
            }
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
        public static object ByteToStructure(byte[] data, Type type)
        {
            IntPtr buff = Marshal.AllocHGlobal(data.Length); // 배열의 크기만큼 비관리 메모리 영역에 메모리를 할당한다.
            Marshal.Copy(data, 0, buff, data.Length);        // 배열에 저장된 데이터를 위에서 할당한 메모리 영역에 복사한다.
            object obj = Marshal.PtrToStructure(buff, type); // 복사된 데이터를 구조체 객체로 변환한다.
            Marshal.FreeHGlobal(buff);                       // 비관리 메모리 영역에 할당했던 메모리를 해제함

            if (Marshal.SizeOf(obj) != data.Length)          // 구조체와 원래의 데이터의 크기 비교
            {
                return null;
            }
            return obj;
        }//ByteToStructure
    }


}
