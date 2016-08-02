using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.IO;
using ChatDefine;



namespace client
{
    class client
    {
        static void Main(string[] args)
        {
            Socket clientSocket = ConnectToServer();


            while (true)
            {

                //FirstPage
                FirstPageUI();

                string command = Console.ReadLine().ToUpper();
                //FirstPage ENd

                //Login Page Start
                if (command == "LOGIN") {

                    LogIn(clientSocket);

                    //try login
                    bool loginResult = LogInAccept(clientSocket);

                    if (loginResult)
                    {
                        //LobbyPage();
                        break;
                    }
                    else
                    {
                        continue;
                    }
  
                }else if(command == "EXIT") {    
                    clientSocket.Close();
                    break;
                }

                //Login Page End
            }



        }//main ends

        public static void FirstPageUI()
        {
            Console.WriteLine("==========================================================");
            Console.WriteLine("================Welcome to the POCKETCHAT!!===============");
            Console.WriteLine("==========================================================");
            Console.WriteLine("=================LOGIN==================EXIT==============");
        }



        public static void LogIn(Socket s)
        {
            string id = "";
            string pw = "";
            int idMaxSize = 12;

            Console.WriteLine("==========================================================");
            Console.WriteLine("==================Welcome to POCKETCHAT!!=================");
            Console.WriteLine("==========================================================");

            do
            {
                Console.WriteLine("Enter your ID : ");
                id = Console.ReadLine();
            }
            while (!IsValidID(id, idMaxSize));

            do
            {
                Console.WriteLine("Enter your PW : ");
                pw = Console.ReadLine();
            }
            while (!IsValidPW(pw, idMaxSize));


            string loginInfo = id + "#" + pw;
            Console.WriteLine("before");
            byte[] loginData = CreatePacket(10, Convert.ToUInt16(loginInfo.Length), loginInfo);
            Console.WriteLine("after");
            ClientToServer(s, loginData);

        }

        public static bool LogInAccept(Socket s)
        {

            ChatProtocol result = ServerToClient(s);

            if (result.command == 12)
            {
                if (BitConverter.ToInt32(result.valueB, 0) == 1)
                {
                    Console.WriteLine("Welcome!");
                    return true;
                }
                else if (BitConverter.ToInt32(result.valueB, 0) == -1)
                {
                    Console.WriteLine("LogIn Failed");
                    return false;
                }
                else
                {
                    Console.WriteLine("오류가 발생하였습니다.");
                    return false;
                }
            }
            else
            {
                Console.WriteLine("오류가 발생하였습니다.");
                return false;
            }
        }

        public static bool IsValidID(string info, int idMaxSize)
        {
            if (Encoding.UTF8.GetBytes(info).Length > idMaxSize)
            {
                Console.WriteLine("길이는 최대 12byte를 넘길 수 없습니다. (영어 12자, 한글 4자)");
                return false;
            }
            else
            {
                string cleanString = Regex.Replace(info, @"[^a-zA-Z0-9가-힣]", "", RegexOptions.Singleline);

                if (info.CompareTo(cleanString) != 0)
                {
                    Console.WriteLine("특수문자 또는 공백을 포함할 수 없습니다.");
                    return false;
                }
            }
            return true;
        }

        public static bool IsValidPW(string info, int idMaxSize)
        {
            if (Encoding.UTF8.GetBytes(info).Length > idMaxSize)
            {
                Console.WriteLine("길이는 최대 12byte를 넘길 수 없습니다. (영숫자 혼합 12자)");
                return false;
            }
            else
            {
                string cleanString = Regex.Replace(info, @"[^a-zA-Z0-9]", "", RegexOptions.Singleline);

                if (info.CompareTo(cleanString) != 0)
                {
                    Console.WriteLine("한글 또는 특수문자 또는 공백을 포함할 수 없습니다.");
                    return false;
                }
            }
            return true;
        }

        public static void MainUI()
        {
            Console.WriteLine("==========================================================");
            Console.WriteLine("====================POCKETCHAT ver.BETA===================");
            Console.WriteLine("==========================================================");
            Console.WriteLine("===========LIST===========CREATE==========LOGOUT==========");
        }

        public static Socket ConnectToServer()
        {
            string serverIP = SelectServer();
            IPAddress serip = IPAddress.Parse(serverIP);
            IPEndPoint serep = new IPEndPoint(serip, 50001);

            Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            Console.WriteLine("소켓생성");

            sock.Connect(serep);
            Console.WriteLine("Connect to Server : " + sock.RemoteEndPoint);

            return sock;
        }




        /* public static void GetCommand(string cmd)
         {
             switch ()
             {

             }
         }*/


        public static byte[] CreatePacket(byte cmd, ushort vA, string msg)
        {
            byte[] message = new byte[1024];    //max message size
            Array.Copy(Encoding.UTF8.GetBytes(msg), message, Encoding.UTF8.GetBytes(msg).Length);

            ChatProtocol CP = new ChatProtocol();
            CP.command = cmd;
            CP.valueA = vA;
            CP.valueB = message;

            byte[] data = StructureToByte(CP);

            return data;
        }


        public static void ClientToServer(Socket s, byte[] data)
        {
            Console.WriteLine("Before Send");
            s.Send(data, 0, data.Length, SocketFlags.None);
            Console.WriteLine("After Send");
        }


        public static ChatProtocol ServerToClient(Socket s)
        {

            byte[] data = new byte[Marshal.SizeOf(typeof(ChatProtocol))];

            s.Receive(data, data.Length, SocketFlags.None);

            ChatProtocol pt = (ChatProtocol)ByteToStructure(data, typeof(ChatProtocol));
            return pt;

            /*
            byte[] rcvBuff = new byte[1024];
            int rcvByteNum = 0;
            rcvByteNum = s.Receive(rcvBuff, SocketFlags.None);
            Console.WriteLine("From server : " + Encoding.UTF8.GetString(rcvBuff, 0, rcvByteNum));*/
        }


        public static string SelectServer()
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "config.txt");
            string[] lines = System.IO.File.ReadAllLines(path);
            Random rand = new Random();
            return lines[rand.Next(lines.Length)];
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
            Marshal.Copy(data, 0, buff, data.Length); // 배열에 저장된 데이터를 위에서 할당한 메모리 영역에 복사한다.
            object obj = Marshal.PtrToStructure(buff, type); // 복사된 데이터를 구조체 객체로 변환한다.
            Marshal.FreeHGlobal(buff); // 비관리 메모리 영역에 할당했던 메모리를 해제함


            if (Marshal.SizeOf(obj) != data.Length)// (((PACKET_DATA)obj).TotalBytes != data.Length) // 구조체와 원래의 데이터의 크기 비교
            {
                return null; // 크기가 다르면 null 리턴
            }
            return obj; // 구조체 리턴
        }

    }

}