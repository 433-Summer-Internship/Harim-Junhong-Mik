# Harim-Junhong-Mik
```
struct ChatProtocol {  
	byte 		command;	//256 possible commands  
	ushort		valueA;		//unsigned short custom-value  
	byte[]		valueB;		//Variable sized value  
}  
```
  
Command:  
-Number 0-255 which determines the command type (Example: commend = 1 for Login)      

login           10  
logout          11
loginResult     12  
MessageToServer 21  
MessageToClient 22  
JoinRoom        31  
CreateRoom      32  
LeaveRoom       33  
RoomList        40  
snedRommList    41  
UserList        50  
heatbeat        60  

Login format = ID#PW  
  
  
ValueA:  
-Ushort for assigning the first relevant parameter for each command (fixed size)  
Login – 0  
Logout – 0  
MessageToServer – Message Length  
MessageToClient – Message Length  
GetRoomList – 0  
JoinRoom – Room Number  
CreateRoom – Room Number  
LeaveRoom – 0  
SendRoomList – Number of Rooms  
HeartBeat – 0  
GetUserInfo – 0  
SendUserInfo – Number of Users  
  
ValueB:  
-Variable (up to maximum value) OR fixed length parameter  
Login – Username+Password (fixed length)  
Logout – Username (fixed length)  
MessageToServer – Message (Length = ValueA)  
MessageToClient – Message (Length = ValueA)  
GetRoomList – 0 (fixed length – 1 byte)  
JoinRoom – 0 (fixed length – 1 byte)  
CreateRoom – 0 (fixed length – 1 byte)  
LeaveRoom – 0 (fixed length – 1 byte)  
SendRoomList – Room Data (Length = ValueA * factor)  
HeartBeat – 0 (fixed length – 1 byte)  
GetUserInfo – 0 (fixed length – 1 byte)  
SendUserInfo – Number of Real Users (fixed length)  


