//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using System.Threading;
//using System.Net;
//using System.Net.Sockets;
//using System.Runtime.InteropServices;
//using def;

///*
// login 10
//logout 11
//MessageToServer 21
//MessageToClient 22
//JoinRoom 31
//CreateRoom 32
//LeaveRoom 33
//RoomList 40
//UserList 50
//heatbeat 60

//*/
//namespace sev
//{ 
//    class Room
//    {
//        static List<Client> clientList = new List<Client>();
//        Socket listenSock = null;
//        Socket clientSock = null;

//        public Room(int port)
//        {
//            Console.WriteLine("소켓생성, 바인드");
//            listenSock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
//            listenSock.Bind(new IPEndPoint(IPAddress.Any, port));
//            listenSock.Listen(5);
//        }//Room

//        void RoomOpen()
//        {
//            try
//            {
//                clientSock = listenSock.Accept();
//                Console.WriteLine("연결확인");
//                Client client = new Client(clientSock);
//                clientList.Add(client);
//            }//try
//            catch(SocketException e)
//            {
//                Console.WriteLine(e.ToString());
//            }//catch
            
//        }//RoomOpen

//        void test()
//        {
//            //Thread th = new Thread(stru)
//            while (true)
//            {
//                try
//                {   //접속을 받아서 클라이언트 리스트에 추가
//                    clientSock = listenSock.Accept();
//                    Client client = new Client(clientSock);
//                    clientList.Add(client);

//                    //Console.WriteLine("입장 : "+clientList[clientList.Count - 1].GetSocket().RemoteEndPoint.ToString());


//                    byte[] recevData = new byte[Marshal.SizeOf(typeof(ChatProtocol))];                      //프로토콜 크기만큼 메모리 할당
//                    clientSock.Receive(recevData, recevData.Length, SocketFlags.None);                      //온 값을 받아서 저장
//                    ChatProtocol pt = (ChatProtocol)ByteToStructure(recevData, typeof(ChatProtocol));       // 프로토콜 분해
                    

//                    Console.WriteLine(pt.command + "," + pt.valueA + "," + Encoding.Default.GetString(pt.valueB));
//                }//try
//                catch (SocketException e)
//                {
//                    Console.WriteLine(e.ToString());
//                }//catch (SocketException e)
//                catch (Exception e)
//                {
//                    Console.WriteLine(e.ToString());
//                    Console.WriteLine(((IPEndPoint)(clientSock.RemoteEndPoint)).Port + " OUT");
//                    clientSock.Close();
//                }//catch (Exception e)

//            }//while
//        }//main

//        void ListenSocket()
//        {

//        }
//        public static object ByteToStructure(byte[] data, Type type)
//        {
//            IntPtr buff = Marshal.AllocHGlobal(data.Length); // 배열의 크기만큼 비관리 메모리 영역에 메모리를 할당한다.
//            Marshal.Copy(data, 0, buff, data.Length);        // 배열에 저장된 데이터를 위에서 할당한 메모리 영역에 복사한다.
//            object obj = Marshal.PtrToStructure(buff, type); // 복사된 데이터를 구조체 객체로 변환한다.
//            Marshal.FreeHGlobal(buff);                       // 비관리 메모리 영역에 할당했던 메모리를 해제함

//            if (Marshal.SizeOf(obj) != data.Length)          // 구조체와 원래의 데이터의 크기 비교
//            {
//                return null;
//            }
//            return obj;
//        }//ByteToStructure

//        public static byte[] StructureToByte(object obj)
//        {
//            int datasize = Marshal.SizeOf(obj);              // 구조체에 할당된 메모리의 크기를 구한다.
//            IntPtr buff = Marshal.AllocHGlobal(datasize);    // 비관리 메모리 영역에 구조체 크기만큼의 메모리를 할당한다.
//            Marshal.StructureToPtr(obj, buff, false);        // 할당된 구조체 객체의 주소를 구한다.
//            byte[] data = new byte[datasize];                // 구조체가 복사될 배열
//            Marshal.Copy(buff, data, 0, datasize);           // 구조체 객체를 배열에 복사
//            Marshal.FreeHGlobal(buff);                       // 비관리 메모리 영역에 할당했던 메모리를 해제함
//            return data;
//        }//StructureToByte

//        void GetRoomList()
//        {
           
//        }
//    }
//}