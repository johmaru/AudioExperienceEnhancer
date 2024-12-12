using System.CommandLine;
using System.CommandLine.Invocation;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace AudioExperienceEnhancer;

public abstract class Program
{
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
    
    public static async Task<int> Main(string[] args)
    {
        
        // Create a root command with a single argument
        var rootCommand = new RootCommand()
        {
            new Argument<string>("firstArgument", "The first argument"),
        };
        
        rootCommand.Description = "A audio experience enhance tool";

        var testCommand = new Command("test", "Test the audio experience enhancer")
        {

        };
            
        testCommand.SetHandler(async () =>
        {
            await TestCommand();
        });
        
        var audioDeviceArgs = new Option<bool>("--device", "The audio device to use");
        var volumeArgs = new Option<bool>("--volume", "get the volume of the audio device");
        var sampleRateArgs = new Option<bool>("--samplerate", "The sample rate of the audio device");
        
        var getAudioDevicesCommand = new Command("gad", "Get the audio devices")
        {
            audioDeviceArgs,
            volumeArgs,
            sampleRateArgs,
        };
        
        getAudioDevicesCommand.SetHandler(async (device, volume,samplerate) =>
        {
            await GetAudioDevicesCommand(device, volume,samplerate);
        }
            ,audioDeviceArgs
            ,volumeArgs
            ,sampleRateArgs
            
            );
        
        // Add the commands to the root command
        rootCommand.AddCommand(testCommand);
        rootCommand.AddCommand(getAudioDevicesCommand);

        return rootCommand.InvokeAsync(args).Result;
    }
    
    private static Task TestCommand()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            Console.WriteLine("Windows");
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            Console.WriteLine("Linux");
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            Console.WriteLine("unsupported OS");
        }
      
        
        return Task.CompletedTask;
    }
    
    private static Task GetAudioDevicesCommand(
        bool device, 
        bool volume,
        bool sampleRate
        )
    {
       if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
       {
           
       }
       else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
       {
           if (device)
           {
               string[] lines = File.ReadAllLines("/proc/asound/cards");
           
               foreach (string line in lines)
               {
                   Console.WriteLine(line);
               }
           }
           else if (volume)
           {
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
           
           else if (sampleRate)
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
                    int dir;
                    err = snd_pcm_hw_params_get_rate(hwParams, out rate, out dir);
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
           
           else
           {
               Console.WriteLine("please input any argument");
           }
       }
       else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
       {
           Console.WriteLine("Unsupported OS");
       }
        
        
        
       return Task.CompletedTask;
    }
}