using System.CommandLine;
using System.CommandLine.Invocation;
using System.Diagnostics;
using System.Runtime.InteropServices;
using AudioExperienceEnhancer.commands;

namespace AudioExperienceEnhancer;

public abstract class Program
{
    public static Task<int> Main(string[] args)
    {
        // Create a root command with a single argument
        var rootCommand = new RootCommand()
        {
           
        };
        
        rootCommand.Description = "A audio experience enhance tool";

        var testCommand = new Command("test", "Test the audio experience enhancer")
        {

        };
            
        testCommand.SetHandler(async () =>
        {
            await TestCommand();
        });
        
        var audioDeviceArgs = new Option<string>("device", "The audio device to use");
        var audioDeviceIntArgs = new Option<uint>("device_id", "The audio device to use");
        var getVolumeArgs = new Option<bool>("volume", "get the volume of the audio device");
        var muteArgs = new Option<bool>("mute", "Mute the audio device");
        var sampleRateArgs = new Option<bool>("samplerate", "The sample rate of the audio device");
        
        var getAudioDevicesCommand = new Command("gad", "Get the audio devices")
        {
            audioDeviceArgs,
            audioDeviceIntArgs,
            getVolumeArgs,
            sampleRateArgs,
        };
        
        getAudioDevicesCommand.SetHandler(async (device,deviceInt,getvolume,samplerate) =>
        {
               if (string.IsNullOrEmpty(device))
               {
                   
               }
               else
               {
                   await GetAudioDevicesCommand.snd_getDevice( device,deviceInt);
               }

               if (samplerate)
               {
                   await GetAudioDevicesCommand.snd_getSampleRate();
               }
        }
            ,audioDeviceArgs
            ,audioDeviceIntArgs
            ,getVolumeArgs
            ,sampleRateArgs
            
            );
        var setAudioDevicesCommand = new Command("sad", "Set the audio devices")
        {
            muteArgs,
        };
        
        setAudioDevicesCommand.SetHandler(async (mute) =>
        {
            if (mute)
            {
                await SetAudioDevicesCommand.snd_muteCommand();
            }
        }
            ,muteArgs
            );
        
        // Add the commands to the root command
        rootCommand.AddCommand(testCommand);
        rootCommand.AddCommand(getAudioDevicesCommand);
        rootCommand.AddCommand(setAudioDevicesCommand);

        return Task.FromResult(rootCommand.InvokeAsync(args).Result);
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
}