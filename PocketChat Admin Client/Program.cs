using System;
using System.Collections.Generic;
using MikRedisDB;
using System.Text;
using System.IO;
using System.Threading;

namespace PocketChatAdminClient
{
    class Program
    {
        static void Main(string[] args)
        {
            //Loop Control Variables
            bool isConnectionEstablished = false;
            bool isAdminFinished = false;
            //Redis Client Handler
            RedisDBController redis = new RedisDBController();
            //Input variables
            string input;
            //Output variables
            string messageToAdmin = null;
            //Variables for config file retrieval
            int tempX;
            string configFile = "redisconfig.txt";
            string[] configInfo = null;

            //Loop until the user is able to load a probably-okay config file containing the Redis server's IP, port, and password
            while (true)
            {
                var path = Path.Combine(Directory.GetCurrentDirectory(), configFile);           //Get the location of the config file
                //Be sure to catch any file reading errors
                try
                {
                    configInfo = File.ReadAllLines(path);                                       //Read the config file
                    if (configInfo.Length < 3)                                                  //Ensure that the config file has at least three entries
                    {
                        Console.WriteLine("\nPlease ensure that " + configFile + " contains the Redis server IP, port number, and password.");
                    } else
                    {
                        if (!int.TryParse(configInfo[1], out tempX))                            //Check that the port number string can be parsed correctly into a numeric value
                        {
                            Console.WriteLine("\nPlease ensure that " + configFile + " contains a numeric value for the port number.");
                        } else
                        {
                            break;
                        }
                    }
                }
                catch (FileNotFoundException e)
                {
                    Console.WriteLine("\nFileNotFoundException during " + configFile + " reading attempt.\n");
                    Console.WriteLine(e.HResult + " : " + e.Message);
                    Console.WriteLine("\nPlease ensure that " + configFile + " is contained within the application's directory.");
                }
                catch (Exception e)
                {
                    Console.WriteLine("\nException during " + configFile + " reading attempt.\n");
                    Console.WriteLine(e.HResult + " : " + e.Message);
                    Console.WriteLine("\nPlease ensure that " + configFile + " is contained within the application's directory.");
                }
                
                Console.Write("\tEnter q to quit or any other key to try again: ");             //Prompt a retry in case of some type of failure
                if(Console.ReadKey().KeyChar == 'q')
                {
                    return;
                }
            }
            Console.Clear();                                                                    //Clear the console to allow for clean UI generation
            
            //Loop until the user establishes a connection with the client
            while (isConnectionEstablished == false)
            {
                //Welcome UI
                Console.WriteLine("==========================================================");
                Console.WriteLine("================Welcome to the POCKETCHAT!!===============");
                Console.WriteLine("===================Admin Client Portal====================");
                Console.WriteLine("===============CONNECT=================EXIT===============");
                Console.WriteLine(messageToAdmin);
                Console.Write("> ");
                input = Console.ReadLine();

                //Check for correct input and attempt connection
                switch (input)
                {
                    case "CONNECT":
                    case "connect":
                        Console.Write("Connecting to Redis server. . .");
                        try
                        {
                            redis.SetConfigurationOptions(configInfo[0], int.Parse(configInfo[1]), configInfo[2]);      //Setup Configuration Options
                            redis.SetupConnection();                                                                    //Attempt connection
                        }
                        catch
                        {
                            messageToAdmin = "\t\t---Connection Failed---";
                            break;
                        }
                        Console.WriteLine(" . . .Connected!");
                        messageToAdmin = "\t\t---Connection Succeeded---";
                        isConnectionEstablished = true;
                        break;
                    case "EXIT":
                    case "exit":
                        Console.WriteLine("Thank you for using POCKETCHAT Admin Client Portal. Enter anything to exit.");
                        Console.ReadLine();
                        return;
                    default:
                        messageToAdmin = "\t\t---Invalid Command---";
                        break;
                }
                Console.Clear();
            }

            //Variables for holding general command results
            bool flag = false;
            bool dbEmulationMode = false;
            bool menuOn = true;
            byte menuID = 0;
            int number = 0;
            int numResult = 0;
            uint anotherNumber = 0;
            uint time = 0;
            long bigNumber = 0;
            DateTime timeNow = new DateTime(1970, 1, 1);
            DateTime timeLater;
            ConsoleKeyInfo ckey;
            string response = null;
            string name;
            string password;
            string[] nameList;
            Dictionary<string, double> rankList;

            //UI Top
            Console.WriteLine("==========================================================");
            Console.WriteLine("================Welcome to the POCKETCHAT!!===============");
            Console.WriteLine("===================Admin Client Portal====================");
            Console.WriteLine("==========================================================");
            Console.WriteLine(messageToAdmin);
            messageToAdmin = null;

            //This while loop runs until the admin wants to quit the program
            while (isAdminFinished == false)
            {
                if (menuOn)                                                                                 //If the menu is on, print the menu
                {
                    switch (menuID)                                                                         //Print the menu in which the admin is located
                    {
                        case 1:
                            Console.WriteLine("\t*****User and Room Information Commands*****");
                            Console.WriteLine("\tuexist \t\t- \tCheck User Existence");
                            Console.WriteLine("\tchklogin \t- \tCheck If User is Logged In");
                            Console.WriteLine("\tchkdummy \t- \tCheck If User is a Dummy");
                            Console.WriteLine("\tchkblock \t- \tCheck If User is Blocked");
                            Console.WriteLine("\tgetrank \t- \tGet User's Rank");
                            Console.WriteLine("\tgetuloc \t- \tGet User's Location");
                            Console.WriteLine("\tgetconnid \t- \tGet User Connection ID");
                            Console.WriteLine("\tgetsusp \t- \tGet User Suspend Time");
                            Console.WriteLine("\tgetmsgcnt \t- \tGet User Message Count");
                            Console.WriteLine("\tgetrmsgcnt \t- \tGet User Message Count at rank");
                            Console.WriteLine("\tgetruser \t- \tGet User at rank");
                            Console.WriteLine("\trexist \t\t- \tCheck Room Existence");
                            Console.WriteLine("\trconuser \t- \tCheck If a User is in a room");
                            Console.WriteLine("\trtitle \t\t- \tGet room title");
                            Console.WriteLine("\trucount \t- \tGet room user count");
                            Console.WriteLine("\trowner \t\t- \tGet room owner");
                            Console.WriteLine("\trservid \t\t- \tGet room server ID");
                            Console.WriteLine("\trsizerank \t- \tGet room size rank");
                            Console.WriteLine("\tservrcount \t- \tGet server room count");
                            Console.WriteLine("\tback \t\t- \tReturn to Main Menu");
                            break;
                        case 2:
                            Console.WriteLine("\t*****Moderator Commands*****");
                            Console.WriteLine("\tnew \t\t- \tCreate New User");
                            Console.WriteLine("\tdel \t\t- \tDelete User");
                            Console.WriteLine("\trnew \t\t- \tCreate New Room");
                            Console.WriteLine("\trdel \t\t- \tDelete Room");
                            Console.WriteLine("\tchgname \t- \tChange Username");
                            Console.WriteLine("\tchgpass \t- \tChange User Password");
                            Console.WriteLine("\tchgconn \t- \tChange User Connection ID");
                            Console.WriteLine("\tblkuser \t- \tBlock User");
                            Console.WriteLine("\tublkuser \t- \tUnblock User");
                            Console.WriteLine("\tincmsgcnt \t- \tModify a User's Message Count");
                            Console.WriteLine("\tlogin \t\t- \tAttempt User Login");
                            Console.WriteLine("\tlogout \t\t- \tAttempt User Logout");
                            Console.WriteLine("\trchgtitle \t- \tChange Room Title");
                            Console.WriteLine("\trsetown \t- \tChange Room Owner");
                            Console.WriteLine("\trsetserv \t- \tChange Room Server ID");
                            Console.WriteLine("\tradduse \t- \tAdd user to room");
                            Console.WriteLine("\trremuse \t- \tRemove user from room");
                            Console.WriteLine("\trpurge \t- \t**Purge and delete room");
                            Console.WriteLine("\tback \t\t- \tReturn to Main Menu");
                            break;
                        case 3:
                            Console.WriteLine("\t*****DB Statistics Commands*****");
                            Console.WriteLine("\tnuser \t\t- \tGet Number of Users in the UserPool");
                            Console.WriteLine("\tgetulist \t- \tGet User List in the UserPool");
                            Console.WriteLine("\tgetrlist \t- \tGet Top-n Rank List from the UserPool");
                            Console.WriteLine("\tnloguser \t- \tGet Number of Users in the LoginUserPool");
                            Console.WriteLine("\tndummy \t- \tGet Number of Users in the LoginUserPool");
                            Console.WriteLine("\tgetloglist \t- \tGet User List in the LoginUserPool");
                            Console.WriteLine("\tgetsubrlist \t- \tGet Top-n Rank List from UserPool Subset");
                            Console.WriteLine("\trulist \t\t- \tGet Rooms's User List");
                            Console.WriteLine("\trlist \t\t- \tGet Room List");
                            Console.WriteLine("\trlistserv \t- \tGet Rooms in a Particular Server");
                            Console.WriteLine("\trranklist \t- \tGet Room Ranking List");
                            Console.WriteLine("\tloblist \t- \tGet Lobby User List");
                            Console.WriteLine("\tservrank \t- \tGet Server Room Count Rankings");
                            Console.WriteLine("\tback \t\t- \tReturn to Main Menu");
                            break;
                        case 4:
                            MonitoringMode (redis);
                            menuID = 0;
                            Console.Clear();
                            Console.WriteLine("==========================================================");
                            Console.WriteLine("================Welcome to the POCKETCHAT!!===============");
                            Console.WriteLine("===================Admin Client Portal====================");
                            Console.WriteLine("==========================================================");
                            Console.WriteLine("\n\t***Live Monitoring Mode Ended***");
                            break;
                        case 0:
                        default:
                            if (dbEmulationMode == true)
                            {
                                Console.WriteLine("\t*****WARNING: Moderator Mode Permanently Alters DB Information*****");
                                
                            }
                            Console.WriteLine("\tlive \t\t- \tEnter Live Monitoring Mode");
                            Console.WriteLine("\tstats \t\t- \tGet DB statistics commands");
                            Console.WriteLine("\tinfolist \t- \tGet User and Room Information Commands");
                            if (dbEmulationMode == false)
                            {
                                Console.WriteLine("\tmmode \t\t- \tEnter Moderator Mode");
                            } else
                            {
                                Console.WriteLine("\tmodlist \t- \tGet full list of room commands");
                            }
                            Console.WriteLine("\treconn \t\t- \tAttempt to reconnect to the Redis server");
                            Console.WriteLine("\texit \t\t- \tQuit");
                            break;
                    }

                    //If menu mode is on, get the next command then redraw the window
                    Console.Write("\n> ");
                    response = Console.ReadLine();
                    Console.Clear();

                    menuOn = false;
                }

                //UI Top
                Console.WriteLine("==========================================================");
                Console.WriteLine("================Welcome to the POCKETCHAT!!===============");
                Console.WriteLine("===================Admin Client Portal====================");
                Console.WriteLine("==========================================================");
                messageToAdmin = null;

                //This switch statement takes care of all possible commands
                switch (response)
                {
                    //***Menu Commands***
                    case "infolist":
                        menuID = 1;
                        menuOn = true;
                        break;
                    case "modlist":
                        menuID = 2;
                        menuOn = true;
                        break;
                    case "stats":
                        menuID = 3;
                        menuOn = true;
                        break;
                    case "live":
                        menuID = 4;
                        menuOn = true;
                        break;
                    case "back":
                        menuID = 0;
                        menuOn = true;
                        break;
                    case "menu":
                        menuOn = true;
                        break;
                    //***Activation of Moderator Mode***
                    case "mmode":
                        Console.Write("Please enter the db emulation password: ");
                        password = Console.ReadLine();
                        if (password.CompareTo("3dalkgalbi!") == 0)
                        {
                            messageToAdmin = "\t***Moderator Mode Activated!***";
                            dbEmulationMode = true;
                        } else
                        {
                            messageToAdmin = "\t***Moderator Mode Activation Failed!***";
                        }
                        break;
                    //***User related commands***
                    case "reconn":
                        Console.Write("Connecting to Redis server. . .");
                        try
                        {
                            redis.SetupConnection();             
                        }
                        catch
                        {
                            messageToAdmin = "\t\t---Connection Failed---";
                            break;
                        }
                        Console.WriteLine(" . . .connecting in progress. . .");
                        messageToAdmin = "\t\t---Connection Succeeded---";
                        break;
                    case "new":         //Create new user
                        if (dbEmulationMode == false)
                        {
                            messageToAdmin = "***Error: DB Moderator Mode must be activated before using command \'" + response + "\'***";
                            break;
                        }
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
                    case "del":         //Delete User
                        if (dbEmulationMode == false)
                        {
                            messageToAdmin = "***Error: DB Moderator Mode must be activated before using command \'" + response + "\'***";
                            break;
                        }
                        Console.Write("Please enter a username: ");
                        name = Console.ReadLine();
                        Console.Write("Please enter a password: ");
                        password = Console.ReadLine();
                        flag = redis.DeleteUser(name, password);
                        if (flag == true)
                        {
                            Console.WriteLine(name + " successfully deleted from the UserPool.");
                        }
                        else
                        {
                            Console.WriteLine("Incorrect Username or Password.");
                        }
                        break;
                    case "uexist":      //Check user existence
                        Console.Write("Please enter a username: ");
                        name = Console.ReadLine();
                        if (redis.DoesUsernameExist(name))
                        {
                            Console.Write(name + " exists in the UserPool.");
                        }
                        else
                        {
                            Console.Write(name + " does not exist in the UserPool.");
                        }
                        break;
                    case "chklogin":    //Check user login status
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
                    case "chkdummy":    //Check user dummy status
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
                    case "chkblock":    //Check user block status
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
                    case "chgname":     //Change username
                        if (dbEmulationMode == false)
                        {
                            messageToAdmin = "***Error: DB Moderator Mode must be activated before using command \'" + response + "\'***";
                            break;
                        }
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
                    case "chgpass":     //Change user password
                        if (dbEmulationMode == false)
                        {
                            messageToAdmin = "***Error: DB Moderator Mode must be activated before using command \'" + response + "\'***";
                            break;
                        }
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
                                Console.WriteLine("Password change unnecessary.");
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
                    case "chgconn":     //Change connection ID
                        if (dbEmulationMode == false)
                        {
                            messageToAdmin = "***Error: DB Moderator Mode must be activated before using command \'" + response + "\'***";
                            break;
                        }
                        Console.Write("Please enter a username: ");
                        name = Console.ReadLine();
                        Console.Write("\nPlease enter a new connection ID: ");
                        if (!int.TryParse(Console.ReadLine(), out number))
                        {
                            Console.WriteLine("\t---Incorrect Input---");
                            break;
                        }
                        numResult = redis.ChangeConnectionID(name, number);
                        if (numResult < 0)
                        {
                            Console.WriteLine("User does not exist.");
                        }
                        else
                        {
                            Console.WriteLine(name + "'s connection ID has been changed from " + numResult + " to " + number + ".");
                        }
                        break;
                    case "blkuser":     //Block User
                        if (dbEmulationMode == false)
                        {
                            messageToAdmin = "***Error: DB Moderator Mode must be activated before using command \'" + response + "\'***";
                            break;
                        }
                        Console.Write("Please enter a username: ");
                        name = Console.ReadLine();
                        Console.Write("\nPlease enter a time penalty in minutes: ");
                        if (!int.TryParse(Console.ReadLine(), out number))
                        {
                            Console.WriteLine("\t---Incorrect Input---");
                            break;
                        }
                        time = redis.BlockUser(name, (uint)Math.Abs(number));
                        if (time == 0)
                        {
                            Console.WriteLine("User does not exist.");
                        }
                        else
                        {
                            Console.WriteLine(name + " has been blocked by (an additional) " + number + " minute(s)");
                            timeLater = timeNow.AddSeconds(time);
                            Console.WriteLine("Suspension will end at " + timeLater + ".");
                        }
                        break;
                    case "ublkuser":    //Unblock user
                        if (dbEmulationMode == false)
                        {
                            messageToAdmin = "***Error: DB Moderator Mode must be activated before using command \'" + response + "\'***";
                            break;
                        }
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
                    case "getrank":     //Get user's rank
                        Console.Write("Please enter a username: ");
                        name = Console.ReadLine();
                        number = redis.GetUserRank(name);
                        if (number >= 0)
                        {
                            Console.WriteLine(name + " has a rank of " + number + ".");
                        }
                        else
                        {
                            Console.WriteLine("User does not exist.");
                        }
                        break;
                    case "getuloc":     //Get user location
                        Console.Write("Please enter a username: ");
                        name = Console.ReadLine();
                        number = redis.GetUserLocation(name);
                        if (number == 0)
                        {
                            Console.WriteLine(name + " is located in the Lobby.");
                        }
                        else if (number > 0)
                        {
                            Console.WriteLine(name + " is located in Room " + number + ".");
                        }
                        else
                        {
                            Console.WriteLine("User does not exist.");
                        }
                        break;
                    case "getconnid":   //Get user connection ID
                        Console.Write("Please enter a username: ");
                        name = Console.ReadLine();
                        number = redis.GetUserConnectionID(name);
                        if (number < 0)
                        {
                            Console.WriteLine("User does not exist.");
                        }
                        else
                        {
                            Console.WriteLine(name + " has a connection ID of " + number);
                        }
                        break;
                    case "getsusp":     //Get suspension time
                        Console.Write("Please enter a username: ");
                        name = Console.ReadLine();
                        time = redis.GetUserSuspendTime(name);
                        if (time == 0)
                        {
                            Console.WriteLine("User does not exist.");
                        }
                        else if (time == 1)
                        {
                            Console.WriteLine("User is not blocked.");
                        }
                        else if (time == 2)
                        {
                            Console.WriteLine("User is no longer blocked.");
                        }
                        else
                        {
                            timeLater = timeNow.AddSeconds(time);
                            Console.WriteLine(name + " is suspended until " + timeLater + ".");
                        }
                        break;
                    case "getmsgcnt":   //Get user message count
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
                    case "getrmsgcnt":  //Get message count by rank
                        Console.Write("Please enter a rank value: ");
                        if (!int.TryParse(Console.ReadLine(), out number))
                        {
                            Console.WriteLine("\t---Incorrect Input---");
                            break;
                        }
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
                    case "getruser":    //Get user by rank
                        Console.Write("Please enter a rank value: ");
                        if (!int.TryParse(Console.ReadLine(), out number))
                        {
                            Console.WriteLine("\t---Incorrect Input---");
                            break;
                        }
                        response = redis.GetUsernameAtRank(number);
                        if (response.CompareTo("0") == 0)
                        {
                            Console.WriteLine("Rank does not exist.");
                        } else if (response.CompareTo("1") == 0)
                        {
                            Console.WriteLine("Redis connection error!");
                            break;
                        } else
                        {
                            Console.WriteLine("Rank " + number + " is held by " + response);
                        }
                        break;
                    case "incmsgcnt":   //Modify message count
                        if (dbEmulationMode == false)
                        {
                            messageToAdmin = "***Error: DB Moderator Mode must be activated before using command \'" + response + "\'***";
                            break;
                        }
                        Console.Write("Please enter a username: ");
                        name = Console.ReadLine();
                        Console.Write("Please enter an amount to increment by: ");
                        if (!int.TryParse(Console.ReadLine(), out number))
                        {
                            Console.WriteLine("\t---Incorrect Input---");
                            break;
                        }
                        numResult = redis.AddToUserMessageCount(name, number);
                        if (numResult >= 0)
                        {
                            Console.WriteLine(name + "'s message count updated by " + number + " to " + numResult + ".");
                        }
                        else if (numResult == -1)
                        {
                            Console.WriteLine("Incorrect Username or Password.");
                        }
                        else
                        {
                            Console.WriteLine("Unknown Error.");
                        }
                        break;
                    case "login":       //Login attempt
                        if (dbEmulationMode == false)
                        {
                            messageToAdmin = "***Error: DB Moderator Mode must be activated before using command \'" + response + "\'***";
                            break;
                        }
                        Console.Write("Please enter a username: ");
                        name = Console.ReadLine();
                        Console.Write("Please enter a password: ");
                        password = Console.ReadLine();
                        Console.Write("Is the user a dummy? (y = yes) ");
                        ckey = Console.ReadKey();
                        if (ckey.KeyChar == 'y' || ckey.KeyChar == 'Y')
                        {
                            flag = true;
                        }
                        else
                        {
                            flag = false;
                        }
                        Console.Write("\nPlease enter a connection ID: ");
                        if (!int.TryParse(Console.ReadLine(), out number))
                        {
                            Console.WriteLine("\t---Incorrect Input---");
                            break;
                        }
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
                    case "logout":      //Logout attempt
                        if (dbEmulationMode == false)
                        {
                            messageToAdmin = "***Error: DB Moderator Mode must be activated before using command \'" + response + "\'***";
                            break;
                        }
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
                    case "nuser":       //Number of registered users
                        bigNumber = redis.GetUserPoolSize();
                        Console.WriteLine(bigNumber + " user(s) in the UserPool.");
                        break;
                    case "getulist":    //Get user list
                        nameList = redis.GetUserList();
                        //Prevent null reference errors upon Redis connection cut
                        if (nameList == null)
                        {
                            Console.WriteLine("Redis connection error!");
                            break;
                        }
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
                    case "getrlist":    //Get rank listing
                        Console.Write("\nPlease enter the top-n list value (0 for full list): ");
                        if (!int.TryParse(Console.ReadLine(), out number))
                        {
                            Console.WriteLine("\t---Incorrect Input---");
                            break;
                        }
                        rankList = redis.GetTopList(--number);
                        //Prevent null reference errors upon Redis connection cut
                        if (rankList == null)
                        {
                            Console.WriteLine("Redis connection error!");
                            break;
                        }
                        int x = 0;
                        foreach (KeyValuePair<string, double> keyValuePair in rankList)
                        {
                            x++;
                            Console.WriteLine("Rank " + x + ":\t" + keyValuePair.Key + "\t\t" + keyValuePair.Value);
                        }
                        break;
                    case "nloguser":    //Number of logged in users
                        bigNumber = redis.GetLoginPoolSize();
                        Console.WriteLine(bigNumber + " real user(s) logged in.");
                        break;
                    case "ndummy":      //Number of dummy users
                        bigNumber = redis.GetDummyPoolSize();
                        Console.WriteLine(bigNumber + " dummy(s) logged in.");
                        break;
                    case "getloglist":  //Get login list
                        nameList = redis.GetLoginList();
                        //Prevent null reference errors upon Redis connection cut
                        if (nameList == null)
                        {
                            Console.WriteLine("Redis connection error!");
                            break;
                        }
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
                    case "getsubrlist": //Get ranking sub-list
                        Console.Write("Please enter a sub-pool name (LoginPool, DummyPool, etc): ");
                        name = Console.ReadLine();
                        Console.Write("\nPlease enter the top-n list value (0 for full list): ");
                        if (!int.TryParse(Console.ReadLine(), out number))
                        {
                            Console.WriteLine("\t---Incorrect Input---");
                            break;
                        }
                        rankList = redis.GetSubTopList(--number, name);
                        //Prevent null reference errors upon Redis connection cut
                        if (rankList == null)
                        {
                            Console.WriteLine("Redis connection error!");
                            break;
                        }
                        int y = 0;
                        foreach (KeyValuePair<string, double> keyValuePair in rankList)
                        {
                            y++;
                            Console.WriteLine("Rank " + y + ":\t" + keyValuePair.Key + "\t\t" + (keyValuePair.Value - 1));
                        }
                        break;
                    case "exit":        //Exit application
                        isAdminFinished = true;
                        menuOn = true;
                        break;
                    case "rnew":        //Create new room
                        if (dbEmulationMode == false)
                        {
                            messageToAdmin = "***Error: DB Moderator Mode must be activated before using command \'" + response + "\'***";
                            break;
                        }
                        Console.Write("\nPlease enter room number: ");
                        if (!int.TryParse(Console.ReadLine(), out number))
                        {
                            Console.WriteLine("\t---Incorrect Input---");
                            break;
                        }
                        Console.Write("Please enter a room title: ");
                        response = Console.ReadLine();
                        Console.Write("Please enter a room owner: ");
                        name = Console.ReadLine();
                        Console.Write("\nPlease enter server ID: ");
                        if (!uint.TryParse(Console.ReadLine(), out anotherNumber))
                        {
                            Console.WriteLine("\t---Incorrect Input---");
                            break;
                        }
                        numResult = redis.RoomCreate((uint)number, response, name, anotherNumber);
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
                    case "rdel":        //Delete room
                        if (dbEmulationMode == false)
                        {
                            messageToAdmin = "***Error: DB Moderator Mode must be activated before using command \'" + response + "\'***";
                            break;
                        }
                        Console.WriteLine("Cannot delete a room manually. The room will delete upon the last user leaving.");
                        break;
                    case "rexist":      //Check room existence
                        Console.Write("\nPlease enter room number: ");
                        if (!int.TryParse(Console.ReadLine(), out number))
                        {
                            Console.WriteLine("\t---Incorrect Input---");
                            break;
                        }
                        if (redis.RoomExist((uint)number))
                        {
                            Console.WriteLine("Room " + number + " exists.");
                        }
                        else
                        {
                            Console.WriteLine("Room " + number + " does not exist.");
                        }
                        break;
                    case "rconuser":    //Check if room contains a user
                        Console.Write("\nPlease enter room number: ");
                        if (!int.TryParse(Console.ReadLine(), out number))
                        {
                            Console.WriteLine("\t---Incorrect Input---");
                            break;
                        }
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
                    case "rtitle":      //Check room title
                        Console.Write("\nPlease enter room number: ");
                        if (!int.TryParse(Console.ReadLine(), out number))
                        {
                            Console.WriteLine("\t---Incorrect Input---");
                            break;
                        }
                        if (redis.RoomGetTitle((uint)number, out name))
                        {
                            //Prevent null reference errors upon Redis connection cut
                            if (name == null)
                            {
                                Console.WriteLine("Redis connection error!");
                                break;
                            }
                            Console.WriteLine("Room " + number + "'s title is \"" + name + ".");
                        }
                        else
                        {
                            Console.WriteLine("Room does not exist.");
                        }
                        break;
                    case "rucount":     //Get room's user count
                        Console.Write("\nPlease enter room number: ");
                        if (!int.TryParse(Console.ReadLine(), out number))
                        {
                            Console.WriteLine("\t---Incorrect Input---");
                            break;
                        }
                        numResult = redis.RoomGetUserCount((uint)number);
                        if (numResult < 0)
                        {
                            Console.WriteLine("Room does not exist.");
                        }
                        else
                        {
                            Console.WriteLine("Room " + number + " contains " + numResult + " user(s).");
                        }
                        break;
                    case "rowner":      //Get room's owner
                        Console.Write("\nPlease enter room number: ");
                        if (!int.TryParse(Console.ReadLine(), out number))
                        {
                            Console.WriteLine("\t---Incorrect Input---");
                            break;
                        }
                        if (redis.RoomGetOwner((uint)number, out name))
                        {
                            Console.WriteLine("Room " + number + "'s owner is " + name + ".");
                        }
                        else
                        {
                            Console.WriteLine("Room does not exist.");
                        }
                        break;
                    case "rservid":      //Get room's server ID
                        Console.Write("\nPlease enter room number: ");
                        if (!int.TryParse(Console.ReadLine(), out number))
                        {
                            Console.WriteLine("\t---Incorrect Input---");
                            break;
                        }
                        if (redis.RoomGetServerID((uint)number, out anotherNumber))
                        {
                            Console.WriteLine("Room " + number + "'s server is Server #" + anotherNumber + ".");
                        }
                        else
                        {
                            Console.WriteLine("Room does not exist.");
                        }
                        break;
                    case "rsizerank":   //Get room's size rank
                        Console.Write("\nPlease enter room number: ");
                        if (!int.TryParse(Console.ReadLine(), out number))
                        {
                            Console.WriteLine("\t---Incorrect Input---");
                            break;
                        }
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
                    case "rchgtitle":   //Change room title
                        if (dbEmulationMode == false)
                        {
                            messageToAdmin = "***Error: DB Moderator Mode must be activated before using command \'" + response + "\'***";
                            break;
                        }
                        Console.Write("\nPlease enter room number: ");
                        if (!int.TryParse(Console.ReadLine(), out number))
                        {
                            Console.WriteLine("\t---Incorrect Input---");
                            break;
                        }
                        Console.Write("Please enter a new room title: ");
                        response = Console.ReadLine();
                        numResult = redis.RoomChangeTitle((uint)number, response, out name);
                        //Prevent null reference errors upon Redis connection cut
                        if (name == null)
                        {
                            Console.WriteLine("Redis connection error!");
                            break;
                        }
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
                    case "rsetown":     //Change room owner
                        if (dbEmulationMode == false)
                        {
                            messageToAdmin = "***Error: DB Moderator Mode must be activated before using command \'" + response + "\'***";
                            break;
                        }
                        Console.Write("\nPlease enter room number: ");
                        if (!int.TryParse(Console.ReadLine(), out number))
                        {
                            Console.WriteLine("\t---Incorrect Input---");
                            break;
                        }
                        Console.Write("Please enter a new room owner name: ");
                        response = Console.ReadLine();
                        numResult = redis.RoomSetOwner((uint)number, response, out name);
                        //Prevent null reference errors upon Redis connection cut
                        if (name == null)
                        {
                            Console.WriteLine("Redis connection error!");
                            break;
                        }
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
                    case "rsetserv":     //Change room server ID
                        if (dbEmulationMode == false)
                        {
                            messageToAdmin = "***Error: DB Moderator Mode must be activated before using command \'" + response + "\'***";
                            break;
                        }
                        Console.Write("\nPlease enter room number: ");
                        if (!int.TryParse(Console.ReadLine(), out number))
                        {
                            Console.WriteLine("\t---Incorrect Input---");
                            break;
                        }
                        Console.Write("Please enter a new server ID: ");
                        if (!uint.TryParse(Console.ReadLine(), out anotherNumber))
                        {
                            Console.WriteLine("\t---Incorrect Input---");
                            break;
                        }
                        numResult = redis.RoomSetServerID((uint)number, anotherNumber, out time);
                        switch (numResult)
                        {
                            case -1:
                                Console.WriteLine("Room does not exist.");
                                break;
                            case 0:
                                Console.WriteLine("The new server ID is not different than the current server ID.");
                                break;
                            case 1:
                                Console.WriteLine("Room " + number + "'s server ID has been changed from " + time + " to " + anotherNumber + ".");
                                break;
                            default:
                                Console.WriteLine("Unknown Error.");
                                break;
                        }
                        break;
                    case "radduse":     //Add user to room
                        if (dbEmulationMode == false)
                        {
                            messageToAdmin = "***Error: DB Moderator Mode must be activated before using command \'" + response + "\'***";
                            break;
                        }
                        Console.Write("\nPlease enter room number: ");
                        if (!int.TryParse(Console.ReadLine(), out number))
                        {
                            Console.WriteLine("\t---Incorrect Input---");
                            break;
                        }
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
                    case "rremuse":     //Remove user from room
                        if (dbEmulationMode == false)
                        {
                            messageToAdmin = "***Error: DB Moderator Mode must be activated before using command \'" + response + "\'***";
                            break;
                        }
                        Console.Write("\nPlease enter room number: ");
                        if (!int.TryParse(Console.ReadLine(), out number))
                        {
                            Console.WriteLine("\t---Incorrect Input---");
                            break;
                        }
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
                                    //Prevent null reference errors upon Redis connection cut
                                    if (response == null)
                                    {
                                        Console.WriteLine("Redis connection error!");
                                        break;
                                    }
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
                    case "rpurge":      //Purge a room of users then delete the room
                        if (dbEmulationMode == false)
                        {
                            messageToAdmin = "***Error: DB Moderator Mode must be activated before using command \'" + response + "\'***";
                            break;
                        }
                        Console.Write("\nPlease enter room number: ");
                        if (!int.TryParse(Console.ReadLine(), out number))
                        {
                            Console.WriteLine("\t---Incorrect Input---");
                            break;
                        }
                        numResult = redis.RoomPurge((uint)number);
                        switch (numResult)
                        {
                            case -1:
                                Console.WriteLine("Room does not exist.");
                                break;
                            case 0:
                                Console.WriteLine("Room was found empty and couldn't be deleted.");
                                break;
                            case 1:
                                Console.WriteLine("Purge successful.");
                                break;
                            case 2:
                                Console.WriteLine("Room was found empty and was subsequently deleted.");
                                break;
                            default:
                                Console.WriteLine("Unknown Error.");
                                break;
                        }
                        break;
                    case "rulist":      //Get room's user list
                        Console.Write("\nPlease enter room number: ");
                        if (!int.TryParse(Console.ReadLine(), out number))
                        {
                            Console.WriteLine("\t---Incorrect Input---");
                            break;
                        }
                        nameList = redis.RoomUserList((uint)number);
                        if (nameList == null)
                        {
                            Console.WriteLine("Room does not exist.");
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
                    case "rlist":       //Get room list
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
                                    //Prevent null reference errors upon Redis connection cut
                                    if (name == null)
                                    {
                                        Console.WriteLine("Redis connection error!");
                                        break;
                                    }
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
                    case "rlistserv":       //Get room list by server ID
                        Console.Write("Please enter a new server ID: ");
                        if (!uint.TryParse(Console.ReadLine(), out anotherNumber))
                        {
                            Console.WriteLine("\t---Incorrect Input---");
                            break;
                        }
                        nameList = redis.RoomListByServerID(anotherNumber);
                        if (nameList == null)
                        {
                            Console.WriteLine("No server IDs exist.");
                        }
                        else
                        {
                            for (int i = 0; i < nameList.Length; i++)
                            {
                                Console.Write(nameList[i] + "\t");
                                if (redis.RoomGetTitle(uint.Parse(nameList[i].Substring(5)), out name))
                                {
                                    //Prevent null reference errors upon Redis connection cut
                                    if (name == null)
                                    {
                                        Console.WriteLine("Redis connection error!");
                                        break;
                                    }
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
                    case "rranklist":   //Get Room Ranking List
                        Console.Write("\nPlease enter the top-n list value (0 for full list): ");
                        if (!int.TryParse(Console.ReadLine(), out number))
                        {
                            Console.WriteLine("\t---Incorrect Input---");
                            break;
                        }
                        rankList = redis.RoomSizeRankList(--number);
                        //Prevent null reference errors upon Redis connection cut
                        if (rankList == null)
                        {
                            Console.WriteLine("Redis connection error!");
                            break;
                        }
                        int z = 0;
                        foreach (KeyValuePair<string, double> keyValuePair in rankList)
                        {
                            z++;
                            Console.WriteLine("Rank " + z + ":\t" + keyValuePair.Key + "\t\t" + keyValuePair.Value);
                        }
                        break;
                    case "loblist":     //Get lobby list
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
                    case "servrcount":  //Get a server's room count
                        Console.Write("\nPlease enter server ID number: ");
                        if (!int.TryParse(Console.ReadLine(), out number))
                        {
                            Console.WriteLine("\t---Incorrect Input---");
                            break;
                        }
                        numResult = redis.ServerGetRoomCount((uint)number);
                        if (numResult < 0)
                        {
                            Console.WriteLine("Room does not exist.");
                        }
                        else
                        {
                            Console.WriteLine("Server #" + number + " contains " + numResult + " room(s).");
                        }
                        break;
                    case "servrank":   //Get Server Room-Count Ranking List
                        Console.Write("\nPlease enter the top-n list value (0 for full list): ");
                        if (!int.TryParse(Console.ReadLine(), out number))
                        {
                            Console.WriteLine("\t---Incorrect Input---");
                            break;
                        }
                        rankList = redis.ServerRoomCountRanking(--number);
                        //Prevent null reference errors upon Redis connection cut
                        if (rankList == null)
                        {
                            Console.WriteLine("Redis connection error!");
                            break;
                        }
                        int w = 0;
                        foreach (KeyValuePair<string, double> keyValuePair in rankList)
                        {
                            w++;
                            Console.WriteLine("Rank " + w + ":\t" + keyValuePair.Key + "\t\t" + keyValuePair.Value);
                        }
                        break;
                    default:            //Incorrect command, print invalid entry
                        messageToAdmin = "\n\t\t---Invalid Entry---";
                        break;
                }
                
                //If there was a admin message, print it
                Console.WriteLine(messageToAdmin);

                //If the menu wasn't on, get the next command
                if (!menuOn)
                {
                    //Get next command
                    Console.Write("\nTIP: Enter 'menu' to return to the command list menu.");
                    Console.Write("\n> ");
                    response = Console.ReadLine();
                    Console.Clear();
                }
            }

            //Close with a kind message followed by connection closure
            Console.WriteLine("Thank you for using the Redis Test Client. Enter anything to exit.");
            Console.ReadLine();
            redis.CloseConnection();
        }

        //Method for running Live Monitoring Mode
        public static void MonitoringMode (RedisDBController redis)
        {
            //Loop Control variables
            bool isAdminFinished = false;
            int x = 0;
            int z = 0;
            int monitorTimer = 0;
            //Variables for holding DB data
            long nLoggedUsers = 0;
            long nDummyUsers = 0;
            Dictionary<string, double> userTop10Ranking = null;
            Dictionary<string, double> roomTop5Ranking = null;
            string[] roomTitles = new string[5];

            //Variables for key-input control
            StringBuilder input = new StringBuilder("");
            Thread keyInputReaderThread;
            KeyInputController keyInputControl = new KeyInputController(input);
            keyInputControl.StartReading(out keyInputReaderThread);

            isAdminFinished = false;
            //Loop until the admin wants to stop
            while (isAdminFinished == false)
            {
                //If the monitor time timed out, get information from the Redis database
                if (monitorTimer <= 0)
                {
                    //Get new info from DB
                    nLoggedUsers = redis.GetLoginPoolSize();
                    nDummyUsers = redis.GetDummyPoolSize();
                    userTop10Ranking = redis.GetSubTopList(9, "LoginPool");
                    roomTop5Ranking = redis.RoomSizeRankList(4);
                    //Prevent null reference errors upon Redis connection cut
                    if (userTop10Ranking == null)
                    {
                        Console.WriteLine("Redis connection error! (Press enter to continue)");
                        keyInputReaderThread.Abort();
                        Console.ReadLine();
                        break;
                    }
                    if (roomTop5Ranking == null)
                    {
                        Console.WriteLine("Redis connection error! (Press enter to continue)");
                        keyInputReaderThread.Abort();
                        Console.ReadLine();
                        break;
                    }
                    //Get room titles
                    x = 0;
                    foreach (KeyValuePair<string, double> room in roomTop5Ranking)
                    {
                        try
                        {
                            redis.RoomGetTitle(uint.Parse(room.Key.ToString().Substring(5)), out roomTitles[x++]);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("Exception during room title retrieval parsing: " + e.Message);
                        }
                    }
                    monitorTimer = 1000;            //Set monitor timer so we get an update every 1 sec
                }
                else
                {
                    monitorTimer -= 100;            //If the monitor timer didn't trigger, subtract 100 ms (based on sleep time later in the code)
                }
                
                //If a keystroke was made OR monitor timer cycle has been restarted (meaning a db update was archived), then redraw the screen
                if (keyInputControl.wasCharWritten == true || monitorTimer == 1000)
                {
                    //Reset the char-write trigger
                    keyInputControl.wasCharWritten = false;

                    //Draw screen
                    Console.Clear();
                    Console.WriteLine("==========================================================");
                    Console.WriteLine("================Welcome to the POCKETCHAT!!===============");
                    Console.WriteLine("===================Admin Client Portal====================");
                    Console.WriteLine("==========================================================");
                    Console.WriteLine();
                    Console.WriteLine("    Active real users = " + nLoggedUsers + "\tActive dummy users = " + nDummyUsers);
                    Console.WriteLine("\n                Active User Rankings");
                    z = 0;
                    foreach (KeyValuePair<string, double> keyValuePair in userTop10Ranking)
                    {
                        z++;
                        Console.Write("Rank " + z + ": \t" + keyValuePair.Key.Substring(5) + "\t");
                        if (keyValuePair.Key.Substring(5).Length < 8)
                        {
                            Console.Write("\t");
                        }
                        Console.Write((keyValuePair.Value - 1) + " message(s) sent.\n");
                    }
                    Console.WriteLine("\n                Active Room Rankings");
                    z = 0;
                    foreach (KeyValuePair<string, double> keyValuePair in roomTop5Ranking)
                    {
                        z++;
                        Console.Write("Rank " + z + ": \t" + keyValuePair.Key + "\t" + roomTitles[z - 1] + "\t");
                        if (roomTitles[z-1].Length < 6)
                        {
                            Console.Write("\t");
                        }
                        Console.Write(keyValuePair.Value + " user(s).\n");
                    }
                    Console.WriteLine("\n==========================================================");
                    Console.WriteLine("Enter 'stop' to terminate live monitoring.");
                    Console.Write("> " + input.ToString());
                }
                
                Thread.Sleep(100);  //Sleep the thread for 100 ms

                //When the stop command is triggered, kill the loop
                if(keyInputControl.wasStopCalled == true)
                {
                    isAdminFinished = true;
                }
            }
        }

        //Class for monitoring key-by-key input
        public class KeyInputController
        {
            //A handle to a string builder and public volatile booleans to be accessed by another thread
            public StringBuilder command;
            public volatile bool wasCharWritten;
            public volatile bool wasStopCalled;

            //Constructor that passes the reference for the string builder which will be added to
            public KeyInputController (StringBuilder input)
            {
                command = input;
            }

            //Initialization of a new thread for the connection
            public void StartReading(out Thread keyInputReaderThread)
            {
                //Create a new thread to handle the connection
                keyInputReaderThread = new Thread(BuildInput);
                //Start!
                keyInputReaderThread.Start();
            }

            //This Method loops and waits for each keystroke from the user
            //At each keystroke, the entry is stored in the string builder
            public void BuildInput ()
            {
                //Container for keystroke
                ConsoleKeyInfo holdKey = new ConsoleKeyInfo ();

                do
                {
                    command.Clear();                                    //When the command wasn't a 'stop', clear the request and continue collecting the new input
                    do
                    {
                        holdKey = Console.ReadKey();                    //Get the input keystroke
                        switch (holdKey.KeyChar)
                        {
                            case '\r':                                  //Do nothing when the return carriage was pressed
                                break;
                            case '\b':
                                if (command.Length > 0)                 //If there was a backspace, remove a character (unless the string builder is empty)
                                {
                                    command.Remove(command.Length - 1, 1);
                                }
                                break;
                            default:
                                command.Append(holdKey.KeyChar);        //Otherwise, add the keystroke to the string builder
                                break;
                        }
                        wasCharWritten = true;                          //Flag that a key has been stroked
                    } while (holdKey.KeyChar != '\r');                  //On a return key, stop the loop to check for a "stop" command
                } while (command.ToString().CompareTo("stop") != 0);    //Run this build input until a stop command is received
                //Signal that a stop command has been fired
                wasStopCalled = true;
            }
        }
    }
}
