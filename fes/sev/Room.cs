using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using def;
using ChatProtocolController;

namespace Servernamespace
{
    [Serializable]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    class Room
    {

        string name;

        [MarshalAs(UnmanagedType.U2)]
        ushort roomNumber;
        
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
        List<Client> roomMember = new List<Client>();

        Socket clientSock = null;

        public Room(string nm,ushort number,Client s)
        {
            name = nm;
            roomNumber = number;
            Console.WriteLine("RoomNumber : "+ roomNumber);
            roomMember.Add(s);
            Thread broadcast = new Thread(Broadcast);
            broadcast.Start();
        }//Room

        public void RoomOpen()                                  //Accept, 리스트 추가
        {
            while (true)
            {
                try
                {
                    //Client client = new Client(clientSock);
                    //roomMember.Add(client);
                    Console.WriteLine("IN : " + roomMember[roomMember.Count - 1].GetSocket().RemoteEndPoint.ToString());
                    clientSock = null;
                }//try
                catch (SocketException e)
                {
                    Console.WriteLine(e.ToString());
                }//catch
            }//while
        }//RoomOpen

        public void Broadcast()
        {
            Socket curSock = null;
            while (true)
            {
                if (0 >= roomMember.Count)
                {
                    //room_remove
                    continue;
                }

                try
                {
                    List<Socket> selectList = new List<Socket>();
                    
                    for (int i = 0; i < roomMember.Count; i++)
                    {
                        selectList.Add(roomMember[i].GetSocket());
                    }

                    Socket.Select(selectList, null, null, 1000);
                    
                    if (selectList.Count == 0)
                    {
                        //heartbeat
                        continue;
                    }
                    for (int i = 0; i < selectList.Count(); i++)
                    {
                        curSock = selectList[i];
                        
                        byte[] recevData = new byte[Marshal.SizeOf(typeof(ChatProtocol))];                      //프로토콜 크기만큼 메모리 할당

                        int rc = curSock.Receive(recevData);
                        if (0 >= rc)
                        {//change to heartbeat
                            ClientOut(curSock);
                            break;
                        }//if

                        ChatProtocol pt = new ChatProtocol();

                        if (!PacketMaker.TryDePacket(recevData, out pt))
                        {
                            Console.WriteLine("trydepacketerror(96line)");
                        }

                        for (int j = 0; j < roomMember.Count; j++)
                        {//Broadcast
                            roomMember[j].GetSocket().Send(recevData,0,recevData.Length,SocketFlags.None);
                        }//for
                    }//selectfor
                }//try
                catch (SocketException e)
                {
                    if (e.ErrorCode != 10054)                       //disconnected
                    {
                        Console.WriteLine("ErrorCode : " + e.ErrorCode.ToString());
                        Console.WriteLine(e.ToString());
                    }
                    ClientOut(curSock);
                }//catch (SocketException e)
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }//catch (Exception e)
            }//while
        }//Broadcast

        
        public ushort GetNumber()
        {
            return roomNumber;
        }
        public int GetCount()
        {
            return roomMember.Count();
        }
        public string Getname()
        {
            return name;
        }

        void ClientOut(Socket s)
        {
            Console.WriteLine("OUT : " + s.RemoteEndPoint.ToString());
            roomMember.Remove(Functions.FindeClient(roomMember,s));
            s.Close();
        }//ClientOut
 
    }
}