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
using MikRedisDB;
using ChatProtocolController;
/*
login 10
logout 11
MessageToServer 21
MessageToClient 22
JoinRoom 31
CreateRoom 32
LeaveRoom 33
RoomList 40
UserList 50
heatbeat 60
*/

namespace Servernamespace
{
    public class Server
    {
        public static Server instance = null;
        RedisDBController redis;
        List<Socket> clientSocketList = new List<Socket>();                         //in lobby clients list
        List<Socket> selectList = new List<Socket>();                               
        List<Client> clientList = new List<Client>();                               //login member
        List<Room> roomList = new List<Room>();

        Socket listenSock = null;
        Socket clientSock = null;

        public Server()
        {
            if (instance != null)
                instance = this;
        }
        void RemoveLobbyClient(Socket so)
        {
            clientList.Remove(Functions.FindeClient(clientList, so));
            clientSocketList.Remove(so);
        }
        public void AcceptThread()
        {
            //Connect with Redis DB
            Console.Write("Connecting to redis server. . .");
            redis = new RedisDBController();
            try
            {
                redis.SetConfigurationOptions("10.100.58.10", 30433, "433redis!");
                redis.SetupConnection();
            }
            catch
            {
                Console.WriteLine(" . . .Failed!");
                Console.WriteLine("Please check your server and try again. . . Goodbye!");
                return;
            }
            Console.WriteLine(" . . .Connected!");

            listenSock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            listenSock.Bind(new IPEndPoint(IPAddress.Any, 50001));
            listenSock.Listen(5);

            Thread t = new Thread(ProcessThread);
            t.Start();
            while (true)                                                               //Accpet 
            {
                try
                {
                    clientSock = listenSock.Accept();
                    clientSocketList.Add(clientSock);
                    Console.WriteLine("IN : " + clientSock.RemoteEndPoint.ToString());
                    clientSock = null;
                }
                catch (SocketException e)
                {
                    Console.WriteLine(e.ToString());
                    listenSock.Close();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                    listenSock.Close();
                }
            }
        }
        void ProcessThread()
        {
            while (true)
            {
                selectList = new List<Socket>(clientSocketList);
                selectList.Add(listenSock);
                Socket.Select(selectList, null, null, 1000);
                
                for (int i = 0; i < selectList.Count; i++)
                {

                    Socket curSock = selectList[i];

                    try
                    {
                        byte[] recevData = new byte[Marshal.SizeOf(typeof(ChatProtocol))];                      //프로토콜 크기만큼 메모리 할당
                        int rc = curSock.Receive(recevData);
                        ChatProtocol recvPT = new ChatProtocol();
                        if(!PacketMaker.TryDePacket(recevData, out recvPT))
                        {
                            Console.WriteLine("trydepacketerror(110line)");
                        }

                        
                        byte[] sendData = new byte[Marshal.SizeOf(typeof(ChatProtocol))];
                        string str = "\n";
                        byte command = 0;
                        ushort fixedLengthField = 0;
                        byte[] variableLengthField = new byte[1024];
                        
                        switch (recvPT.command)
                        {

                            #region login
                            case (int)PacketMaker.CommandCode.LOGIN:

                                // 아이디랑 비밀번호 나누기
                                int length = recvPT.fixedLengthField;
                                string[] idpw = Encoding.UTF8.GetString(recvPT.variableLengthField).Split('#');

                                str = "-1";
                                if (idpw.Length == 2)
                                {
                                    int errorCheck = 0;                                                 //Keep track of the results of the process
                                    if (!redis.DoesUsernameExist(idpw[0]))                              //If the username does not exist, create a new account...
                                    {
                                        if (!redis.CreateUser(idpw[0], idpw[1]))                        //Attempt to create an account
                                        {
                                            errorCheck = -2;                                            //If failed, mark -2 so we can report back a failure
                                        }
                                    }
                                    if (errorCheck != -2)
                                    {
                                        errorCheck = redis.Login(idpw[0], idpw[1], 1111, false);        //If account creation didn't result in an error, we will attempt a login and record the result
                                    }
                                    //Fail Code
                                    //-2 = Unable to create account (username already taken)
                                    //-1 = Account created, but user is blocked
                                    //0 = username or password is incorrect
                                    switch (errorCheck)                                                 //If we obtained a connection ID number, the login was a success
                                    {
                                        case -2:
                                            //str = "Cannot create account; username is already taken!";
                                            str = "-2";
                                            //sendpt.variableLengthField = BitConverter.GetBytes(-1);
                                            break;
                                        case -1:
                                            //str = "You are blocked!";
                                            str = "-1";
                                            break;
                                        case 0:
                                            //str = "Username or password is incorrect!";
                                            str = "-1";
                                            break;
                                        default:   // "Login successful!";                            //The returned connection ID number is an OLD connection ID number (to handle re-logins)
                                            Client cli = new Client(curSock, idpw);
                                            clientList.Add(cli);

                                            str = "1";

                                            break;
                                    }
                                }//if(idpw.Length == 2)

                                command = PacketMaker.CommandCode.LOGIN_RESULT;
                                BitConverter.GetBytes(Int32.Parse(str)).CopyTo(variableLengthField, 0);
                                fixedLengthField = (ushort)str.Length;

                                break;
                            #endregion login
                            case (int)PacketMaker.CommandCode.CREATE_ROOM:
                                //variableLengthField가 이름
                                //a가 길이
                                //클라이언트리스트에서 널이 아니면
                                if (Functions.FindeClient(clientList, curSock) == null)
                                {
                                    str = "login first";
                                    Encoding.UTF8.GetBytes(str).CopyTo(variableLengthField, 0);
                                    //sendpt.variableLengthField = bt;
                                    //sendpt.fixedLengthField = (ushort)str.Length;
                                    fixedLengthField = (ushort)str.Length;
                                    break;
                                }


                                //방 번호가 있으면 else
                                //룸 만들고
                                int cnt = roomList.Count + 1;
                                //리스트 추가
                                
                                Room ro = new Room(Encoding.UTF8.GetString(recvPT.variableLengthField, 0, recvPT.fixedLengthField), (ushort)cnt, Functions.FindeClient(clientList, curSock));
                                roomList.Add(ro);

                                //str += "[";
                                //str += string.Format("{0:000}", ro.GetNumber());
                                //str += "] " + ro.Getname() + " join\n";


                                RemoveLobbyClient(curSock);




                                str = ro.GetNumber().ToString();
                                command = PacketMaker.CommandCode.CREATE_ROOM_RESULT;
                                fixedLengthField = (ushort)str.Length;
                                
                                //Encoding.UTF8.GetBytes(str).CopyTo(variableLengthField, 0);
                                BitConverter.GetBytes(Int32.Parse(str)).CopyTo(variableLengthField, 0);
                                


                                //else
                                //방 만들기 실패
                                //if(이미 방에 접속해 있음)

                                break;

                            case (int)PacketMaker.CommandCode.JOIN_ROOM:

                            //success: valueB = 1
                            //fail: valueB = -1
                                ushort roomNumber = recvPT.fixedLengthField;

                                //check room
                                if (roomList.Count != 0 && roomList[roomNumber] != null)
                                    str = "dd";
                                else
                                    str = "failed";
                                //있으면 입장

                                //아니면 실패

                                //sendpt.fixedLengthField = (ushort)str.Length;
                                fixedLengthField = (ushort)str.Length;
                                Encoding.UTF8.GetBytes(str).CopyTo(variableLengthField, 0);
                                //sendpt.variableLengthField = bt;

                                break;
                            #region rommlist
                            case (int)PacketMaker.CommandCode.ROOM_LIST_REQUEST:

                                if (0 >= roomList.Count)
                                {
                                    //sendpt.command = (byte)command.SendRoomList;
                                    command = (byte)PacketMaker.CommandCode.ROOM_LIST_SEND;
                                }
                                if (0 >= roomList.Count)
                                {
                                    str = "Empty";
                                }
                                else
                                {
                                    for (int j = 0; j < roomList.Count; j++)
                                    {
                                        str += "[";
                                        str += string.Format("{0:000}", roomList[i].GetNumber());
                                        str += "] "+ roomList[i].Getname() + " Current "+ roomList[i].GetCount() + " / 5 \n";
                                    }
                                }

                                //sendpt.fixedLengthField = (ushort)str.Length;
                                fixedLengthField = (ushort)str.Length;
                                 Encoding.UTF8.GetBytes(str).CopyTo(variableLengthField, 0);
                                //sendpt.variableLengthField = bt;

                                break;
                            #endregion rommlist

                            default:
                                str = "error";
                                Encoding.UTF8.GetBytes(str).CopyTo(variableLengthField, 0);
                                fixedLengthField = (ushort)str.Length;
                                break;

                        }//swich
                        sendData = PacketMaker.CreatePacket(command,fixedLengthField,variableLengthField);
                        curSock.Send(sendData, 0, sendData.Length, SocketFlags.None);

                    }//try
                    catch (SocketException e)
                    {
                        if (e.ErrorCode != 10054)                       //disconnected
                        {
                            Console.WriteLine("ErrorCode : " + e.ErrorCode.ToString());
                            Console.WriteLine(e.ToString());
                        }
                        Console.WriteLine("OUT : " + curSock.RemoteEndPoint.ToString());
                        clientSocketList.Remove(curSock);
                        clientList.Remove(Functions.FindeClient(clientList, curSock));
                        curSock.Close();
                    }//catch (SocketException e)
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                        clientSock.Close();
                    }//catch (Exception e)
                }//for
            }
        }
    }
}



/*
                


            }//while
        }//main
    }//server

}
*/