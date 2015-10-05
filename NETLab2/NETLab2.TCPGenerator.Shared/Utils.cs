using System;
using System.Runtime.InteropServices;

namespace NETLab2.TCPGenerator.Shared
{
    class Utils
    {
        public static Byte[] SerializeMessage<T>(T msg) where T : class
        {
            int objsize = Marshal.SizeOf(typeof(T));
            Byte[] ret = new Byte[objsize];
            IntPtr buff = Marshal.AllocHGlobal(objsize);
            Marshal.StructureToPtr(msg, buff, true);
            Marshal.Copy(buff, ret, 0, objsize);
            Marshal.FreeHGlobal(buff);
            return ret;
        }
    }
}
