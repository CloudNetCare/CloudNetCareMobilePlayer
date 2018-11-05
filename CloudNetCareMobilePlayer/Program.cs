using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using CloudNetCare.AppiumPilot;
using CloudNetCare.SeleniumWrapper;
using Microsoft.Extensions.Configuration;

namespace CloudNetCare.MobilePlayer
{
    class Program
    {
        private static IConfiguration Configuration;

        /// <summary>
        /// 
        /// Play a scenario on a local device (or emulator)
        /// 
        /// Prerequisite:
        /// 
        /// ANDROID:
        /// 
        ///     Intalled:
        ///         - NodeJS & appium
        ///         - Android: Android Studio with emulator
        /// 
        /// TO DO iOS:
        /// 
        ///     Intalled:
        ///         - NodeJS & appium
        ///         - iOS: XCode with simulator
        /// 
        /// 
        /// Start android emulator: cmd /k "emulator.exe -netdelay none -netspeed full -avd GALAXYS7API24"
        /// Start appium 
        /// 
        /// 
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables();

            Configuration = builder.Build();

            var dataDir = Configuration.GetValue<string>("DataDir");

            var scriptPath = Path.Combine(dataDir, Configuration.GetValue<string>("ScriptFile"));
            var packagePath = Path.Combine(dataDir, Configuration.GetValue<string>("PackageFile"));

            var buildToolsVersion = Configuration.GetValue<string>("ANDROID_BUILD_TOOLS_VERSION");
            var aaptPath = Path.Combine(Configuration.GetValue<string>("ANDROID_HOME"), "build-tools", $"{buildToolsVersion}","aapt.exe");

            CheckFileExists(scriptPath);
            CheckFileExists(packagePath);
            CheckFileExists(aaptPath);

            var appiumServerHost = Configuration.GetValue<string>("AppiumHost");
            var appiumPort = Configuration.GetValue<int>("AppiumPort", 4723);

            var deviceTarget = new DeviceTarget();
            Configuration.Bind("DeviceTarget", deviceTarget);

            var stepList = ScriptParser.GetStepListFromScript(scriptPath);

            PlayOnDevice(stepList, deviceTarget, packagePath, appiumServerHost, appiumPort, aaptPath);
        }

        private static void CheckFileExists(string file)
        {
            if (!File.Exists(file))
            {
                throw new FileNotFoundException($"{file}");
            }
        }

        private static void PlayOnDevice(IEnumerable<LocalStep> stepList, DeviceTarget deviceTarget, string packagePath, string appiumServerIp, int appiumPort, string aaptPath)
        {
            using (var devicePilot = new DevicePilot())
            {
                Dictionary<string, string> persistantVariables = new Dictionary<string, string>();

                var isOk = devicePilot.StartDevice(deviceTarget, packagePath, appiumServerIp, appiumPort, aaptPath);
                if (!isOk)
                {
                    Console.WriteLine("Error opening device !");
                    Console.ReadKey();
                    return;
                }

                foreach (LocalStep step in stepList)
                {
                    Console.WriteLine($"Play STEP: command:{step.Command} target:{step.Target} value:{step.Value}");
                    string stepValue;
                    var ret = devicePilot.ExecuteCommand(step.Command, step.Target, step.Value, step.Condition, step.TimedOutValue, ref persistantVariables, out stepValue);
                    if (!ret.isOk)
                    {
                        Console.WriteLine($"Error! Message: {ret.message} Result Value:{ret.resultValue}");
                        devicePilot.Dispose();
                        Console.ReadKey();
                        return;
                    }
                    Console.WriteLine("OK! " + ret.message);
                    Thread.Sleep(step.TimeBetweenSteps ?? 5 * 1000);
                }
            }
            Console.WriteLine("Finished without any error !");
            Console.ReadKey();
        }
    }
}
