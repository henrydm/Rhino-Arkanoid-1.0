using System;
using System.Runtime.InteropServices;

namespace NAudio.Wave.Compression
{
    /// <summary>
    /// Interop structure for ACM stream headers.
    /// ACMSTREAMHEADER 
    /// http://msdn.microsoft.com/en-us/library/dd742926%28VS.85%29.aspx
    /// </summary>    
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Size = 128)] // explicit size to make it work for x64
    class AcmStreamHeaderStruct
    {
        public int cbStruct;
        public AcmStreamHeaderStatusFlags fdwStatus = 0;
        public IntPtr userData;
        public IntPtr sourceBufferPointer;
        public int sourceBufferLength;
        public int sourceBufferLengthUsed;
        public IntPtr sourceUserData;
        public IntPtr destBufferPointer;
        public int destBufferLength;
        public int destBufferLengthUsed = 0;
        public IntPtr destUserData;

    }
}
