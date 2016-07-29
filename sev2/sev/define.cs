using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace sev
{
    class define
    {
        [Serializable]
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct ChatProtocol
        {
            [MarshalAs(UnmanagedType.U8)]
            public byte command;    //256 possible commands  

            [MarshalAs(UnmanagedType.U2)]
            public ushort valueA;     //unsigned short custom-value  

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
            public byte[] valueB;     //Variable sized value  
        }
    }
}
