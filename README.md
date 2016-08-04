# Harim-Junhong-Mik (Team PocketChat)

<b>PocketChat Application</b> - A Console based chatting application that includes live chat rooms, message rankings, and multiple server support. Below is a summary of the client to server protocol design. The user and room information databases are maintaned by a Redis server run on Centos 7. The database is accessed by the servers and an admin/moderator client application.

PocketChat Application Protocol Summary
-> The full details of the protocol can be found in ChatProtocol.cs
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
MessageToServer 21  
MessageToClient 22  
JoinRoom        31  
CreateRoom      32  
LeaveRoom       33  
RoomList        40  
sendRoomList    41  
UserList        50  
heatbeat        60   
reponse +100  

Login format = ID#PW  

Maximum Message Size = 1010 bytes
  
ValueA:  
-Ushort for assigning the first relevant parameter for each command (fixed size)  
Login – ID#PW Length  
Logout – 0  
LoginResult - 1
MessageToServer – Message Length  
MessageToClient – Message Length  
GetRoomList – 0  
JoinRoom – Room Number  
CreateRoom – Room Name Length    
LeaveRoom – 0  
SendRoomList – Number of Rooms  
HeartBeat – 0  
GetUserInfo – 0  
SendUserInfo – Number of Users  
  
ValueB:  
-Variable (up to maximum value) OR fixed length parameter  
Login – Username+Password (fixed length)  
Logout – Username (fixed length)  
LoginResult - 1 (success)  
LoginResult - -1 (fail)   
MessageToServer – Message (Length = ValueA)  
MessageToClient – Message (Length = ValueA)  
GetRoom - Room Name (Length = ValueA)  
GetRoomList – 0 (fixed length – 1 byte)  
JoinRoom – 0 (fixed length – 1 byte)  
CreateRoom – 0 (fixed length – 1 byte)  
CreateRoomResult - Room Number  
LeaveRoom – 0 (fixed length – 1 byte)  
SendRoomList – Room Data (Length = ValueA * factor)  
HeartBeat – 0 (fixed length – 1 byte)  
GetUserInfo – 0 (fixed length – 1 byte)  
SendUserInfo – Number of Real Users (fixed length)  

Code:  
Client:   
FE Server:   
Redis Client: https://github.com/Mukikaizoku/MikRedisDB-for-Pocketchat-App   
Admin (Monitor) Client: https://github.com/Mukikaizoku/PocketChat-Admin-Client


