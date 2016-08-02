using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace MikRedisDB
{
    //INFORMATION:
    //MikRedisDB Creates and Controls a Redis DB with the following structures:
    //Chatting Userpool with message count ranks (ZSet)
    //User information (Hash)
    //Lists of logged in and dummy users (Set)
    //Roompool with number of members information (ZSet)
    //Room information (Hash)
    //Lists of users in each room (Set)

    public class RedisDBController
    {
        /*****
            Methods and Properties for
            Thread-Safe Set-up, Connection, and Close
        *****/

        private static ConfigurationOptions configOptions = new ConfigurationOptions();
        private IDatabase db;

        //**Thread-safe singleton pattern for ConnectionMultiplexer**
        //Create the redis connection "lazily" meaning the connection won't be made until it is needed
        //The Lazy<T> class allows for thread-safe initialization
        private static Lazy<ConnectionMultiplexer> connMulti = new Lazy<ConnectionMultiplexer>(() =>
           ConnectionMultiplexer.Connect(configOptions)
        );

        //Make the ConnectionMultiplexer a read-only property
        private static ConnectionMultiplexer safeConn
        {
            get
            {
                return connMulti.Value;
            }
        }

        //Method for Configuration Setup
        public void SetConfigurationOptions (string ip, int portNumber, string password)
        {
            configOptions.EndPoints.Add(ip, portNumber);
            configOptions.Password = password;
            configOptions.ClientName = "RedisConnection";
            configOptions.KeepAlive = 200;
            configOptions.ConnectTimeout = 100000;
            configOptions.SyncTimeout = 100000;
            configOptions.AbortOnConnectFail = false;
        }

        //Method for Attempting a Connection
        //Throws exceptions on fail? (Try, Catch => Throw new)
        public void SetupConnection()
        {
            db = safeConn.GetDatabase();
        }

        //Shutdown connection
        public void CloseConnection()
        {
            safeConn.Close();
        }

        /*****
            Methods for
            Account Creation/Deletion/Existence/CheckValues/Retrieval/Update
        *****/

        //Create a new user account
        public bool CreateUser (string name, string password)
        {
            //Ensure the user does not exist
            if (!DoesUsernameExist (name))
            {
                //Create a new entry for the user
                db.SortedSetAdd("UserPool", "User:" + name, 0);         //Add new user to the userpool
                db.HashSet("User:" + name, "Password", password);       //Add user's password
                db.HashSet("User:" + name, "ConnectionID", 0);          //Set Connection ID default
                db.HashSet("User:" + name, "LoginFlag", false);         //Set Login Flag default
                db.HashSet("User:" + name, "DummyFlag", false);         //Set Dummy Flag default
                db.HashSet("User:" + name, "BlockFlag", false);         //Set Block Flag default
                db.HashSet("User:" + name, "SuspendTimer", 0);          //Set Suspend Timer default
                db.HashSet("User:" + name, "RoomNumber", 0);            //Set RoomNumber to Lobby (0)
                return true;                                            //Creation Successful
            }
            return false;                                               //Username already exists
        }

        //Function for deleting an account
        public bool DeleteUser(string name, string password)
        {
            if (DoesUsernameExist(name))
            {
                if (IsPasswordCorrect(name, password))
                {
                    //Remove the user from their room
                    RoomRemoveUser((uint)db.HashGet("User:" + name, "RoomNumber"), name);
                    //Delete the User's key
                    db.KeyDelete("User:" + name);
                    db.SortedSetRemove("UserPool", "User:" + name);
                    //Be sure to remove the User name from any other pools
                    db.SetRemove("LoginPool", "User:" + name);
                    db.SetRemove("DummyPool", "User:" + name);
                    return true;   //Return true for success
                }
            }
            return false;          //Username or Password is incorrect
        }

        //***Check Value Functions***

        //Check if a particular user account exists
        //Return true if it exists, false if not
        public bool DoesUsernameExist (string name)
        {
            if (db.KeyExists("User:" + name))
            {
                return true;
            }
            return false;
        }

        //Check if password is correct
        //Return true if correct, false otherwise
        //Private for protection
        private bool IsPasswordCorrect (string name, string password)
        {
            if (db.HashGet("User:" + name, "Password") == password)
            {
                return true;
            }
            return false;
        }

        //Check if a user is logged in
        public int IsUserLoggedIn (string name)
        {
            if (DoesUsernameExist(name))
            {
                if (db.HashGet("User:" + name, "LoginFlag") == true)
                {
                    return 1;       //User is logged in
                }
                return 0;           //User is not logged in
            }
            return -1;              //Username does not exist
        }

        //Check if a user is a dummy
        public int IsUserDummy (string name)
        {
            if (DoesUsernameExist(name))
            {
                if (db.HashGet("User:" + name, "DummyFlag") == true)
                {
                    return 1;       //User is a dummy
                }
                return 0;           //User is not a dummy
            }
            return -1;              //Username does not exist
        }

        //Check if a user is blocked
        public int IsUserBlocked (string name)
        {
            if (DoesUsernameExist(name))
            {
                if (db.HashGet("User:" + name, "BlockFlag") == true)
                {
                    if (HasSuspensionEnded(name))
                    {
                        return 2;   //Return 2 to say the user is no longer blocked   
                    }
                    return 1;       //User is blocked
                }
                return 0;           //User is not blocked
            }
            return -1;              //Username does not exist
        }

        //***Data Retrieval Functions***

        //Function to get connection ID
        public int GetUserConnectionID (string name)
        {
            if (DoesUsernameExist(name))
            {
                return (int)db.HashGet("User:" + name, "ConnectionID");     //Return Connection ID
            }
            return -1;                                                      //Username does not exist
        }

        //Function to get a blocked user's suspend time
        public uint GetUserSuspendTime (string name)
        {
            if (DoesUsernameExist(name))
            {
                int check = IsUserBlocked(name);
                switch (check)
                {
                    case 1:
                        return (uint)db.HashGet("User:" + name, "SuspendTimer");        //Return penalty expiration timestamp
                    case 2:
                        return 2;                                                       //Return 2 to say the user is no longer blocked
                    default:
                        return 1;                                                       //Return 1 for user is not blocked
                }
            }
            return 0;                                                                                           //Username does not exist
        }

        //Private function to check if a suspend time has expired
        //If the time has expired, this function unblocks the user
        private bool HasSuspensionEnded (string name)
        {
            uint timePenalty = (uint)db.HashGet("User:" + name, "SuspendTimer");                        //Get the timestamp when the penality expires
            uint timeNow = (uint)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;     //Get the current unix time
            if (timePenalty <= timeNow)
            {
                //Unblock-user
                db.HashSet("User:" + name, "BlockFlag", false);                                         //Unset Block Flag
                db.HashSet("User:" + name, "SuspendTimer", 0);                                          //Set the duration of the block back to 0  
                return true;                                                                            //Return true to say the user is no longer blocked   
            }
            return false;                                                                               //Return false to say the user is still blocked
        }

        //Function to get a user's message count
        public int GetUserMessageCount (string name)
        {
            if (DoesUsernameExist(name))
            {
                return (int)db.SortedSetScore("UserPool", "User:" + name);  //Return user's message count
            }
            return -1;                                                      //Username does not exist
        }

        //Function to get user's room location
        public int GetUserLocation (string name)
        {
            if (DoesUsernameExist(name))
            {
                return ((int)db.HashGet("User:" + name, "RoomNumber"));       //Return user's room number
            }
            return -1;                                                        //Return -1 if the user does not exist
        }

        //Function to get User's rank
        public int GetUserRank (string name)
        {
            if (DoesUsernameExist(name))
            {
                return ((int)db.SortedSetRank("UserPool", "User:" + name, Order.Descending) + 1);       //Return user's rank (in descending order) [+1 because the index starts at 0]
            }
            return -1;                                                                                  //Username does not exist
        }

        //Get the message count of a certain rank
        public int GetMessageCountAtRank (int rank)
        {
            if (rank <= db.SortedSetLength("UserPool"))                                                 //Check if the rank value is within range
            {
                //Return the score at the value which is the at the value at the position given by the specified rank position
                return (int)db.SortedSetScore("UserPool", db.SortedSetRangeByRank("UserPool", 0, rank - 1, Order.Descending)[rank - 1]);
            }
            return -1;                                                                                  //Rank does not exist
        }

        //***Data Set Functions***

        //Function to change username
        public int ChangeUsername (string currentName, string newName, string password)
        {
            if (DoesUsernameExist(currentName))
            {
                if (IsPasswordCorrect(currentName, password))
                {
                    //Check if user is not blocked
                    int check = IsUserBlocked(currentName);
                    switch (check)
                    {
                        case 0:
                        case 2:
                            //Check if the new name is different than the old one
                            if (currentName == newName)
                            {
                                return -2;                                                              //If the two names are the same, return -2
                            }
                            if (DoesUsernameExist(newName))
                            {
                                return -3;                                                              //If the new name is already used, return -3
                            }
                            uint tempNumber = (uint)db.HashGet("User:" + currentName, "RoomNumber");
                            RoomRemoveUser(tempNumber, currentName);                                    //Remove the user's name from the current room
                            RoomAddUser(tempNumber, newName);                                           //Add the user's name from the current room

                            db.KeyRename("User:" + currentName, "User:" + newName);                     //At this point all the credentials have been cleared and we can now update the user's name
                            tempNumber = (uint)db.SortedSetScore("UserPool", "User:" + currentName);    //Temporarily hold the user's score
                            db.SortedSetAdd("UserPool", "User:" + newName, tempNumber);                 //Create the new user entry
                            db.SortedSetRemove("UserPool", "User:" + currentName);                      //Remove the old entry

                            //If logged in or dummy, need to update sets
                            if (db.SetContains("LoginPool", "User:" + currentName))
                            {
                                db.SetRemove("LoginPool", "User:" + currentName);
                                db.SetAdd("LoginPool", "User:" + newName);
                            }
                            if (db.SetContains("DummyPool", "User:" + currentName))
                            {
                                db.SetRemove("DummyPool", "User:" + currentName);
                                db.SetAdd("DummyPool", "User:" + newName);
                            }
                            return 1;                                                                   //Return 1 if successful
                        default:
                            return -1;                                                                  //User is blocked
                    }                                                                                   
                }
            }
            return 0;                                                                           //Username or Password is incorrect
        }

        //Function to change password
        public int ChangePassword (string name, string currentPassword, string newPassword)
        {
            if (DoesUsernameExist(name))
            {
                if (IsPasswordCorrect(name, currentPassword))
                {
                    //Check if user is not blocked
                    int check = IsUserBlocked(name);
                    switch (check)
                    {
                        case 0:
                        case 2:
                            //Check if the new name is different than the old one
                            if (currentPassword == newPassword)
                            {
                                return -2;                                                      //If the two passwords are the same, return -2
                            }
                            db.HashSet("User:" + name, "Password", newPassword);                //At this point all the credentials have been cleared and we can now update the user's password
                            return 1;                                                           //Return 1 if successful
                        default:
                            return -1;                                                                  //User is blocked
                    }
                }
            }
            return 0;                                                                       //Username or Password is incorrect (0)
        }

        //Function to change Connection ID
        //Returns old connection ID
        public int ChangeConnectionID (string name, int newConnectionID)
        {
            if (DoesUsernameExist(name))
            {
                int oldConnectionID = GetUserConnectionID(name);                //Set the reporting ID to the old connection ID
                db.HashSet("User:" + name, "ConnectionID", newConnectionID);    //Set new Connection ID
                return oldConnectionID;                                         //Return the old connection ID
            }
            return -1;                                                          //Username does not exist
        }

        //Function to set block
        public uint BlockUser (string name, uint minutes)
        {
            if (DoesUsernameExist(name))
            {
                uint unixTime = 0;
                if (IsUserBlocked (name) == 1)
                {
                    unixTime = (uint)db.HashGet("User:" + name, "SuspendTimer");                            //If blocked, get the current suspend time
                } else
                {
                    unixTime = (uint)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;     //Otherwise, get the current unix time
                }
                unixTime = unixTime + (minutes * 60);                                                       //Add the new penalty to the time
                db.HashSet("User:" + name, "SuspendTimer", unixTime);                                       //Set the penalty timeout time
                db.HashSet("User:" + name, "BlockFlag", true);                                              //Set Block Flag
                return unixTime;                                                                            //Return the time for success
            }
            return 0;                                                                                       //Username does not exist
        }

        //Function to ublock a user
        public int UnBlockUser(string name)
        {
            if (DoesUsernameExist(name))
            {
                if (IsUserBlocked(name) == 0)                                   //If the user isn't blocked
                {
                    return -1;                                                  //Return -1
                }
                db.HashSet("User:" + name, "BlockFlag", false);                 //Unset Block Flag
                db.HashSet("User:" + name, "SuspendTimer", 0);                  //Set the duration of the block back to 0
                return 1;                                                       //Return 1 for success
            }
            return 0;                                                           //Username does not exist, return 0
        }

        //Function to increment users message count
        //This function allows the message count to be decremented with a negative input value
        //However, the message count cannot go below 0
        //Return of -1 indicates the username does not exist
        //Otherwise the updated message count number is returned
        public int AddToUserMessageCount (string name, int count)
        {
            if (DoesUsernameExist(name))
            {
                int currentMessageCount = (int)db.SortedSetScore("UserPool", "User:" + name);   //Get the current message count
                currentMessageCount += count;                                                   //Increment by the desired value
                if (currentMessageCount < 0)                                                    //If the new value is less than zero, set to zero
                {
                    currentMessageCount = 0;
                }
                db.SortedSetAdd("UserPool", "User:" + name, currentMessageCount);               //Update the user's message count
                return currentMessageCount;                                                     //Return user's new message count
            }
            return -1;                                                                          //Username does not exist
        }

        /*****
            Methods for
            Account Login/Logout
        *****/

        //Function for log-in
        public int Login (string name, string password, int connectionID, bool isDummy)
        {
            if (DoesUsernameExist(name))
            {
                if (IsPasswordCorrect (name, password))
                {
                    //Check if user is blocked
                    int check = IsUserBlocked(name);
                    switch (check)
                    {
                        case 0:
                        case 2:
                            int oldConnectionID = connectionID;                         //By default, set the reporting ID variable to the new connectionID
                            if (IsUserLoggedIn(name) == 1)
                            {
                                oldConnectionID = GetUserConnectionID(name);            //If the user is already logged in, then change the reporting ID to the old connection ID
                            }
                            db.HashSet("User:" + name, "LoginFlag", true);              //Set the Login-Flag to true
                            db.HashSet("User:" + name, "ConnectionID", connectionID);   //Set Dummy Flag and Connection ID
                            db.HashSet("User:" + name, "DummyFlag", isDummy);           //Set Dummy Flag and Connection ID
                            db.SetAdd("LoginPool", "User:" + name);                     //Keep track of logged in users for stats
                            db.SetAdd("LobbyPool", "User:" + name);                     //Add user from the lobby pool
                            if (isDummy == true)
                            {
                                db.SetAdd("DummyPool", "User:" + name);                 //Keep track of dummy users for stats
                            }
                            return connectionID;                                        //Return connectionID if successful 
                        default:
                            return -1;                                                  //User is blocked
                    }
                }
            }
            return 0;                                                               //Username or Password is incorrect
        }

        //Function for logging out
        public int Logout (string name)
        {
            if (DoesUsernameExist(name))
            {
                if (IsUserLoggedIn(name) == 0)
                {
                    return 0;                                                       //If the user isn't logged in, report 0 and do nothing
                }
                //Reset relevant data
                db.SetRemove("LoginPool", "User:" + name);                          //Keep track of logged in users for stats
                if (IsUserDummy(name) == 1)
                {
                    db.SetRemove("DummyPool", "User:" + name);                      //Keep track of dummy users for stats
                }
                db.HashSet("User:" + name, "LoginFlag", false);                     //Set the Login-Flag to true
                db.HashSet("User:" + name, "ConnectionID", 0);                      //Set Dummy Flag and Connection ID
                db.HashSet("User:" + name, "DummyFlag", false);                     //Set Dummy Flag and Connection ID
                int location = GetUserLocation(name);
                if (location == 0)
                {
                    db.SetRemove("LobbyPool", "User:" + name);                      //Remove user from the lobby pool
                } else
                {
                    db.SetRemove("Room:" + location + ":Contents", "User:" + name); //Remove user from the room they are in
                }
                return 1;                                                           //Logout successful
            }
            return -1;                                                              //Username incorrect
        }
        
        /*****
            Methods for
            Database Statistics
        *****/

        //Function for getting size of UserPool
        public long GetUserPoolSize ()
        {
            return db.SortedSetLength ("UserPool");
        }

        //Function to get all users
        public string[] GetUserList ()
        {
            return db.SortedSetRangeByRank("UserPool", 0, -1, Order.Descending).ToStringArray ();
        }

        //Function to get rank list
        //Generate a top-n list from the UserPool
        public Dictionary<string, double> GetTopList(int topNumber)
        {
            return db.SortedSetRangeByRankWithScores("UserPool", 0, topNumber, Order.Descending).ToStringDictionary();
        }

        //Function to get number of logged in users
        public long GetLoginPoolSize()
        {
            return db.SetLength("LoginPool");
        }

        //Function to get the number of dummy users (logged) in
        public long GetDummyPoolSize()
        {
            return db.SetLength("DummyPool");
        }

        //Function to get all logged in user info
        public string[] GetLoginList()
        {
            return db.SetMembers("LoginPool").ToStringArray();
        }

        //Function to get all dummy user info
        public string[] GetDummyList()
        {
            return db.SetMembers("DummyPool").ToStringArray();
        }

        //Function to get the user rank list based on an intersection with another list
        //NOTE: This function yields message counts +1 greater than their actual value!!
        public Dictionary<string, double> GetSubTopList(int topNumber, string poolName)
        {
            db.SortedSetCombineAndStore(SetOperation.Intersect, poolName + "Ranked", "UserPool", poolName, Aggregate.Sum);              //Create a temporary ranked login pool
            db.KeyExpire(poolName + "Ranked", new TimeSpan (0, 1, 0));                                                                  //Let the pool expire after 1 minute
            return db.SortedSetRangeByRankWithScores(poolName + "Ranked", 0, topNumber, Order.Descending).ToStringDictionary();         //Return the dictionary
        }

        /*****
            Methods for
            Rooms
        *****/

        //Create Room
        public int RoomCreate (uint roomNumber, string roomTitle, string owner)
        {
            if (RoomExist(roomNumber) == false)
            {
                //Check if owner exists
                if (!DoesUsernameExist (owner))
                {
                    return 0;                                               //Return 0 if the owner does not exist
                }
                db.SortedSetAdd("RoomPool", "Room:" + roomNumber, 0);       //Add entry to the room pool with 0 people
                db.HashSet("Room:" + roomNumber, "RoomTitle", roomTitle);   //Create hash with room's information
                db.HashSet("Room:" + roomNumber, "RoomOwner", owner);       //Create hash with room's information
                RoomAddUser(roomNumber, owner);
                return 1;                                                   //Return 1 for success
            }
            return -1;                                                      //Return -1 if room already exists
        }

        //Destroy Room
        //This is set to private (for now) to protect users from deleting a room with people inside
        //To delete a room, users must be removed until there are 0 users left in the room
        private bool RoomDelete (uint roomNumber)
        {
            if (!RoomExist(roomNumber))
            {
                return false;                                               //Return false if room does not exist
            }
            db.KeyDelete("Room:" + roomNumber + ":Contents");               //Remove room::contents key
            db.KeyDelete("Room:" + roomNumber);                             //Remove room key
            db.SortedSetRemove("RoomPool", "Room:" + roomNumber);           //Remove entry from room pool
            return true;
        }

        //***Retrieve Room Data Functions***

        //Room existance
        public bool RoomExist (uint roomNumber)
        {
            if (db.KeyExists("Room:" + roomNumber))         //Check if the room's key exists
            {
                return true;
            }
            return false;
        }

        //Check if a user is in a room
        public int RoomContainsUser (uint roomNumber, string name)
        {
            if (!RoomExist(roomNumber))
            {
                return -1;                                                      //Return -1 if room does not exist
            }
            if (!DoesUsernameExist(name))
            {
                return -2;                                                      //Return -2 if user does not exist
            }
            if (db.SetContains("Room:" + roomNumber + ":Contents", "User:" + name))
            {
                return 1;                                                       //Return 1 if user is in the room
            }
            return 0;                                                           //Return 0 if user is not in the room
        }

        //Get Room Title
        public bool RoomGetTitle (uint roomNumber, out string roomTitle)
        {
            roomTitle = null;                                               //Set the default out-string value
            if (!RoomExist(roomNumber))
            {
                return false;                                               //Return false if room does not exist
            }
            roomTitle = db.HashGet("Room:" + roomNumber, "RoomTitle");      //Get the title from the room's hash
            return true;
        }

        //Get room user count
        public int RoomGetUserCount (uint roomNumber)
        {
            if (!RoomExist(roomNumber))
            {
                return -1;                                                          //Return -1 if room does not exist
            }
            return (int)db.SortedSetScore("RoomPool", "Room:" + roomNumber);        //Get the user count from the room's sorted set entry
        }

        //Get room owner
        public bool RoomGetOwner(uint roomNumber, out string roomOwner)
        {
            roomOwner = null;                                               //Set the default out-string value
            if (!RoomExist(roomNumber))
            {
                return false;                                               //Return false if room does not exist
            }
            roomOwner = db.HashGet("Room:" + roomNumber, "RoomOwner");      //Get the owner's name from the room's hash
            return true;
        }

        //Get room's size rank
        public int RoomGetSizeRank (uint roomNumber)
        {
            if (!RoomExist(roomNumber))
            {
                return -1;                                                                              //Return -1 if room does not exist
            }
            return ((int)db.SortedSetRank("RoomPool", "Room:" + roomNumber, Order.Descending) + 1);     //Return room's rank (in descending order) [+1 because the index starts at 0]
        }

        //Change room name
        public int RoomChangeTitle(uint roomNumber, string newRoomTitle, out string oldRoomTitle)
        {
            oldRoomTitle = null;                                                //Set the default out-string value
            if (!RoomExist(roomNumber))
            {
                return -1;                                                      //Return -1 if room does not exist
            }
            oldRoomTitle = db.HashGet("Room:" + roomNumber, "RoomTitle");       //Get the title from the room's hash
            if (oldRoomTitle == newRoomTitle)
            {
                return 0;                                                       //Return 0 if the new room title is the same as before
            }
            db.HashSet("Room:" + roomNumber, "RoomTitle", newRoomTitle);        //Set the new title to the room's hash
            return 1;
        }

        //Set room owner
        public int RoomSetOwner(uint roomNumber, string newRoomOwner, out string oldRoomOwner)
        {
            oldRoomOwner = null;                                                //Set the default out-string value
            if (!RoomExist(roomNumber))
            {
                return -1;                                                      //Return -1 if room does not exist
            }
            oldRoomOwner = db.HashGet("Room:" + roomNumber, "RoomOwner");       //Get the owner name from the room's hash
            if (oldRoomOwner == newRoomOwner)
            {
                return 0;                                                       //Return 0 if the new owner name is the same as before
            }
            db.HashSet("Room:" + roomNumber, "RoomOwner", newRoomOwner);        //Set the new owner name to the room's hash
            return 1;
        }

        //Enter room
        public int RoomAddUser (uint roomNumber, string name)
        {
            if (!RoomExist(roomNumber))
            {
                return -1;                                                  //Return -1 if room does not exist
            }
            if (!DoesUsernameExist(name))
            {
                return 0;                                                   //Return 0 if user does not exist
            }
            if (RoomContainsUser(roomNumber, name) == 1)
            {
                return -2;                                                  //Return -2 if the room already contains the user
            }
            db.SortedSetIncrement("RoomPool", "Room:" + roomNumber, 1);     //Add one to the room's user count
            db.SetAdd("Room:" + roomNumber + ":Contents", "User:" + name);  //Add the user to the room's user pool
            db.SetRemove("LobbyPool", "User:" + name);                      //Remove user from the lobby pool
            db.HashSet("User:" + name, "RoomNumber", roomNumber);           //Record that the user is in the room
            return 1;                                                       //Return 1 on success
        }

        //Leave room
        public int RoomRemoveUser(uint roomNumber, string name)
        {
            if (!RoomExist(roomNumber))
            {
                return -1;                                                                  //Return -1 if room does not exist
            }
            if (!DoesUsernameExist(name))
            {
                return 0;                                                                   //Return 0 if user does not exist
            }
            if (RoomContainsUser (roomNumber, name) != 1)
            {
                return -3;                                                                  //Return -3 if the user is not in the room          
            }
            db.SortedSetDecrement("RoomPool", "Room:" + roomNumber, 1);                     //Remove one from the room's user count
            db.SetRemove("Room:" + roomNumber + ":Contents", "User:" + name);               //Remove the user from the room's user pool
            db.SetAdd("LobbyPool", "User:" + name);                                         //Add user from the lobby pool
            db.HashSet("User:" + name, "RoomNumber", 0);                                    //Record that the user is back in the lobby
            if (db.SortedSetScore ("RoomPool", "Room:" + roomNumber) < 1)
            {
                if(RoomDelete(roomNumber))                                                  //If the room user count dropped to 0, delete the room
                {
                    return 2;                                                               //Return 2 to indicate that the room was destroyed
                }
                return -2;                                                                  //Return -2 if the room should have been deleted but wasn't possible for an unknown reason                                                 
            }

            //If there are still people in the room, we need to transfer the owner to a different user (if the person removed was the owner!)
            //First, check if the removed user was the owner
            if (string.Compare(db.HashGet("Room:" + roomNumber, "RoomOwner"), name) == 0)
            {
                string newOwner = db.SetRandomMember("Room:" + roomNumber + ":Contents");   //Get a random user from the room's pool
                if (newOwner == null)
                {
                    return 3;                                                               //Return 3 if for some unknown reason no member name was obtained
                }
                db.HashSet("Room:" + roomNumber, "RoomOwner", newOwner);                    //Set the new owner name to the room's hash
                return 4;                                                                   //Return 4 to indicate the owner was changed
            }
            return 1;                                                                       //Return 1 on success and the owner didn't need changing
        }

        //***Room Statistics***

        //Get all users in room
        public string[] RoomUserList(uint roomNumber)
        {
            if (!RoomExist(roomNumber))
            {
                return null;                                                  //Return null if room does not exist
            }
            return db.SetMembers("Room:" + roomNumber + ":Contents").ToStringArray();
        }

        //Get members in the lobby
        public string[] GetLobbyUserList ()
        {
            return db.SetMembers("LobbyPool").ToStringArray();
        }

        //Get room list
        public string[] RoomList()
        {
            return db.SortedSetRangeByRank("RoomPool", 0, -1, Order.Descending).ToStringArray();
        }

        //Get room list rankings (with score)
        public Dictionary<string, double> RoomSizeRankList(int topNumber)
        {
            return db.SortedSetRangeByRankWithScores("RoomPool", 0, topNumber, Order.Descending).ToStringDictionary();
        }
    }
}
