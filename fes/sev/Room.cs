using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Timers;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using def;
using ChatProtocolController;
using MikRedisDB;

namespace Servernamespace
{
    public class Room
    {
        RedisDBController redis;

        string name;
        ushort roomNumber;
        Timer broadcast;
        Timer heartbeat;
        List<Client> roomMember = new List<Client>();

        public Room(string nm, ushort number, RedisDBController redisDB)
        {
            name = nm;
            roomNumber = number;
            redis = redisDB;
            Functions.Log("createroom : " + roomNumber);

            broadcast = new Timer(10);
            heartbeat = new Timer(1000);

            //broadcast.Elapsed += new ElapsedEventHandler(Broadcast);
            heartbeat.Elapsed += new ElapsedEventHandler(Broadcast);

            //broadcast.Enabled = true;
            heartbeat.Enabled = true;

        }//Room

        public void JoinRoom(Client s)
        {
            roomMember.Add(s);
        }
        
        public void Broadcast(object sender, ElapsedEventArgs events)
        {
            Socket curSock = null;
            {
                if (0 >= roomMember.Count)
                {
                    return;
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
                        return; 
                    }
                    for (int i = 0; i < selectList.Count(); i++)
                    {
                        curSock = selectList[i];
                        
                        byte[] recevData = new byte[Marshal.SizeOf(typeof(ChatProtocol))];                      //프로토콜 크기만큼 메모리 할당

                        try
                        {
                            int rc = curSock.Receive(recevData);
                            if (0 >= rc)
                            {
                                ClientOut(curSock);
                                break;
                            }
                        }
                        catch(SocketException e)
                        {
                            if (e.ErrorCode == 10054)                       //disconnected
                            {
                                Functions.Log("ErrorCode : " + e.ErrorCode.ToString());
                                ClientOut(curSock);
                            }
                        }

                        ChatProtocol recvpt = new ChatProtocol();
                        ChatProtocol sendpt = new ChatProtocol();
                        byte[] sendData = new byte[Marshal.SizeOf(typeof(ChatProtocol))];
                        byte[] variableLengthField = new byte[1024];
                        string str = "";
                        byte[] inputdata;

                        if (!PacketMaker.TryDePacket(recevData, out recvpt))
                        {
                            Functions.Log("trydepacketerror(96line)");
                        }

                        switch (recvpt.command)
                        {
                            case PacketMaker.CommandCode.LEAVE_ROOM:

                                Functions.Log(Functions.FindClient(roomMember, curSock).GetID() + " Leave_Room");

                                sendpt.command = PacketMaker.CommandCode.LEAVE_ROOM_RESULT;
                                str = "1";

                                if (!LeaveRoom(Functions.FindClient(roomMember, curSock)))
                                {
                                    str = "-1";
                                }

                                inputdata = BitConverter.GetBytes(Int32.Parse(str));
                                inputdata.CopyTo(variableLengthField, 0);
                                sendpt.fixedLengthField = (ushort)inputdata.Length;
                                sendData = PacketMaker.CreatePacket(sendpt.command, sendpt.fixedLengthField, variableLengthField);
                                curSock.Send(sendData, 0, sendData.Length, SocketFlags.None);

                                break;

                            case PacketMaker.CommandCode.MESSAGE_TO_SERVER:

                                string temp = Encoding.UTF8.GetString(recvpt.variableLengthField, 0, recvpt.fixedLengthField);

                                try
                                {
                                    Functions.FindClient(roomMember, curSock).SetMsgCount(redis.AddToUserMessageCount(Functions.FindClient(roomMember, curSock).GetID(), 1));

                                    sendpt.command = PacketMaker.CommandCode.MESSAGE_TO_CLIENT;
                                    str += Functions.FindClient(roomMember, curSock).GetID() + " :" + temp;

                                    inputdata = Encoding.UTF8.GetBytes(str);
                                    inputdata.CopyTo(variableLengthField, 0);
                                    sendpt.fixedLengthField = (ushort)inputdata.Length;

                                    sendData = PacketMaker.CreatePacket(sendpt.command, sendpt.fixedLengthField, variableLengthField);

                                    byte[] sendData2 = new byte[Marshal.SizeOf(typeof(ChatProtocol))];
                                    string str2 = "";
                                    str2 += "ME :" + temp;

                                    inputdata = Encoding.UTF8.GetBytes(str2);
                                    inputdata.CopyTo(variableLengthField, 0);
                                    sendpt.fixedLengthField = (ushort)inputdata.Length;

                                    sendData2 = PacketMaker.CreatePacket(sendpt.command, sendpt.fixedLengthField, variableLengthField);

                                    Console.WriteLine(temp);
                                    //Broadcast
                                    for (int j = 0; j < roomMember.Count; j++)
                                    {
                                        if (roomMember[j].GetSocket() == curSock)
                                        {
                                            curSock.Send(sendData2);
                                            continue;
                                        }

                                        roomMember[j].GetSocket().Send(sendData);
                                    }//for
                                }
                                catch (OutOfMemoryException e)
                                {
                                    Functions.Log(e.ToString());
                                    byte co = PacketMaker.CommandCode.MESSAGE_TO_SERVER_RESULT;
                                    string er = "MessageToserver";
                                    ushort le = (ushort)er.Length;
                                    byte[] M2Serr = PacketMaker.CreatePacket(co, le, Encoding.UTF8.GetBytes(er));
                                    curSock.Send(M2Serr);
                                }
                                break;

                            default:
                                Functions.Log(Functions.FindClient(roomMember, curSock).GetID() + " - Out of command range");
                                str = "Out of command range";
                                inputdata = Encoding.UTF8.GetBytes(str);
                                inputdata.CopyTo(variableLengthField, 0);
                                sendpt.fixedLengthField = (ushort)inputdata.Length;
                                sendData = PacketMaker.CreatePacket(sendpt.command, sendpt.fixedLengthField, sendpt.variableLengthField);
                                curSock.Send(sendData, 0, sendData.Length, SocketFlags.None);
                                break;
                        }
                    }//selectfor
                }//try
                catch (SocketException e)
                {
                    if (e.ErrorCode == 10054)                       //disconnected
                    {
                        Functions.Log("ErrorCode : " + e.ErrorCode.ToString());
                        ClientOut(curSock);
                    }
                    if (e.ErrorCode == 10038)                       //socket_closed
                    {
                        RemoveUser(curSock);
                        Functions.Log("Disconnection : " + curSock.RemoteEndPoint.ToString());
                    }
                    else
                    {
                        Functions.Log("ErrorCode : " + e.ErrorCode.ToString());
                        Functions.Log(e.ToString());
                    }
                }//catch (SocketException e)
                catch(ObjectDisposedException e)
                {
                    Functions.Log(e.HelpLink);
                }
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
       
        bool LeaveRoom(Client c)
        {
            int a = redis.RoomRemoveUser((uint)this.GetNumber(), c.GetID());
            
            if (!Server.AddLobbyClient(c))
                return false;

            roomMember.Remove(c);
            if (roomMember.Count <= 0)
            {
                if (!Server.RemoveRoom((Room)this))
                {
                    return false;
                }
            }
                    heartbeat.Enabled = false;

            return true;
        }
        public Client GetMember(string id)
        {
            for (int i = 0; i < roomMember.Count; i++)
            {
                if (roomMember[i].GetID() == id)
                    return roomMember[i];
            }
            return null;
        }

        void RemoveUser(Socket s)
        {
            redis.RoomRemoveUser(this.GetNumber(), Functions.FindClient(roomMember, s).GetID());
            roomMember.Remove(Functions.FindClient(roomMember, s));
        }
        public void ClientOut(Socket s)
        {
            RemoveUser(s);
            Functions.Log("OUT : " + s.RemoteEndPoint.ToString());
            s.Close();
        }//ClientOut
    }
}