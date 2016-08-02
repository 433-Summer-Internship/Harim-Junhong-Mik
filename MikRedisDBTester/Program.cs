using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MikRedisDB;

namespace MikRedisDBTester
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("\n\t*****MikRedisDB Emulator*****");
            Console.WriteLine("WARNING: This emulator is not coded to handle incorrect user input (argument value type checking).");
            Console.Write("Connecting to redis server. . .");
            RedisDBController redis = new RedisDBController();
            try
            {
                redis.SetConfigurationOptions("10.100.58.10", 30433, "433redis!");
                redis.SetupConnection();
            } catch
            {
                Console.WriteLine(" . . .Failed!");
                Console.WriteLine("Please check your server and try again. . . Goodbye!");
                return;
            }
            Console.WriteLine(" . . .Connected!");

            bool isUserFinished = false;
            bool flag = false;
            int number = 0;
            int numResult = 0;
            uint time = 0;
            DateTime timeNow = new DateTime(1970, 1, 1);
            DateTime timeLater;
            long bigNumber = 0;
            ConsoleKeyInfo ckey;
            string response;
            string name;
            string password;
            string[] nameList;
            Dictionary<string, double> rankList;

            while (isUserFinished == false)
            {
                Console.WriteLine("\n\t*****MikRedisDB Emulator*****");
                Console.WriteLine("\tcmdlist \t- \tGet full list of user commands");
                Console.WriteLine("\trcmdlist \t- \tGet full list of room commands");
                Console.WriteLine("\tstats \t\t- \tGet statistics commands");
                Console.WriteLine("\texit \t\t- \tQuit");
                Console.Write("\nEnter a command: ");
                response = Console.ReadLine();

                switch (response)
                {
                    case "cmdlist":
                        Console.WriteLine("\n\t*****User Commands*****");
                        Console.WriteLine("\tnew \t\t- \tCreate New User");
                        Console.WriteLine("\tdel \t\t- \tDelete User");
                        Console.WriteLine("\tuexist \t\t- \tCheck User Existance");
                        Console.WriteLine("\tchklogin \t- \tCheck If User is Logged In");
                        Console.WriteLine("\tchkdummy \t- \tCheck If User is a Dummy");
                        Console.WriteLine("\tchkblock \t- \tCheck If User is Blocked");
                        Console.WriteLine("\tchgname \t- \tChange Username");
                        Console.WriteLine("\tchgpass \t- \tChange User Password");
                        Console.WriteLine("\tchgconn \t- \tChange User Connection ID");
                        Console.WriteLine("\tblkuser \t- \tBlock User");
                        Console.WriteLine("\tublkuser \t- \tUnblock User");
                        Console.WriteLine("\tgetrank \t- \tGet User's Rank");
                        Console.WriteLine("\tgetuloc \t- \tGet User's Location");
                        Console.WriteLine("\tgetconnid \t- \tGet User Connection ID");
                        Console.WriteLine("\tgetsusp \t- \tGet User Suspend Time");
                        Console.WriteLine("\tgetmsgcnt \t- \tGet User Message Count");
                        Console.WriteLine("\tgetrmsgcnt \t- \tGet User Message Count at rank");
                        Console.WriteLine("\tincmsgcnt \t- \tModify a User's Message Count");
                        Console.WriteLine("\tlogin \t\t- \tAttempt User Login");
                        Console.WriteLine("\tlogout \t\t- \tAttempt User Logout");
                        break;
                    case "rcmdlist":
                        Console.WriteLine("\n\t*****Room Commands*****");
                        Console.WriteLine("\trnew \t\t- \tCreate New Room");
                        Console.WriteLine("\trdel \t\t- \tDelete Room");
                        Console.WriteLine("\trexist \t\t- \tCheck Room Existance");
                        Console.WriteLine("\trconuser \t- \tCheck If a User is in a room");
                        Console.WriteLine("\trtitle \t\t- \tGet room title");
                        Console.WriteLine("\trucount \t- \tGet room user count");
                        Console.WriteLine("\trowner \t\t- \tGet room owner");
                        Console.WriteLine("\trsizerank \t- \tGet room size rank");
                        Console.WriteLine("\trchgtitle \t- \tChange Room Title");
                        Console.WriteLine("\trsetown \t- \tChange Room Owner");
                        Console.WriteLine("\tradduse \t- \tAdd user to room");
                        Console.WriteLine("\trremuse \t- \tRemove user from room");
                        break;
                    case "stats":
                        Console.WriteLine("\n\t*****Statistics Commands*****");
                        Console.WriteLine("\tnuser \t\t- \tGet Number of Users in the UserPool");
                        Console.WriteLine("\tgetulist \t- \tGet User List in the UserPool");
                        Console.WriteLine("\tgetrlist \t- \tGet Top-n Rank List from the UserPool");
                        Console.WriteLine("\tnloguser \t- \tGet Number of Users in the LoginUserPool");
                        Console.WriteLine("\tgetloglist \t- \tGet User List in the LoginUserPool");
                        Console.WriteLine("\tgetsubrlist \t- \tGet Top-n Rank List from UserPool Subset");
                        Console.WriteLine("\trulist \t\t- \tGet Rooms's User List");
                        Console.WriteLine("\trlist \t\t- \tGet Room List");
                        Console.WriteLine("\trranklist \t- \tGet Room Ranking List");
                        Console.WriteLine("\tloblist \t- \tGet Lobby User List");
                        break;
                    case "new":
                        Console.Write("Please enter a username: ");
                        name = Console.ReadLine();
                        Console.Write("Please enter a password: ");
                        password = Console.ReadLine();
                        flag = redis.CreateUser(name, password);
                        if (flag == true)
                        {
                            Console.WriteLine(name + " successfully added to the UserPool.");
                        }
                        else
                        {
                            Console.WriteLine(name + " is already in the UserPool.");
                        }
                        break;
                    case "del":
                        Console.Write("Please enter a username: ");
                        name = Console.ReadLine();
                        Console.Write("Please enter a password: ");
                        password = Console.ReadLine();
                        flag = redis.DeleteUser(name, password);
                        if (flag == true)
                        {
                            Console.WriteLine(name + " successfully deleted from the UserPool.");
                        } else
                        {
                            Console.WriteLine("Incorrect Username or Password.");
                        }
                        break;
                    case "uexist":
                        Console.Write("Please enter a username: ");
                        name = Console.ReadLine();
                        if (redis.DoesUsernameExist(name))
                        {
                            Console.Write(name + " exists in the UserPool.");
                        } else
                        {
                            Console.Write(name + " does not exist in the UserPool.");
                        }
                        break;
                    case "chklogin":
                        Console.Write("Please enter a username: ");
                        name = Console.ReadLine();
                        number = redis.IsUserLoggedIn(name);
                        switch (number)
                        {
                            case -1:
                                Console.WriteLine("User does not exist.");
                                break;
                            case 0:
                                Console.WriteLine(name + " is not logged in.");
                                break;
                            case 1:
                                Console.WriteLine(name + " is logged in.");
                                break;
                            default:
                                Console.WriteLine("Unexpected Error");
                                break;
                        }
                        break;
                    case "chkdummy":
                        Console.Write("Please enter a username: ");
                        name = Console.ReadLine();
                        number = redis.IsUserDummy(name);
                        switch (number)
                        {
                            case -1:
                                Console.WriteLine("User does not exist.");
                                break;
                            case 0:
                                Console.WriteLine(name + " is not a dummy client.");
                                break;
                            case 1:
                                Console.WriteLine(name + " is a dummy client.");
                                break;
                            default:
                                Console.WriteLine("Unexpected Error");
                                break;
                        }
                        break;
                    case "chkblock":
                        Console.Write("Please enter a username: ");
                        name = Console.ReadLine();
                        number = redis.IsUserBlocked(name);
                        switch (number)
                        {
                            case -1:
                                Console.WriteLine("User does not exist.");
                                break;
                            case 0:
                                Console.WriteLine(name + " is not blocked.");
                                break;
                            case 1:
                                Console.WriteLine(name + " is blocked.");
                                break;
                            case 2:
                                Console.WriteLine(name + " is no longer blocked.");
                                break;
                            default:
                                Console.WriteLine("Unexpected Error");
                                break;
                        }
                        break;
                    case "chgname":
                        Console.Write("Please enter a username: ");
                        name = Console.ReadLine();
                        Console.Write("Please enter a password: ");
                        password = Console.ReadLine();
                        Console.Write("Please desired new name: ");
                        response = Console.ReadLine();
                        numResult = redis.ChangeUsername(name, response, password);
                        switch (numResult)
                        {
                            case -3:
                                Console.WriteLine(response + " is already in use.");
                                break;
                            case -2:
                                Console.WriteLine("Current username is already " + response + ".");
                                break;
                            case -1:
                                Console.WriteLine(name + " is blocked.");
                                break;
                            case 0:
                                Console.WriteLine("Incorrect Username or Password.");
                                break;
                            case 1:
                                Console.WriteLine("Username successfully changed from " + name + " to " + response + ".");
                                break;
                            default:
                                Console.WriteLine("Unknown Error.");
                                break;
                        }
                        break;
                    case "chgpass":
                        Console.Write("Please enter a username: ");
                        name = Console.ReadLine();
                        Console.Write("Please enter a password: ");
                        password = Console.ReadLine();
                        Console.Write("Please desired new password: ");
                        response = Console.ReadLine();
                        numResult = redis.ChangePassword(name, password, response);
                        switch (numResult)
                        {
                            case -2:
                                Console.WriteLine("Password change unnessary.");
                                break;
                            case -1:
                                Console.WriteLine(name + " is blocked.");
                                break;
                            case 0:
                                Console.WriteLine("Incorrect Username or Password.");
                                break;
                            case 1:
                                Console.WriteLine("Password successfully changed.");
                                break;
                            default:
                                Console.WriteLine("Unknown Error.");
                                break;
                        }
                        break;
                    case "chgconn":
                        Console.Write("Please enter a username: ");
                        name = Console.ReadLine();
                        Console.Write("\nPlease enter a new connection ID: ");
                        number = int.Parse(Console.ReadLine());
                        numResult = redis.ChangeConnectionID(name, number);
                        if (numResult < 0)
                        {
                            Console.WriteLine("User does not exist.");
                        } else
                        {
                            Console.WriteLine(name + "'s connection ID has been changed from " + numResult + " to " + number + ".");
                        }
                        break;
                    case "blkuser":
                        Console.Write("Please enter a username: ");
                        name = Console.ReadLine();
                        Console.Write("\nPlease enter a time penalty in minutes: ");
                        number = int.Parse(Console.ReadLine());
                        time = redis.BlockUser(name, (uint)Math.Abs(number));
                        if (time == 0)
                        {
                            Console.WriteLine("User does not exist.");
                        } else
                        {
                            Console.WriteLine(name + " has been blocked by (an additional) " + number + " minute(s)");
                            timeLater = timeNow.AddSeconds(time);
                            Console.WriteLine("Suspension will end at " + timeLater + ".");
                        }
                        break;
                    case "ublkuser":
                        Console.Write("Please enter a username: ");
                        name = Console.ReadLine();
                        numResult = redis.UnBlockUser(name);
                        switch (numResult)
                        {
                            case -1:
                                Console.WriteLine("User is not blocked.");
                                break;
                            case 0:
                                Console.WriteLine("User does not exist.");
                                break;
                            case 1:
                                Console.WriteLine(name + " has been unblocked.");
                                break;
                            default:
                                Console.WriteLine("Unknown error.");
                                break;
                        }
                        break;
                    case "getrank":
                        Console.Write("Please enter a username: ");
                        name = Console.ReadLine();
                        number = redis.GetUserRank(name);
                        if (number >= 0)
                        {
                            Console.WriteLine(name + " has a rank of " + number + ".");
                        } else
                        {
                            Console.WriteLine("User does not exist.");
                        }
                        break;
                    case "getuloc":
                        Console.Write("Please enter a username: ");
                        name = Console.ReadLine();
                        number = redis.GetUserLocation(name);
                        if (number == 0)
                        {
                            Console.WriteLine(name + " is located in the Lobby.");
                        } else if (number > 0)
                        {
                            Console.WriteLine(name + " is located in Room " + number + ".");
                        }
                        else
                        {
                            Console.WriteLine("User does not exist.");
                        }
                        break;
                    case "getconnid":
                        Console.Write("Please enter a username: ");
                        name = Console.ReadLine();
                        number = redis.GetUserConnectionID(name);
                        if (number < 0)
                        {
                            Console.WriteLine("User does not exist.");
                        } else
                        {
                            Console.WriteLine(name + " has a connection ID of " + number);
                        }
                        break;
                    case "getsusp":
                        Console.Write("Please enter a username: ");
                        name = Console.ReadLine();
                        time = redis.GetUserSuspendTime(name);
                        if (time == 0)
                        {
                            Console.WriteLine("User does not exist.");
                        } else if (time == 1)
                        {
                            Console.WriteLine("User is not blocked.");
                        } else if (time == 2)
                        {
                            Console.WriteLine("User is no longer blocked.");
                        }
                        else
                        {
                            timeLater = timeNow.AddSeconds(time);
                            Console.WriteLine(name + " is suspended until " + timeLater + ".");
                        }
                        break;
                    case "getmsgcnt":
                        Console.Write("Please enter a username: ");
                        name = Console.ReadLine();
                        number = redis.GetUserMessageCount(name);
                        if (number < 0)
                        {
                            Console.WriteLine("User does not exist.");
                        }
                        else
                        {
                            Console.WriteLine(name + " has a message count of " + number);
                        }
                        break;
                    case "getrmsgcnt":
                        Console.Write("Please enter a rank value: ");
                        number = int.Parse(Console.ReadLine());
                        numResult = redis.GetMessageCountAtRank(number);
                        if (numResult < 0)
                        {
                            Console.WriteLine("Rank does not exist.");
                        }
                        else
                        {
                            Console.WriteLine("Rank " + number + " has a message count of " + numResult);
                        }
                        break;
                    case "incmsgcnt":
                        Console.Write("Please enter a username: ");
                        name = Console.ReadLine();
                        Console.Write("Please enter an amount to increment by: ");
                        number = int.Parse(Console.ReadLine());
                        numResult = redis.AddToUserMessageCount(name, number);
                        if (numResult >= 0)
                        {
                            Console.WriteLine(name + "'s message count updated by " + number + " to " + numResult + ".");
                        }
                        else if (numResult == -1)
                        {
                            Console.WriteLine("Incorrect Username or Password.");
                        } else
                        {
                            Console.WriteLine("Unknown Error.");
                        }
                        break;
                    case "login":
                        Console.Write("Please enter a username: ");
                        name = Console.ReadLine();
                        Console.Write("Please enter a password: ");
                        password = Console.ReadLine();
                        Console.Write("Is the user a dummy? (y = yes) ");
                        ckey = Console.ReadKey();
                        if (ckey.KeyChar == 'y' || ckey.KeyChar == 'Y')
                        {
                            flag = true;
                        } else
                        {
                            flag = false;
                        }
                        Console.Write("\nPlease enter a connection ID: ");
                        number = int.Parse(Console.ReadLine());
                        numResult = redis.Login(name, password, number, flag);
                        switch (numResult)
                        {
                            case -1:
                                Console.WriteLine("User is blocked.");
                                break;
                            case 0:
                                Console.WriteLine("Incorrect Username or Password.");
                                break;
                            default:
                                if (number != numResult)                                    //If the passed connection ID is different than the result, that means the connection was overridden
                                {
                                    Console.WriteLine(name + " was already logged in and their session has been overridden");
                                    Console.WriteLine(name + "'s connection ID has been changed from " + numResult + " to " + number + ".");
                                }
                                Console.WriteLine("Login Successful!");
                                break;
                        }
                        break;
                    case "logout":
                        Console.Write("Please enter a username: ");
                        name = Console.ReadLine();
                        numResult = redis.Logout(name);
                        switch (numResult)
                        {
                            case -1:
                                Console.WriteLine("User does not exist.");
                                break;
                            case 0:
                                Console.WriteLine(name + " is not logged in.");
                                break;
                            case 1:
                                Console.WriteLine(name + " has been logged out.");
                                break;
                            default:
                                Console.WriteLine("Unknown error.");
                                break;
                        }
                        break;
                    case "nuser":
                        bigNumber = redis.GetUserPoolSize ();
                        Console.WriteLine(bigNumber + " user(s) in the UserPool.");
                        break;
                    case "getulist":
                        nameList = redis.GetUserList();
                        for (int i = 0; i < nameList.Length; i++)
                        {
                            Console.Write(nameList[i] + "\t");
                            if ((i+1)%5 == 0)
                            {
                                Console.Write("\n");
                            }
                        }
                        Console.Write("\n");
                        break;
                    case "getrlist":
                        Console.Write("\nPlease enter the top-n list value (0 for full list): ");
                        number = int.Parse(Console.ReadLine());
                        rankList = redis.GetTopList(--number);
                        int x = 0;
                        foreach (KeyValuePair<string, double> keyValuePair in rankList)
                        {
                            x++;
                            Console.WriteLine("Rank " + x + ":\t" + keyValuePair.Key + "\t\t" + keyValuePair.Value);
                        }
                        break;
                    case "nloguser":
                        bigNumber = redis.GetLoginPoolSize();
                        Console.WriteLine(bigNumber + " user(s) logged in.");
                        break;
                    case "getloglist":
                        nameList = redis.GetLoginList();
                        for (int i = 0; i < nameList.Length; i++)
                        {
                            Console.Write(nameList[i] + "\t");
                            if ((i + 1) % 5 == 0)
                            {
                                Console.Write("\n");
                            }
                        }
                        Console.Write("\n");
                        break;
                    case "getsubrlist":
                        Console.Write("Please enter a sub-pool name (LoginPool, DummyPool, etc): ");
                        name = Console.ReadLine();
                        Console.Write("\nPlease enter the top-n list value (0 for full list): ");
                        number = int.Parse(Console.ReadLine());
                        rankList = redis.GetSubTopList(--number, name);
                        int y = 0;
                        foreach (KeyValuePair<string, double> keyValuePair in rankList)
                        {
                            y++;
                            Console.WriteLine("Rank " + y + ":\t" + keyValuePair.Key + "\t\t" + (keyValuePair.Value - 1));
                        }
                        break;
                    case "exit":
                        isUserFinished = true;
                        break;
                    case "rnew":
                        Console.Write("\nPlease enter room number: ");
                        number = int.Parse(Console.ReadLine());
                        Console.Write("Please enter a room title: ");
                        response = Console.ReadLine();
                        Console.Write("Please enter a room owner: ");
                        name = Console.ReadLine();
                        numResult = redis.RoomCreate((uint)number, response, name);
                        switch (numResult)
                        {
                            case -1:
                                Console.WriteLine("Room already exists.");
                                break;
                            case 0:
                                Console.WriteLine("Owner does not exist.");
                                break;
                            case 1:
                                Console.WriteLine("Room " + number + " - \"" + response + "\" successfully created and owned by " + name + ".");
                                break;
                            default:
                                Console.WriteLine("Unknown Error.");
                                break;
                        }
                        break;
                    case "rdel":
                        Console.WriteLine("Cannot delete a room manually. The room will delete upon the last user leaving.");
                        break;
                    case "rexist":
                        Console.Write("\nPlease enter room number: ");
                        number = int.Parse(Console.ReadLine());
                        if(redis.RoomExist((uint)number))
                        {
                            Console.WriteLine("Room " + number + " exists.");
                        } else
                        {
                            Console.WriteLine("Room " + number + " does not exist.");
                        }
                        break;
                    case "rconuser":
                        Console.Write("\nPlease enter room number: ");
                        number = int.Parse(Console.ReadLine());
                        Console.Write("Please enter a name: ");
                        name = Console.ReadLine();
                        numResult = redis.RoomContainsUser((uint)number, name);
                        switch (numResult)
                        {
                            case -2:
                                Console.WriteLine("User does not exist.");
                                break;
                            case -1:
                                Console.WriteLine("Room does not exist.");
                                break;
                            case 0:
                                Console.WriteLine(name + " is not in Room " + number + ".");
                                break;
                            case 1:
                                Console.WriteLine(name + " is in Room " + number + ".");
                                break;
                            default:
                                Console.WriteLine("Unknown Error.");
                                break;
                        }
                        break;
                    case "rtitle":
                        Console.Write("\nPlease enter room number: ");
                        number = int.Parse(Console.ReadLine());
                        if (redis.RoomGetTitle((uint)number, out name))
                        {
                            Console.WriteLine("Room " + number + "'s title is \"" + name + ".");
                        } else
                        {
                            Console.WriteLine("Room does not exist.");
                        }
                        break;
                    case "rucount":
                        Console.Write("\nPlease enter room number: ");
                        number = int.Parse(Console.ReadLine());
                        numResult = redis.RoomGetUserCount((uint)number);
                        if (numResult < 0)
                        {
                            Console.WriteLine("Room does not exist.");
                        } else
                        {
                            Console.WriteLine("Room " + number + " contains " + numResult + " user(s).");
                        }
                        break;
                    case "rowner":
                        Console.Write("\nPlease enter room number: ");
                        number = int.Parse(Console.ReadLine());
                        if (redis.RoomGetOwner((uint)number, out name))
                        {
                            Console.WriteLine("Room " + number + "'s owner is " + name + ".");
                        }
                        else
                        {
                            Console.WriteLine("Room does not exist.");
                        }
                        break;
                    case "rsizerank":
                        Console.Write("\nPlease enter room number: ");
                        number = int.Parse(Console.ReadLine());
                        numResult = redis.RoomGetSizeRank((uint)number);
                        if (numResult < 0)
                        {
                            Console.WriteLine("Room does not exist.");
                        }
                        else
                        {
                            Console.WriteLine("Room " + number + "'s size ranking is " + numResult + ".");
                        }
                        break;
                    case "rchgtitle":
                        Console.Write("\nPlease enter room number: ");
                        number = int.Parse(Console.ReadLine());
                        Console.Write("Please enter a new room title: ");
                        response = Console.ReadLine();
                        numResult = redis.RoomChangeTitle((uint)number, response, out name);
                        switch (numResult)
                        {
                            case -1:
                                Console.WriteLine("Room does not exist.");
                                break;
                            case 0:
                                Console.WriteLine("The new room title is not different than the current setting.");
                                break;
                            case 1:
                                Console.WriteLine("Room " + number + "'s title has been changed from \"" + name + "\" to \"" + response + ".\"");
                                break;
                            default:
                                Console.WriteLine("Unknown Error.");
                                break;
                        }
                        break;
                    case "rsetown":
                        Console.Write("\nPlease enter room number: ");
                        number = int.Parse(Console.ReadLine());
                        Console.Write("Please enter a new room owner name: ");
                        response = Console.ReadLine();
                        numResult = redis.RoomSetOwner((uint)number, response, out name);
                        switch (numResult)
                        {
                            case -1:
                                Console.WriteLine("Room does not exist.");
                                break;
                            case 0:
                                Console.WriteLine("The new room owner is not different than the current setting.");
                                break;
                            case 1:
                                Console.WriteLine("Room " + number + "'s owner has been changed from " + name + " to " + response + ".");
                                break;
                            default:
                                Console.WriteLine("Unknown Error.");
                                break;
                        }
                        break;
                    case "radduse":
                        Console.Write("\nPlease enter room number: ");
                        number = int.Parse(Console.ReadLine());
                        Console.Write("Please enter a name: ");
                        name = Console.ReadLine();
                        numResult = redis.RoomAddUser((uint)number, name);
                        switch (numResult)
                        {
                            case -2:
                                Console.WriteLine(name + " already in Room " + number + ".");
                                break;
                            case -1:
                                Console.WriteLine("Room does not exist.");
                                break;
                            case 0:
                                Console.WriteLine("User does not exist.");
                                break;
                            case 1:
                                Console.WriteLine(name + " added to Room " + number + ".");
                                break;
                            default:
                                Console.WriteLine("Unknown Error.");
                                break;
                        }
                        break;
                    case "rremuse":
                        Console.Write("\nPlease enter room number: ");
                        number = int.Parse(Console.ReadLine());
                        Console.Write("Please enter a name: ");
                        name = Console.ReadLine();
                        numResult = redis.RoomRemoveUser((uint)number, name);
                        switch (numResult)
                        {
                            case -3:
                                Console.WriteLine(name + " is not in Room " + number + ".");
                                break;
                            case -2:
                                Console.WriteLine(name + " removed from Room " + number + ".");
                                Console.WriteLine("Error while trying to close Room " + number + "due to it being empty.");
                                break;
                            case -1:
                                Console.WriteLine("Room does not exist.");
                                break;
                            case 0:
                                Console.WriteLine("User does not exist.");
                                break;
                            case 1:
                                Console.WriteLine(name + " removed from Room " + number + ".");
                                break;
                            case 2:
                                Console.WriteLine(name + " removed from Room " + number + ".");
                                Console.WriteLine("Room " + number + "'s was closed because it became empty.");
                                break;
                            case 3:
                                Console.WriteLine(name + " removed from Room " + number + ".");
                                Console.WriteLine("Error in re-assigning room's owner.");
                                break;
                            case 4:
                                Console.WriteLine(name + " removed from Room " + number + ".");
                                if (redis.RoomGetOwner((uint)number, out response))
                                {
                                    Console.WriteLine("Room " + number + "'s owner changed from " + name + " to " + response + ".");
                                }
                                else
                                {
                                    Console.WriteLine("Error in re-assigning room's owner.");
                                }
                                break;
                            default:
                                Console.WriteLine("Unknown Error.");
                                break;
                        }
                        break;
                    case "rulist":
                        Console.Write("\nPlease enter room number: ");
                        number = int.Parse(Console.ReadLine());
                        nameList = redis.RoomUserList((uint)number);
                        if (nameList == null)
                        {
                            Console.WriteLine("Room does not exist.");
                        } else
                        {
                            for (int i = 0; i < nameList.Length; i++)
                            {
                                Console.Write(nameList[i] + "\t");
                                if ((i + 1) % 5 == 0)
                                {
                                    Console.Write("\n");
                                }
                            }
                            Console.Write("\n");
                        }
                        break;
                    case "rlist":
                        nameList = redis.RoomList();
                        if (nameList == null)
                        {
                            Console.WriteLine("No rooms exist.");
                        }
                        else
                        {
                            for (int i = 0; i < nameList.Length; i++)
                            {
                                Console.Write(nameList[i] + "\t");
                                if (redis.RoomGetTitle(uint.Parse(nameList[i].Substring(5)), out name))
                                {
                                    Console.Write(" - \"" + name + "\"");
                                }
                                Console.Write("\t");
                                if ((i + 1) % 4 == 0)
                                {
                                    Console.Write("\n");
                                }
                            }
                            Console.Write("\n");
                        }
                        break;
                    case "rranklist":
                        Console.Write("\nPlease enter the top-n list value (0 for full list): ");
                        number = int.Parse(Console.ReadLine());
                        rankList = redis.RoomSizeRankList(--number);
                        int z = 0;
                        foreach (KeyValuePair<string, double> keyValuePair in rankList)
                        {
                            z++;
                            Console.WriteLine("Rank " + z + ":\t" + keyValuePair.Key + "\t\t" + keyValuePair.Value);
                        }
                        break;
                    case "loblist":
                        nameList = redis.GetLobbyUserList();
                        if (nameList == null)
                        {
                            Console.WriteLine("The Lobby is empty.");
                        }
                        else
                        {
                            for (int i = 0; i < nameList.Length; i++)
                            {
                                Console.Write(nameList[i] + "\t");
                                if ((i + 1) % 5 == 0)
                                {
                                    Console.Write("\n");
                                }
                            }
                            Console.Write("\n");
                        }
                        break;
                    default:
                        Console.WriteLine("\n---Invalid Entry---\n");
                        break;
                }

            }
            Console.WriteLine("Thank you for using the Redis Test Client. Enter anything to exit.");

            Console.ReadLine();

            redis.CloseConnection();
        }
    }
}
