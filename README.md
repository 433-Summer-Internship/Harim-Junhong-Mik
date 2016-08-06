# Harim-Junhong-Mik (Team PocketChat)

<b>PocketChat Application</b> - A Console based chatting application that includes live chat rooms, message rankings, and multiple server support. Below is a summary of the client to server protocol design. The user and room information databases are maintaned by a Redis server run on Centos 7. The database is accessed by the servers and an admin/moderator client application.

<b>PocketChat Application Protocol Summary</b>  
-> The full details of the protocol can be found in ChatProtocol.cs  
Our strategy for our chat protocol was to use a simple generalized format, which would allow for facile and convenient expandability. Below is the strut that defines our protocol.  
```
struct ChatProtocol {  
	byte 		command;			//256 possible commands  
	ushort		fixedLengthField;		//unsigned short custom-value  
	byte[]		variableLengthField;		//Variable sized value  
}  
```
  
<b>command (byte):</b>  
A single byte at the beginning is reserved for determining the type of command that is being sent to either the server or the client. Using a byte allows for 256 possible commands, which is plenty for a small scale chatting application with plenty of room for expansion. The command 0 is reserved as a lack of command. A list of commands are below:     

Login           	10  
Logout          	11  
MessageToServer 	21  
MessageToClient 	22  
CreateRoom      	30    
JoinRoom        	31  
LeaveRoom       	32  
RoomListRequest        	40  
SendRoomList    	41  
UserListRequest        	50   
UserListSend        	51 
Heatbeat        	60   
ConnectionPass        	60 
reponse 		+100  (example: login_response = 110)

<b>fixedLengthField:</b>  
A ushort is used as the next segment of data. The purpose of this value is to hold small values of small-sized commands such a join room request. For larger-sized commands, this value specifies vital information about the variableLengthField. Some values are listed below:  

Login – ID#PW Length  
Logout – 0  
LoginResult - 1
MessageToServer – Message Length  
MessageToClient – Message Length  
GetRoomList – 0  
JoinRoom – Room Number
CreateRoom – Room Name Length    
LeaveRoom – 0  
SendRoomList – Room List Part Size 
HeartBeat – 0  
GetUserInfo – 0  
SendUserInfo – Number of Users  

<b>variableLengthField:</b>  
A variable length byte field with a maximum size of 1024 bytes. This portion of the protocol allows for messages to be sent of various sizes or secondary small data for smaller-sized commands such as login/logout commands. Some commands use just 1 byte to keep overhead for the common protocol at a minimum. Some details are outlined below:

Login – Username+Password (fixed length)  Login format = ID#PW  
Logout – Username (fixed length)   
LoginResult - 1 (success)   
LoginResult - -1 (fail)   
MessageToServer – Message (Length = fixedLengthField, up to 1010 bytes)  
MessageToClient – Message (Length = fixedLengthField, up to 1024 bytes - includes username)    
GetRoom - Room Name (Length = fixedLengthField)  
GetRoomList – 0 (fixed length – 1 byte)  
JoinRoom – 0 (fixed length – 1 byte)  
CreateRoom – 0 (fixed length – 1 byte)  
CreateRoomResult - Room Number  
LeaveRoom – 0 (fixed length – 1 byte)  
SendRoomList – Room Data (Length = fixedLengthField - includes a part number and total part number in the 0th and 1st byte)  
HeartBeat – 0 (fixed length – 1 byte)  
GetUserInfo – 0 (fixed length – 1 byte)  
SendUserInfo – Number of Real Users (fixed length)  

<b>Code</b>:  
Source code, dll, and exe files can be found at various locations:  
Client: https://github.com/eblikes2play/Chat_Client    
FE Server: https://github.com/433-Summer-Internship/Harim-Junhong-Mik/tree/master/fes   
Redis Client: https://github.com/Mukikaizoku/MikRedisDB-for-Pocketchat-App   
Admin (Monitor) Client: https://github.com/Mukikaizoku/PocketChat-Admin-Client


