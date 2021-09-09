using System;
using System.Runtime.InteropServices;

namespace NAudio.Wave.Compression
{
    /// <summary>
    /// Interop definitions for Windows ACM (Audio Compression Manager) API
    /// </summary>
    class AcmInterop
    {   
        [DllImport("Msacm32.dll")]
        public static extern MmResult acmDriverClose(IntPtr hAcmDriver, int closeFlags);
      
        [DllImport("Msacm32.dll")]
        public static extern MmResult acmDriverOpen(out IntPtr pAcmDriver, IntPtr hAcmDriverId, int openFlags);

        [DllImport("Msacm32.dll",EntryPoint="acmFormatSuggest")]
        public static extern MmResult acmFormatSuggest2(
            IntPtr hAcmDriver,
            IntPtr sourceFormatPointer,
            IntPtr destFormatPointer,
            int sizeDestFormat,
            AcmFormatSuggestFlags suggestFlags);


        /// <summary>
        /// A version with pointers for troubleshooting
        /// </summary>
        [DllImport("Msacm32.dll",EntryPoint="acmStreamOpen")]
        public static extern MmResult acmStreamOpen2(
            out IntPtr hAcmStream,
            IntPtr hAcmDriver,
            IntPtr sourceFormatPointer,
            IntPtr destFormatPointer,
            [In] WaveFilter waveFilter,
            IntPtr callback,
            IntPtr instance,
            AcmStreamOpenFlags openFlags);

        // http://msdn.microsoft.com/en-us/library/dd742923%28VS.85%29.aspx
        [DllImport("Msacm32.dll")]
        public static extern MmResult acmStreamClose(IntPtr hAcmStream, int closeFlags);

        // http://msdn.microsoft.com/en-us/library/dd742924%28VS.85%29.aspx
        [DllImport("Msacm32.dll")]
        public static extern MmResult acmStreamConvert(IntPtr hAcmStream, [In, Out] AcmStreamHeaderStruct streamHeader, AcmStreamConvertFlags streamConvertFlags);

        // http://msdn.microsoft.com/en-us/library/dd742929%28VS.85%29.aspx
        [DllImport("Msacm32.dll")]
        public static extern MmResult acmStreamPrepareHeader(IntPtr hAcmStream, [In, Out] AcmStreamHeaderStruct streamHeader, int prepareFlags);

        //// http://msdn.microsoft.com/en-us/library/dd742929%28VS.85%29.aspx
        //[DllImport("Msacm32.dll")]
        //public static extern MmResult acmStreamReset(IntPtr hAcmStream, int resetFlags);
        
        // http://msdn.microsoft.com/en-us/library/dd742931%28VS.85%29.aspx
        [DllImport("Msacm32.dll")]
        public static extern MmResult acmStreamSize(IntPtr hAcmStream, int inputBufferSize, out int outputBufferSize, AcmStreamSizeFlags flags);

        // http://msdn.microsoft.com/en-us/library/dd742932%28VS.85%29.aspx
        [DllImport("Msacm32.dll")]
        public static extern MmResult acmStreamUnprepareHeader(IntPtr hAcmStream, [In, Out] AcmStreamHeaderStruct streamHeader, int flags);
    }
}
