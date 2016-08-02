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
using ChatProtocolController;




namespace client
{
    class client
    {
        static void Main(string[] args)
        {
            //connect to server
            Socket clientSocket = ConnectToServer();
            
            while(true)
            {
                //default state
                FirstUI();
                
                string command = GetCommand();
                //login state
                if (command.Equals("LOGIN"))
                {
                    //try login
                    LogIn(clientSocket);

                    bool loginResult = LogInAccept(clientSocket);

                    if (loginResult)
                    {
                        string lobbyCommand = "";
                        
                        //login success
                        do
                        {
                            //lobby state
                            LobbyUI();

                            lobbyCommand = GetCommand();

                            switch (lobbyCommand)
                            {
                                case "LIST":
                                    //ShowList();
                                    //JoinRoom();
                                    break;

                                case "JOIN":

                                    int roomNum = Convert.ToInt32(Console.ReadLine());
                                    JoinRoom(roomNum, clientSocket);
                                    break;

                                case "CREATE":

                                    int roomNumber = CreateRoom(clientSocket);

                                    if (roomNumber == -1)
                                    {
                                        Console.WriteLine("Can not create new room");
                                    }
                                    else
                                    {
                                       
                                       // JoinRoom(roomNumber, clientSocket);
                                    }

                                    break;

                                case "LOGOUT":
                                    //LogOut();
                                    //logout success

                                    Console.WriteLine("Logout");
                                    break;

                                default :
                                    Console.WriteLine("Invalid Command");
                                    break;
                            }
                        } while (!lobbyCommand.Equals("LOGOUT"));
                    }
                    else
                    {
                        continue;
                    }

                }
                else if (command.Equals("EXIT"))
                {
                    clientSocket.Close();
                    break;
                }
                else
                {
                    Console.WriteLine("Invalid Command");
                }
                //Login State End
            } //While End
        }//main ends


        public static string GetCommand()
        {
            string command = Console.ReadLine().ToUpper();
            return command;
        }


        public static void FirstUI()
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

            //Get correct login info
            do
            {
                Console.Write("Enter your ID : ");
                id = Console.ReadLine();
            }
            while (!IsValidID(id, idMaxSize));

            do
            {
                Console.Write("Enter your PW : ");
                pw = Console.ReadLine();
            }
            while (!IsValidPW(pw, idMaxSize));

            //Send login info to server
            string loginInfo = id + "#" + pw;
            
            byte[] loginData = PacketMaker.CreatePacket(PacketMaker.CommandCode.LOGIN, Convert.ToUInt16(loginInfo.Length), StringToByte(loginInfo));
            
            ClientToServer(s, loginData);
        }

        public static bool LogInAccept(Socket s)
        {

            ChatProtocol result = ServerToClient(s);

            if (result.command == PacketMaker.CommandCode.LOGIN_RESULT)
            {
                if (BitConverter.ToInt32(result.variableLengthField, 0) == 1)
                {
                    Console.WriteLine("Welcome!");
                    return true;
                }
                else if (BitConverter.ToInt32(result.variableLengthField, 0) == -1)
                {
                    Console.WriteLine("LogIn Failed");
                    return false;
                }
                else
                {
                    Console.WriteLine(BitConverter.ToInt32(result.variableLengthField, 0));
                    Console.WriteLine("valueB error.");
                    return false;
                }
            }
            else
            {
                Console.WriteLine("command error.");
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

        public static void LobbyUI()
        {
            Console.WriteLine("==========================================================");
            Console.WriteLine("==================POCKETCHAT BETA ver.1.0=================");
            Console.WriteLine("==========================================================");
            Console.WriteLine("========LIST========JOIN=======CREATE========LOGOUT=======");
        }

        public static void JoinRoom(int roomNum, Socket s)
        {
            byte[] joinRoomRequest 
                = PacketMaker.CreatePacket(PacketMaker.CommandCode.JOIN_ROOM, Convert.ToUInt16(roomNum), StringToByte("0"));

            ClientToServer(s, joinRoomRequest);
            ChatProtocol joinResult = ServerToClient(s);

            if (joinResult.command == PacketMaker.CommandCode.JOIN_ROOM_RESULT)
            {
                if (BitConverter.ToInt32(joinResult.variableLengthField, 0) == 1)
                {
                    Console.WriteLine("Join to Room # : " + joinResult.variableLengthField);
                    Chat();
                }
                else if (BitConverter.ToInt32(joinResult.variableLengthField, 0) == -1)
                {
                    Console.WriteLine("Join Room Failed");
                }
            }else {
                Console.WriteLine("Invaild Message from Server");
            }
                
        }

        public static int CreateRoom(Socket s)
        {
            int roomNumber = -1;
            int maxRoomNameLength = 60;
            string roomName;

            do
            {
                Console.WriteLine("Enter Room Name");
                roomName = Console.ReadLine();

                if (roomName.Length > maxRoomNameLength)
                {
                    Console.WriteLine("방 이름이 너무 길어양");
                }
            } while (roomName.Length > maxRoomNameLength);

            ushort roomNameLength = Convert.ToUInt16(roomName.Length);
            byte[] newRoomRequest =  PacketMaker.CreatePacket(PacketMaker.CommandCode.CREATE_ROOM, roomNameLength, StringToByte(roomName));
            ClientToServer(s, newRoomRequest);

            ChatProtocol newRoom = ServerToClient(s);

            if (newRoom.command == PacketMaker.CommandCode.CREATE_ROOM_RESULT)
            {
                roomNumber = newRoom.fixedLengthField;
            }else
            {
                Console.WriteLine("command error");
            }

            return roomNumber;
        }


        public static void Chat()
        {
            Console.ReadLine();
        }

        public static Socket ConnectToServer()
        {
            string serverIP = SelectServer();
            IPAddress serip = IPAddress.Parse(serverIP);
            IPEndPoint serep = new IPEndPoint(serip, 50001);

            Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            Console.WriteLine("Create Socket");

            sock.Connect(serep);
            Console.WriteLine("Connect to Server : " + sock.RemoteEndPoint);

            return sock;
        }


        public static byte[] StringToByte(string str)
        {
            byte[] message = new byte[1024];    //max message size
            Array.Copy(Encoding.UTF8.GetBytes(str), message, Encoding.UTF8.GetBytes(str).Length);
            return message;
        }


        public static void ClientToServer(Socket s, byte[] data)
        {
            Console.WriteLine("Before Send");
            s.Send(data, 0, data.Length, SocketFlags.None);
            Console.WriteLine("After Send");
        }


        public static ChatProtocol ServerToClient(Socket s)
        {
            ChatProtocol pt;
            byte[] data = new byte[Marshal.SizeOf(typeof(ChatProtocol))];

            Console.WriteLine("rcv ready");
            s.Receive(data, data.Length, SocketFlags.None);
            Console.WriteLine("rcv end");

            if(!PacketMaker.TryDePacket(data, out pt))
            {
                Console.WriteLine("DePacket Error!");
            }

            return pt;
        }


        public static string SelectServer()
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "config.txt");
            string[] lines = System.IO.File.ReadAllLines(path);
            Random rand = new Random();
            return lines[rand.Next(lines.Length)];
        }
    }

}