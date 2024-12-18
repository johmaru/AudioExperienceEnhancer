using System.Diagnostics;
using System.Runtime.InteropServices;
using AudioExperienceEnhancer;

namespace AudioExperienceEnhancer.commands;

public abstract class SetAudioDevicesCommand
{
    [DllImport( "winmm.dll", SetLastError = true )]
    private static extern uint waveOutGetDevCaps(uint uDeviceID, ref WAVEOUTCAPS pwoc, uint cbwoc);
    
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
    
    [DllImport("winmm.dll", SetLastError = true)]
    private static extern uint waveOutGetNumDevs();
    [DllImport("winmm.dll", SetLastError = true)]
    private static extern uint waveOutOpen(out IntPtr phwo, uint uDeviceID, IntPtr pwfx, IntPtr dwCallback, IntPtr dwInstance, uint fdwOpen);
    [DllImport("winmm.dll", SetLastError = true)]
    private static extern uint waveOutClose(IntPtr hwo);
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
        int Activate(ref Guid iid, CLSCTX dwClsCtx, IntPtr pActivationParams, out IAudioEndpointVolume ppInterface);
    }

    [ComImport]
    [Guid("5CDF2C82-841E-4546-9722-0CF74078229A")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IAudioEndpointVolume
    {
        [PreserveSig]
        int RegisterControlChangeNotify(IntPtr pNotify);

        [PreserveSig]
        int UnregisterControlChangeNotify(IntPtr pNotify);

        [PreserveSig]
        int GetChannelCount(out uint pnChannelCount);

        [PreserveSig]
        int SetMasterVolumeLevel(float fLevelDB, ref Guid pguidEventContext);

        [PreserveSig]
        int SetMasterVolumeLevelScalar(float fLevel, ref Guid pguidEventContext);

        [PreserveSig]
        int GetMasterVolumeLevel(out float pfLevelDB);

        [PreserveSig]
        int GetMasterVolumeLevelScalar(out float pfLevel);

        [PreserveSig]
        int SetChannelVolumeLevel(uint nChannel, float fLevelDB, ref Guid pguidEventContext);

        [PreserveSig]
        int SetChannelVolumeLevelScalar(uint nChannel, float fLevel, ref Guid pguidEventContext);

        [PreserveSig]
        int GetChannelVolumeLevel(uint nChannel, out float pfLevelDB);

        [PreserveSig]
        int GetChannelVolumeLevelScalar(uint nChannel, out float pfLevel);

        [PreserveSig]
        int SetMute([MarshalAs(UnmanagedType.Bool)] bool bMute, ref Guid pguidEventContext);

        [PreserveSig]
        int GetMute(out bool pbMute);
        
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
    
    
    public static Task snd_muteCommand(
        
    )
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
                
                hr = deviceEnumerator.GetDefaultAudioEndpoint(EDataFlow.eRender, ERole.eMultimedia, out var device);
                if (hr != 0)
                {
                    throw new COMException("デフォルトオーディオエンドポイントの取得に失敗しました。", hr);
                }
                
                var IID_IAudioEndpointVolume = typeof(IAudioEndpointVolume).GUID;
                hr = device.Activate(ref IID_IAudioEndpointVolume, CLSCTX.CLSCTX_ALL, IntPtr.Zero, out var endpointVolume);
                if (hr != 0)
                {
                    throw new COMException("IAudioEndpointVolumeの取得に失敗しました。", hr);
                }
                
                hr = endpointVolume.GetMute(out bool isMuted);
                if (hr != 0)
                {
                    throw new COMException("ミュート状態の取得に失敗しました。", hr);
                }

                Guid pguidEventContext = Guid.Empty;
                hr = endpointVolume.SetMute(!isMuted, ref pguidEventContext);
                if (hr != 0)
                {
                    throw new COMException("ミュート状態の設定に失敗しました。", hr);
                }

                Console.WriteLine(!isMuted ? "ミュートしました" : "ミュート解除しました");
            }
            finally
            {
                CoUninitialize();
            }
        }

        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            // did not work this code feature will be fixing
          
            ProcessStartInfo psi = new ProcessStartInfo()
            {
                FileName = "amixer",
                Arguments = "get Master",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
               
            using (Process? process = Process.Start(psi))
            {
                using (StreamReader? reader = process?.StandardOutput)
                {
                    string? result = reader?.ReadToEnd();
                    Console.WriteLine(result);
                }
            }
        }
        return Task.CompletedTask;
    }
}