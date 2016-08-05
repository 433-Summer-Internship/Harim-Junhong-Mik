using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Numbering
{
    /// <summary>
    /// This class contains a queue with all available room numbers. When a room is created, the next number is popped from the queue. 
    /// When a room is deleted, that room's number is pushed onto the queue. 
    /// The class can handle room IDs of byte type and the user must define the maximum value.
    /// </summary>
    public class RoomIDQueue
    {
        private static Queue<byte> availableIDs = new Queue<byte>();    //Queue of type byte for holding available roomIDs
        private static byte maximumValue;                               //Keep track of a maximum value for input range checking     

        /// <summary>
        /// The RoomIDQueue constructor specifies the desired maximum room ID value and generates a queue from 1 to maxValue.
        /// </summary>
        /// <param name="maxValue"></param>
        public RoomIDQueue(byte maxValue)
        {
            maximumValue = maxValue;                                    //Set maximumValue for safe keeping
            for (byte i = 1; i <= maxValue; i++)                        //Start from 1 and go until maxValue
            {
                availableIDs.Enqueue(i);                                //Add number i to the queue
            }
        }

        /// <summary>
        /// The GetRoom method retrives a room from the queue and returns the number. If there are no rooms available, the number 0 is returned.
        /// </summary>
        /// <returns></returns>
        public byte GetRoom()
        {
            if (availableIDs.Count > 0)                                 //Check if the ID count is greater than 0
            {
                byte roomNumber = availableIDs.Dequeue();               //If so, dequeue the next available room number and then return it
                return roomNumber;
            }
            return 0;                                                   //Otherwise return 0
        }

        /// <summary>
        /// The ReturnRoom method adds a room number back to the available room ID queue.
        /// The return values are as follows:
        ///     -1 - roomNumber exceeds the maximum room ID value
        ///     0 - roomNumber is already contained in the queue
        ///     1 - roomNumber successfully added into the queue
        /// </summary>
        /// <param name="roomNumber"></param>
        public int ReturnRoom(byte roomNumber)
        {
            if (roomNumber > maximumValue)
            {
                return -1;                                              //Return false if roomNumber exceeds the maximum value
            }
            if (availableIDs.Contains(roomNumber))
            {
                return 0;                                               //Return false if roomNumber is already contained in the queue
            }
            availableIDs.Enqueue(roomNumber);                           //Add the roomNumber back to the queue
            return 1;                                                   //Return true for success
        }

        public byte GetNumberOfAvailableRooms()
        {
            return (byte)availableIDs.Count;
        }

        /// <summary>
        /// The GetAvailableRoomNumbers method provides an byte array containing all values in the queue.
        /// </summary>
        /// <returns></returns>
        public byte[] GetAvailableRoomNumbers()
        {
            return availableIDs.ToArray();
        }
    }
}



