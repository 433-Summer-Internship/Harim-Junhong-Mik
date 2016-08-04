using System;
using System.Runtime.InteropServices;

namespace ChatProtocolController
{
    /// <summary>
    /// The ChatProtocol struct defines the general structure of the various message types we send in our chat program 
    /// between the client and ther server.
    ///     command - a byte that contains information about the message type
    ///     fixedLengthField - a ushort value used by small-sized commands or length information for large-sized commands
    ///     variableLengthField - a variable length byte array that contains variant sized information such as messages
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ChatProtocol
    {
        [MarshalAs(UnmanagedType.I1)]
        public byte command;                            //256 possible commands  

        [MarshalAs(UnmanagedType.U2)]
        public ushort fixedLengthField;                 //unsigned short custom-value  

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1024)]
        public byte[] variableLengthField;              //Variable sized value  
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct RoomInfoDatum
    {
        
        public byte roomNumber;                         //Room Number (max 255 number as 0 is the Lobby)  

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 20)]
        public string roomTitle;                        //Room Title with a max size of 20

        public byte userCount;                          //User count for the room (max 255 per room) 

        public RoomInfoDatum(byte num, string title, byte count)
        {
            roomNumber = num;
            roomTitle = title;
            userCount = count;
        }
    }

    /// <summary>
    /// This class controls the creation and depacking of sendable byte arrays in accordance to our chat protocol.
    /// </summary>
    public class PacketMaker
    {
        /// <summary>
        /// This class contains readable command names for each command code in this chat protocol.
        /// </summary>
        public class CommandCode
        {
            //Login/logout commands
            public const byte LOGIN =                       10;
            public const byte LOGOUT =                      11;
            public const byte LOGIN_RESULT =                110;
            public const byte LOGOUT_RESULT =               111;

            //Text sending commands
            public const byte MESSAGE_TO_SERVER =           21;
            public const byte MESSAGE_TO_CLIENT =           22;
            public const byte MESSAGE_TO_SERVER_RESULT =    121;
            public const byte MESSAGE_TO_CLIENT_RESULT =    122;

            //Basic room commands
            public const byte CREATE_ROOM =                 30;
            public const byte JOIN_ROOM =                   31;
            public const byte LEAVE_ROOM =                  32;
            public const byte CREATE_ROOM_RESULT =          130;
            public const byte JOIN_ROOM_RESULT =            131;
            public const byte LEAVE_ROOM_RESULT =           132;

            //User statistics requests
            public const byte ROOM_LIST_REQUEST =           40;
            public const byte ROOM_LIST_SEND =              41;
            public const byte ROOM_LIST_REQUEST_RESULT =    140;
            public const byte ROOM_LIST_SEND_RESULT =       141;

            //Admin statistics requests
            public const byte USER_LIST_REQUEST =           50;
            public const byte USER_LIST_SEND =              51;
            public const byte USER_LIST_REQUEST_RESULT =    150;
            public const byte USER_LIST_SEND_RESULT =       151;

            //Connection health commands
            public const byte HEARTBEAT =                   60;
            public const byte HEARTBEAT_RESULT =            160;

            //Connection Passing commands
            public const byte CONNECTIOINPASSING_REQUEST =  70;
            public const byte CONNECTIOINPASSING_RESULT =   170;
        }

        /// <summary>
        /// The CreatePacket method takes in the ChatProtocol's three fields and returns a byte array that is sendable by TCP.
        /// </summary>
        /// <param name="commandCode"></param>
        /// <param name="fixedLengthFieldValue"></param>
        /// <param name="variableLengthFieldValue"></param>
        /// <returns></returns>
        public static byte[] CreatePacket(byte commandCode, ushort fixedLengthFieldValue, byte[] variableLengthFieldValue)
        {
            //Packet value assignment
            ChatProtocol packet = new ChatProtocol();
            packet.command = commandCode;
            packet.fixedLengthField = fixedLengthFieldValue;
            packet.variableLengthField = variableLengthFieldValue;

            //Struct to byte-array conversion
            int datasize = Marshal.SizeOf(packet);                     
            IntPtr buff = Marshal.AllocHGlobal(datasize);               
            Marshal.StructureToPtr(packet, buff, false);                
            byte[] sendableData = new byte[datasize];                   
            Marshal.Copy(buff, sendableData, 0, datasize);              
            Marshal.FreeHGlobal(buff);                                  

            //Return the sendable data
            return sendableData;
        }

        /// <summary>
        /// The DePacket method uses the chat protocol to take a received byte array and convert it back to our ChatProtocol struct.
        /// </summary>
        /// <param name="receivedData"></param>
        /// <returns></returns>
        public static  bool TryDePacket(byte[] receivedData, out ChatProtocol receivedPacket)                            
        {
            //Initialize the out ChatProtocol
            receivedPacket = new ChatProtocol
            { command = 0, fixedLengthField = 0, variableLengthField = null };

            //Byte-array to struct conversion
            IntPtr buff = Marshal.AllocHGlobal(receivedData.Length);                               
            Marshal.Copy(receivedData, 0, buff, receivedData.Length);                               
            object packet = Marshal.PtrToStructure(buff, typeof(ChatProtocol));                    
            Marshal.FreeHGlobal(buff);                  

            //Check for a problem in the data length
            if (Marshal.SizeOf(packet) != receivedData.Length)                              
            {
                return false;                                       //Return False on a Data Length Error                                          
            }
            receivedPacket = (ChatProtocol)packet;                  //If no errors, cast the data into our ChatProtocol struct

            return true;                                            //Return True on success                                                   
        }
    }
}
