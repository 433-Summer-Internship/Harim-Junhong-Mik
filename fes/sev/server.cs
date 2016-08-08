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
using Numbering;

namespace Servernamespace
{
    public class Server
    {
        uint MyserverID;                                                            //fixed ID
        public static Server instance = null;
        RedisDBController redis;
        RoomIDQueue roomIDQueue;                                                    //Room Numbering Class

        List<Socket> clientSocketList = new List<Socket>();                         //in lobby clients list
        List<Socket> selectList = new List<Socket>();
        List<Client> clientList = new List<Client>();                               //login member
        List<Room> roomList = new List<Room>();

        Socket listenSock = null;
        Socket clientSock = null;

        int port;
        public Server(int pp, uint id)
        {
            port = pp;
            MyserverID = id;
            if (instance == null)
                instance = this;
        }
        void RemoveLobbyClient(Socket so)                                           //Remove Clinetinfo in lobby
        {
            clientList.Remove(Functions.FindClient(clientList, so));
            clientSocketList.Remove(so);
        }
        public Room FindRoom(ushort number)
        {
            for (int i = 0; i < roomList.Count; i++)
            {
                if (roomList[i].GetNumber() == number)
                    return roomList[i];
            }
            return null;
        }
        void RedisSet()
        {
            //Connect with Redis DB
            Functions.Log("Connecting to redis server. . .");
            redis = new RedisDBController();
            try
            {
                redis.SetConfigurationOptions("10.100.58.10", 30433, "433redis!");
                redis.SetupConnection();
            }
            catch
            {
                Functions.Log(" . . .Failed!");
                Functions.Log("Please check your server and try again. . . Goodbye!");
                return;
            }
            Functions.Log(" . . .Connected!");
        }


        public void Start()
        {
            RedisSet();
            roomIDQueue = new RoomIDQueue(126);

            listenSock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            listenSock.Bind(new IPEndPoint(IPAddress.Any, port));
            listenSock.Listen(5);

            Thread processThread = new Thread(Process);
            processThread.Start();

            Functions.Log("Process_Thread Start");
            while (true)                                                               //Accpet 
            {
                try
                {
                    clientSock = listenSock.Accept();
                    clientSocketList.Add(clientSock);
                    Functions.Log("Accept : " + clientSock.RemoteEndPoint.ToString());
                    clientSock = null;
                }
                catch (SocketException e)
                {
                    Functions.Log(e.ToString());
                    listenSock.Close();
                }
                catch (Exception e)
                {
                    Functions.Log(e.ToString());
                    listenSock.Close();
                }
            }
        }

        void Process()
        {
            while (true)
            {
                Thread.Sleep(1);
                selectList = new List<Socket>(clientSocketList);

                if (0 >= selectList.Count)
                {
                    continue;
                }

                Socket.Select(selectList, null, null, 1000);

                for (int i = 0; i < selectList.Count; i++)
                {
                    Socket curSock = selectList[i];

                    try
                    {
                        byte[] recevData = new byte[Marshal.SizeOf(typeof(ChatProtocol))];
                        curSock.Receive(recevData);

                        ChatProtocol recvPT = new ChatProtocol();
                        if (!PacketMaker.TryDePacket(recevData, out recvPT))
                        {
                            Functions.Log("trydepacketerror(110line)");
                        }

                        byte[] sendData = new byte[Marshal.SizeOf(typeof(ChatProtocol))];
                        string str = "\n";                                                              //return message
                        byte command = 0;                                                               //sendpacket command
                        ushort fixedLengthField = 0;                                                    //send message length
                        byte[] variableLengthField = new byte[1024];                                    //send message
                        byte[] inputdata;                                                               //byte array to convert

                        switch (recvPT.command)
                        {
                            #region login
                            case (int)PacketMaker.CommandCode.LOGIN:
                                // parsing ID,PW
                                int length = recvPT.fixedLengthField;
                                string[] idpw = Encoding.UTF8.GetString(recvPT.variableLengthField).Split('#');

                                str = "-1";                                                             //defualt error
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

                                        #region oldcennection close
                                        if (redis.IsUserLoggedIn(idpw[0]) == 1)                         //oldconnection disconnected
                                        {//logged in check
                                            int oldRoomNumber;
                                            Socket oldSocket = null;

                                            oldRoomNumber = redis.GetUserLocation(idpw[0]);

                                            if (oldRoomNumber == 0)
                                            {//location == lobby

                                                if (Functions.FindClient(clientList, idpw[0]) != null)
                                                {//oldsocket close
                                                    oldSocket = Functions.FindClient(clientList, idpw[0]).GetSocket();
                                                    clientList.Remove(Functions.FindClient(clientList, idpw[0]));
                                                    clientSocketList.Remove(oldSocket);
                                                    oldSocket.Close();
                                                }
                                            }
                                            else//in room
                                            {
                                                Room oldRoom = FindRoom((ushort)oldRoomNumber);
                                                if (oldRoom != null)
                                                {
                                                    if (oldRoom.GetMember(idpw[0]) != null)
                                                    {
                                                        Socket oldSock = oldRoom.GetMember(idpw[0]).GetSocket();
                                                        if (oldSock != null)
                                                            oldRoom.ClientOut(oldSock);
                                                    }
                                                }
                                            }

                                            redis.Logout(idpw[0]);
                                        }

                                        #endregion

                                        errorCheck = redis.Login(idpw[0], idpw[1], 1111, idpw[0][0] == '!' ? true : false);        //If account creation didn't result in an error, we will attempt a login and record the result
                                    }

                                    switch (errorCheck)                                                 //If we obtained a connection ID number, the login was a success
                                    {
                                        case -2:
                                            //str = "Cannot create account; username is already taken!";
                                            str = "-2";
                                            break;
                                        case -1:
                                            str = "-1";
                                            break;
                                        case 0:
                                            //str = "Username or password is incorrect!";
                                            str = "-1";
                                            break;
                                        default:   // "Login successful!";                            //The returned connection ID number is an OLD connection ID number (to handle re-logins)
                                            Client cli = new Client(curSock, idpw[0]);
                                            clientList.Add(cli);
                                            Functions.Log("Login " + cli.GetID());
                                            str = "1";

                                            break;
                                    }
                                }//if(idpw.Length == 2)
                                if (str != "1")
                                    Functions.Log(curSock.RemoteEndPoint.ToString() + " Login Failed");

                                command = PacketMaker.CommandCode.LOGIN_RESULT;

                                inputdata = BitConverter.GetBytes(Int32.Parse(str));
                                inputdata.CopyTo(variableLengthField, 0);
                                fixedLengthField = (ushort)inputdata.Length;

                                break;
                            #endregion login

                            #region createroom
                            case (int)PacketMaker.CommandCode.CREATE_ROOM:

                                Functions.Log(curSock.RemoteEndPoint.ToString() + " createroom(lobby)");

                                if (Functions.FindClient(clientList, curSock) == null)
                                {
                                    str = "-1";
                                    Functions.Log(curSock.RemoteEndPoint.ToString() + " CreateRoom_faild, login first");
                                    inputdata = BitConverter.GetBytes(Int32.Parse(str));
                                    inputdata.CopyTo(variableLengthField, 0);
                                    fixedLengthField = (ushort)inputdata.Length;
                                    break;
                                }

                                string roomtitle = Encoding.UTF8.GetString(recvPT.variableLengthField, 0, recvPT.fixedLengthField);
                                ushort id = (ushort)roomIDQueue.GetRoom();
                                while (redis.RoomCreate(id, roomtitle, Functions.FindClient(clientList, curSock).GetID(), 1) == -1)
                                {
                                    id = roomIDQueue.GetRoom();
                                    if (id == 0)                        //roomlist full
                                    {
                                        str = "-2";
                                    }
                                    else
                                    {
                                        Room ro = new Room(roomtitle, id, redis);
                                        roomList.Add(ro);
                                        str = ro.GetNumber().ToString();
                                    }
                                }

                                command = PacketMaker.CommandCode.CREATE_ROOM_RESULT;

                                inputdata = BitConverter.GetBytes(Int32.Parse(str));
                                inputdata.CopyTo(variableLengthField, 0);
                                fixedLengthField = (ushort)inputdata.Length;

                                break;

                            #endregion createroom
                            #region joinRoome
                            case (int)PacketMaker.CommandCode.JOIN_ROOM:

                                if (Functions.FindClient(clientList, curSock) == null)
                                {//login check
                                    str = "-1";
                                    Functions.Log(curSock.RemoteEndPoint.ToString() + "+JOIN_ROOM, login first");
                                    inputdata = BitConverter.GetBytes(Int32.Parse(str));
                                    inputdata.CopyTo(variableLengthField, 0);
                                    fixedLengthField = (ushort)inputdata.Length;
                                    break;
                                }

                                ushort roomNumber = recvPT.fixedLengthField;        //request room number

                                //-------------------------------------------------------------------connection passing
                                //check if has room
                                if (roomList.Count <= 0 && (FindRoom(roomNumber) == null))
                                {
                                    uint serverId;
                                    if (redis.RoomGetServerID(roomNumber, out serverId))
                                    {//exist
                                        if (serverId != MyserverID)
                                        {
                                            command = PacketMaker.CommandCode.CONNECTIOINPASSING_REQUEST;
                                            str = serverId.ToString();
                                            inputdata = BitConverter.GetBytes(Int32.Parse(str));
                                            inputdata.CopyTo(variableLengthField, 0);
                                            fixedLengthField = (ushort)inputdata.Length;
                                            break;
                                            //command,length,serverid
                                        }
                                        else
                                        {
                                            redis.RoomPurge(roomNumber);
                                            str = "-2";                                                             //Room no exist
                                        }
                                    }
                                    else
                                    {//no longer exist

                                        str = "-1";
                                        Functions.Log(Functions.FindClient(clientList, curSock).GetID() + " - JoinRoomFailed");
                                    }
                                }//-------------------------------------------------------------------connection passing
                                else
                                {
                                    Functions.Log(Functions.FindClient(clientList, curSock).GetID() + " - JoinRoom " + roomNumber);

                                    redis.RoomAddUser(roomNumber, Functions.FindClient(clientList, curSock).GetID());                  //EnterRoom

                                    FindRoom(roomNumber).JoinRoom(Functions.FindClient(clientList, curSock));
                                    RemoveLobbyClient(curSock);
                                    str = "1";
                                }
                                command = PacketMaker.CommandCode.JOIN_ROOM_RESULT;
                                inputdata = BitConverter.GetBytes(Int32.Parse(str));
                                inputdata.CopyTo(variableLengthField, 0);
                                fixedLengthField = (ushort)inputdata.Length;

                                break;
                            #endregion joinRoome
                            #region rommlist
                            case (int)PacketMaker.CommandCode.ROOM_LIST_REQUEST:

                                Functions.Log(Functions.FindClient(clientList, curSock).GetID() + " - Room_List_REQUEST");

                                if (Functions.FindClient(clientList, curSock) == null)
                                {
                                    str = "-1";
                                    Functions.Log(curSock.RemoteEndPoint.ToString() + " CreateRoom_faild, login first");
                                    inputdata = BitConverter.GetBytes(Int32.Parse(str));
                                    inputdata.CopyTo(variableLengthField, 0);
                                    fixedLengthField = (ushort)inputdata.Length;
                                    break;
                                }
                                fixedLengthField = 0;

                                string[] number = redis.RoomList();
                                if (number == null)
                                {
                                    Functions.Log("redis disconnected");
                                    break;
                                }
                                if (number.Length == 0)
                                {
                                    str += "No rooms exist.";
                                    command = PacketMaker.CommandCode.ROOM_LIST_SEND;
                                    inputdata = Encoding.UTF8.GetBytes(str);
                                    inputdata.CopyTo(variableLengthField, 0);
                                    fixedLengthField = (ushort)inputdata.Length;
                                }

                                int len = number.Length;

                                RoomInfoDatum[] roominfos = new RoomInfoDatum[len];

                                for (int roomCount = 0; roomCount < len; roomCount++)
                                {
                                    byte num = byte.Parse(number[roomCount].Substring(5));
                                    roominfos[roomCount].roomNumber = num;
                                    redis.RoomGetTitle((uint)roomCount, out roominfos[roomCount].roomTitle);
                                    roominfos[roomCount].userCount = (byte)redis.RoomGetUserCount((uint)num);
                                }

                                int RT = len / 46;
                                if (len % 46 != 0)
                                    RT++;

                                command = PacketMaker.CommandCode.ROOM_LIST_REQUEST_RESULT;
                                for (int order = 0; order < RT; order++)
                                {
                                    fixedLengthField = 0;
                                    variableLengthField[0] = (byte)(order + 1);
                                    variableLengthField[1] = (byte)(RT);

                                    for (int rcnt = 0 + (order * 46); rcnt < (order * 46) + ((int)(len / 46) != 0 ? 46 : len); rcnt++)
                                    {
                                        PacketMaker.RoomInfoDatumToByte(roominfos[rcnt]).CopyTo(variableLengthField, 2 + (rcnt * 22) - (order * 46));
                                        fixedLengthField++;
                                    }
                                    len -= 46;
                                    //send;
                                    sendData = PacketMaker.CreatePacket(command, fixedLengthField, variableLengthField);
                                    curSock.Send(sendData, 0, sendData.Length, SocketFlags.None);
                                }
                                command = 0;

                                break;
                            #endregion roomlist

                            case (int)PacketMaker.CommandCode.CONNECTIOINPASSING_RESULT:

                                //command,length,ID#RoomNomber
                                string[] data = Encoding.UTF8.GetString(recvPT.variableLengthField).Split('#');
                                string passingID = data[0];
                                ushort passingRoomNumber = ushort.Parse(data[1]);

                                Functions.Log("passing try" + passingID);

                                //create user information
                                Client passingClient = new Client(curSock, passingID);
                                clientList.Add(passingClient);
                                clientSocketList.Add(curSock);

                                //login only?
                                if (passingRoomNumber != 0)
                                {
                                    Room passingRoome = FindRoom(passingRoomNumber);
                                    if (passingRoome != null)
                                    {
                                        if (FindRoom(passingRoomNumber).GetCount() < 5)
                                        {
                                            Functions.Log(Functions.FindClient(clientList, curSock).GetID() + " - JoinRoom " + passingRoomNumber);

                                            redis.RoomAddUser(passingRoomNumber, passingID);                  //EnterRoom

                                            FindRoom(passingRoomNumber).JoinRoom(passingClient);
                                            RemoveLobbyClient(curSock);
                                            str = "1";
                                        }
                                        else
                                        {
                                            str = "-1";
                                            Functions.Log(Functions.FindClient(clientList, curSock).GetID() + " - JoinRoomFailed");
                                        }
                                        command = PacketMaker.CommandCode.JOIN_ROOM_RESULT;
                                        inputdata = BitConverter.GetBytes(Int32.Parse(str));
                                        inputdata.CopyTo(variableLengthField, 0);
                                        fixedLengthField = (ushort)inputdata.Length;
                                        break;
                                    }
                                    else
                                    {
                                        redis.RoomPurge(passingRoomNumber);
                                        str = "-2";                                                            //room no exist
                                        Functions.Log(Functions.FindClient(clientList, curSock).GetID() + " - JoinRoomFailed");

                                        command = PacketMaker.CommandCode.JOIN_ROOM_RESULT;
                                        inputdata = BitConverter.GetBytes(Int32.Parse(str));
                                        inputdata.CopyTo(variableLengthField, 0);
                                        fixedLengthField = (ushort)inputdata.Length;
                                        break;
                                    }
                                }
                                Functions.Log("passing Login" + passingID);
                                str = "1";

                                command = PacketMaker.CommandCode.JOIN_ROOM_RESULT;
                                inputdata = BitConverter.GetBytes(Int32.Parse(str));
                                inputdata.CopyTo(variableLengthField, 0);
                                fixedLengthField = (ushort)inputdata.Length;

                                break;

                            case PacketMaker.CommandCode.LOGOUT:

                                Client logoutClient = Functions.FindClient(clientList, curSock);
                                if (logoutClient != null)
                                    break;

                                Functions.Log(logoutClient.GetID() + " logout");

                                command = PacketMaker.CommandCode.LOGOUT_RESULT;
                                str = "1";

                                clientList.Remove(logoutClient);
                                redis.Logout(logoutClient.GetID());

                                inputdata = BitConverter.GetBytes(Int32.Parse(str));
                                inputdata.CopyTo(variableLengthField, 0);
                                fixedLengthField = (ushort)inputdata.Length;

                                break;

                            default:
                                Functions.Log(Functions.FindClient(clientList, curSock).GetID() + " - Out of command range");
                                str = "Out of command range";
                                inputdata = Encoding.UTF8.GetBytes(str);
                                inputdata.CopyTo(variableLengthField, 0);
                                fixedLengthField = (ushort)inputdata.Length;
                                break;
                        }//swich
                        if (command != 0)
                        {

                            sendData = PacketMaker.CreatePacket(command, fixedLengthField, variableLengthField);
                            curSock.Send(sendData, 0, sendData.Length, SocketFlags.None);
                        }

                    }//try
                    catch (SocketException e)
                    {
                        if (e.ErrorCode != 10054)                       //disconnected
                        {
                            Functions.Log("ErrorCode : " + e.ErrorCode.ToString());
                            Functions.Log(e.ToString());
                        }
                        Functions.Log("OUT : " + curSock.RemoteEndPoint.ToString());
                        clientSocketList.Remove(curSock);
                        clientList.Remove(Functions.FindClient(clientList, curSock));
                        curSock.Close();
                    }//catch (SocketException e)
                    catch (Exception e)
                    {
                        Functions.Log(e.ToString());
                        if (clientSock != null)
                            clientSock.Close();
                    }//catch (Exception e)
                }//for
            }
        }

        public static bool RemoveRoom(Room ro)
        {
            if (ro == null)
                return false;
            Functions.Log("Room_Number : " + ro.GetNumber() + "Remove");
            instance.roomList.Remove(ro);
            return true;
        }
        public static bool AddLobbyClient(Client c)
        {
            if (c == null)
                return false;

            instance.clientSocketList.Add(c.GetSocket());
            instance.clientList.Add(c);

            return true;
        }
    }
}