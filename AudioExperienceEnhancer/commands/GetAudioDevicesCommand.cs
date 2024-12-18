using System.Runtime.InteropServices;

namespace AudioExperienceEnhancer.commands;

public abstract class GetAudioDevicesCommand
{
    // Linux externs 
    private const string LibraryName = "libasound.so.2"; 
    private const int SND_PCM_STREAM_PLAYBACK = 0;
    private const int SND_PCM_STREAM_CAPTURE = 1;
    private const int SND_PCM_ASYNC = 2;

    const int SND_PCM_ACCESS_RW_INTERLEAVED = 3;
    const int SND_PCM_FORMAT_S16_LE = 2;

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    private static extern int snd_pcm_open(out IntPtr pcm, string name, int stream, int mode);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    private static extern int snd_pcm_close(IntPtr pcm);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    private static extern int snd_pcm_hw_params_malloc(out IntPtr ptr);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    private static extern int snd_pcm_hw_params_any(IntPtr pcm, IntPtr ptr);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    private static extern int snd_pcm_hw_params_set_rate(IntPtr pcm, IntPtr @params, uint val, int dir);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    private static extern int snd_pcm_hw_params_get_rate(IntPtr @params, out uint val, out int dir);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    private static extern int snd_pcm_hw_params_free(IntPtr ptr);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    private static extern int snd_pcm_hw_params_get_rate_min(IntPtr @params, out uint val, out int dir);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    private static extern int snd_pcm_hw_params_current(IntPtr pcm, IntPtr @params);
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    private static extern int snd_pcm_hw_params_set_access(IntPtr pcm, IntPtr @params, int access);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    private static extern int snd_pcm_hw_params_set_format(IntPtr pcm, IntPtr @params, int format);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    private static extern int snd_pcm_hw_params(IntPtr pcm, IntPtr @params);
    
    // windows externs
    
    [DllImport( "winmm.dll", SetLastError = true )]
    private static extern uint waveOutGetDevCaps(uint uDeviceID, ref WAVEOUTCAPS pwoc, uint cbwoc);
    [DllImport("winmm.dll", SetLastError = true)]
    private static extern uint waveOutOpen(out IntPtr phwo, uint uDeviceID, ref WAVEFORMATEX pwfx, IntPtr dwCallback, IntPtr dwInstance, uint fdwOpen);

    [DllImport("winmm.dll", SetLastError = true)]
    private static extern uint waveOutClose(IntPtr hwo);
    
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    private struct WAVEOUTCAPS
    {
        public ushort wMid;
        public ushort wPid;
        public uint vDriverVersion;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string szPname;
        public uint dwFormats;
        public ushort wChannels;
        public ushort wReserved1;
        public uint dwSupport;
    }
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    private struct WAVEFORMATEX
    {
        public ushort wFormatTag;
        public ushort nChannels;
        public uint nSamplesPerSec;
        public uint nAvgBytesPerSec;
        public ushort nBlockAlign;
        public ushort wBitsPerSample;
        public ushort cbSize;
    }
    
    
    [DllImport("winmm.dll", SetLastError = true)]
    private static extern uint waveOutGetNumDevs();
    [DllImport("winmm.dll", SetLastError = true)]
    private static extern uint waveOutGetVolume(IntPtr hwo, ref uint pdwVolume);
    [DllImport("winmm.dll", SetLastError = true)]
    private static extern uint waveOutSetVolume(IntPtr hwo, uint dwVolume);
    
    
    [DllImport("ole32.dll")]
    private static extern int CoInitializeEx(IntPtr pvReserved, uint dwCoInit);
    
    [DllImport("ole32.dll")]
    private static extern void CoUninitialize();
    
    [ComImport]
    [Guid("BCDE0395-E52F-467C-8E3D-C4579291692E")]
    private class MMDeviceEnumerator
    {
    }

    [ComImport]
    [Guid("A95664D2-9614-4F35-A746-DE8DB63617E6")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IMMDeviceEnumerator
    {
        int NotImpl1(); 

        [PreserveSig]
        int GetDefaultAudioEndpoint(EDataFlow dataFlow, ERole role, out IMMDevice ppDevice);
    }

    [ComImport]
    [Guid("D666063F-1587-4E43-81F1-B948E807363F")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IMMDevice
    {
        [PreserveSig]
        int Activate(ref Guid iid, uint dwClsCtx, IntPtr pActivationParams, [MarshalAs(UnmanagedType.IUnknown)] out object ppInterface);
    }
    [ComImport]
    [Guid("94BE9D30-53AC-4802-829C-F13E5AD34775")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IAudioClient3
    {
       
        new int GetMixFormat(out IntPtr ppDeviceFormat);
        
        [PreserveSig]
        int GetCurrentSharedModeEnginePeriod(
            out IntPtr ppFormat,           
            out uint pCurrentPeriodInFrames 
        );

        [PreserveSig]
        int GetSharedModeEnginePeriod(
            IntPtr pFormat,             
            out uint pDefaultPeriodInFrames, 
            out uint pFundamentalPeriodInFrames, 
            out uint pMinPeriodInFrames,   
            out uint pMaxPeriodInFrames     
        );
    }
    
    [ComImport]
    [Guid("1CB9AD4C-DBFA-4c32-B178-C2F568A703B2")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IAudioClient
    {
        [PreserveSig]
        int Initialize(
            int shareMode,
            uint streamFlags,
            long hnsBufferDuration,
            long hnsPeriodicity,
            IntPtr pFormat,
            ref Guid audioSessionGuid
        );

        [PreserveSig]
        int GetBufferSize(out uint bufferFrameCount);

        [PreserveSig]
        int GetStreamLatency(out long hnsLatency);

        [PreserveSig]
        int GetCurrentPadding(out uint numPaddingFrames);

        [PreserveSig]
        int IsFormatSupported(
            int shareMode,
            IntPtr pFormat,
            out IntPtr ppClosestMatch
        );

        [PreserveSig]
        int GetMixFormat(out IntPtr ppDeviceFormat);

        [PreserveSig]
        int GetDevicePeriod(
            out long phnsDefaultDevicePeriod,
            out long phnsMinimumDevicePeriod
        );

        [PreserveSig]
        int Start();

        [PreserveSig]
        int Stop();

        [PreserveSig]
        int Reset();

        [PreserveSig]
        int SetEventHandle(IntPtr eventHandle);

        [PreserveSig]
        int GetService(ref Guid interfaceId, [MarshalAs(UnmanagedType.IUnknown)] out object ppv);
    }
    
    private enum EDataFlow
    {
        eRender,   
        eCapture,  
        eAll       
    }

    private enum ERole
    {
        eConsole,
        eMultimedia,
        eCommunications
    }

    private enum CLSCTX
    {
        CLSCTX_INPROC_SERVER = 1,
        CLSCTX_ALL = CLSCTX_INPROC_SERVER
    }
    
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct WAVEFORMATEXTENSIBLE
    {
        public ushort wFormatTag;
        public ushort nChannels;
        public uint nSamplesPerSec;
        public uint nAvgBytesPerSec;
        public ushort nBlockAlign;
        public ushort wBitsPerSample;
        public ushort cbSize;
        public ushort wValidBitsPerSample;
        public uint dwChannelMask;
        public Guid SubFormat;
    }
     public static Task snd_getDevice(
        string device,
        uint deviceInt
        )
    {
       if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
       {
          
           if (deviceInt.CompareTo(0) == 0)
           {
               WAVEOUTCAPS caps = new WAVEOUTCAPS();
               if (waveOutGetDevCaps(deviceInt, ref caps, (uint)Marshal.SizeOf(typeof(WAVEOUTCAPS))) == 0)
               {
                   Console.WriteLine($"Device {deviceInt}: {caps.szPname}");
                   Console.WriteLine($"Channels: {caps.wChannels}");
                   Console.WriteLine($"Formats: {caps.dwFormats}");
                   Console.WriteLine($"Support: {caps.dwSupport}");
                   Console.WriteLine($"Driver Version: {caps.vDriverVersion}");
                   Console.WriteLine($"Manufacturer ID: {caps.wMid}");
                   Console.WriteLine($"Product ID: {caps.wPid}");
                   Console.WriteLine($"Reserved: {caps.wReserved1}");
               }

               return Task.CompletedTask;
           }
           
           
           switch (device)
           {
               case "all":
               case "":
               {
                   uint devices = waveOutGetNumDevs();
                   for (uint i = 0; i < devices; i++)
                   {
                      WAVEOUTCAPS caps = new WAVEOUTCAPS();
                       if (waveOutGetDevCaps(i, ref caps, (uint)Marshal.SizeOf(typeof(WAVEOUTCAPS))) == 0)
                       {
                           Console.WriteLine($"Device {i}: {caps.szPname}");
                       }
                   }

                   break;
               }
               default:
               {
                     Console.WriteLine("Please input a valid argument");

                   break;
               }
           }

       }
       else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
       {
           switch (device)
           {
               case "all":
               case "":
               {
                   string[] lines = File.ReadAllLines("/proc/asound/cards");
           
                   foreach (string line in lines)
                   {
                       Console.WriteLine(line);
                   }
               }
                   break;
               
               default:
               {
                   Console.WriteLine("Please input a valid argument");
                   break;
               }
           }
       }
       else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
       {
           Console.WriteLine("Unsupported OS");
       }
        
        
        
       return Task.CompletedTask;
    }

    public static Task snd_getSampleRate()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            const uint COINIT_MULTITHREADED = 0x0;
            int hr = CoInitializeEx(IntPtr.Zero, COINIT_MULTITHREADED);
            if (hr != 0 && hr != 1) 
            {
                Marshal.ThrowExceptionForHR(hr);
            }

            try
            {
                var deviceEnumerator = (IMMDeviceEnumerator)(new MMDeviceEnumerator());
                
                IMMDevice defaultDevice;
                hr = deviceEnumerator.GetDefaultAudioEndpoint(EDataFlow.eRender, ERole.eMultimedia, out defaultDevice);
                if (hr != 0) throw new COMException("デフォルトオーディオエンドポイントの取得に失敗", hr);

                object audioClientObj;
                Guid IID_IAudioClient = typeof(IAudioClient).GUID;
                hr = defaultDevice.Activate(ref IID_IAudioClient, (uint)CLSCTX.CLSCTX_ALL, IntPtr.Zero, out audioClientObj);
                if (hr != 0) throw new COMException("IAudioClientの取得に失敗", hr);

                var audioClient = (IAudioClient)audioClientObj;
                IntPtr ptr;
                hr = audioClient.GetMixFormat(out ptr);
                if (hr != 0) throw new COMException("フォーマットの取得に失敗", hr);

                var format = Marshal.PtrToStructure<WAVEFORMATEXTENSIBLE>(ptr);
                Console.WriteLine($"Sample Rate: {format.nSamplesPerSec}");

                Marshal.FreeCoTaskMem(ptr);
                Marshal.ReleaseComObject(audioClient);
                Marshal.ReleaseComObject(defaultDevice);
                Marshal.ReleaseComObject(deviceEnumerator);
            }
            finally
            {
                CoUninitialize();
            }
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
             IntPtr handle;
             int err = snd_pcm_open(out handle, "default",SND_PCM_STREAM_PLAYBACK, 0);
               
             if (err < 0)
             {
                 Console.WriteLine("Error opening the audio device {0}", err);
                 return Task.CompletedTask;
             }

             try
             {
                 IntPtr hwParams;
                 err = snd_pcm_hw_params_malloc(out hwParams);
                 if (err < 0)
                 {
                     Console.WriteLine($"Error allocating hw params: {err}");
                     snd_pcm_close(handle);
                     return Task.CompletedTask;
                 }

                 err = snd_pcm_hw_params_any(handle, hwParams);
                 if (err < 0)
                 {
                     Console.WriteLine($"Error initializing hw params: {err}");
                     snd_pcm_hw_params_free(hwParams);
                     snd_pcm_close(handle);
                     return Task.CompletedTask;
                 }

                 err = snd_pcm_hw_params_set_access(handle, hwParams, SND_PCM_ACCESS_RW_INTERLEAVED);
                 if (err < 0)
                 {
                     Console.WriteLine($"Error setting access: {err}");
                     snd_pcm_hw_params_free(hwParams);
                     snd_pcm_close(handle);
                     return Task.CompletedTask;
                 }

                 err = snd_pcm_hw_params_set_format(handle, hwParams, SND_PCM_FORMAT_S16_LE);
                 if (err < 0)
                 {
                     Console.WriteLine($"Error setting format: {err}");
                     snd_pcm_hw_params_free(hwParams);
                     snd_pcm_close(handle);
                     return Task.CompletedTask;
                 }

                  
                 err = snd_pcm_hw_params(handle, hwParams);
                 if (err < 0)
                 {
                     Console.WriteLine($"Error setting hw params: {err}");
                     snd_pcm_hw_params_free(hwParams);
                     snd_pcm_close(handle);
                     return Task.CompletedTask;
                 }

                 uint rate;
                 err = snd_pcm_hw_params_get_rate(hwParams, out rate, out _);
                 if (err < 0)
                 {
                     Console.WriteLine($"Error getting rate: {err}");
                 }
                 else
                 {
                     Console.WriteLine($"Sample rate is {rate} Hz");
                 }

                 snd_pcm_hw_params_free(hwParams);
             }
             catch (Exception e)
             {
                 Console.WriteLine(e);
                 throw;
             }
             finally
             {
                 snd_pcm_close(handle);
             }
        }
            
        return Task.CompletedTask;
    }
}