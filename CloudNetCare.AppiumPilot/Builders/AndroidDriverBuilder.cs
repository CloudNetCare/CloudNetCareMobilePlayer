using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Android;
using OpenQA.Selenium.Appium.Enums;
using OpenQA.Selenium.Remote;

namespace CloudNetCare.AppiumPilot.Builders
{
    internal class AndroidDriverBuilder
    {
        private string _packagePath;
        private Uri _appiumServerUri;
        private string _aaptExePath;
        private TimeSpan _commandTimeOut = TimeSpan.FromSeconds(300);
        private DeviceTarget _deviceTarget;

        public string InstalledPackageName { get; private set; }

        public AppiumDriver<AndroidElement> Build()
        {
            // install AndroidStudio https://developer.android.com/studio/index.html (Save path Android SDK)
            // Create ANDROID_HOME environment variable to be your Android SDK
            // install JAVA JDK http://www.oracle.com/technetwork/java/javase/downloads/jdk8-downloads-2133151.html
            // install node.js https://nodejs.org/en/ (V6.x LTS)
            // install appium :
            //  - launch node.js command prompt and type : npm install -g appium
            //  - try to launch appium :  node.js command prompt and type : appium &
            // Configure virtual devices with Android Studio

            Console.WriteLine("Try to Start ANDROID_SIMULATOR");

            var platformVersion = GetPlatformVersion(_deviceTarget);

            Console.WriteLine($"AppiumPilot PlatformVersion:{platformVersion} DeviceName:{_deviceTarget.DeviceName}");

            var capabilities = new DesiredCapabilities();
            capabilities.SetCapability(MobileCapabilityType.NewCommandTimeout, _commandTimeOut.Seconds);

            capabilities.SetCapability(MobileCapabilityType.PlatformVersion, platformVersion);
            capabilities.SetCapability(MobileCapabilityType.FullReset, true);
            capabilities.SetCapability(MobileCapabilityType.DeviceName, _deviceTarget.DeviceName);

            var isBrowser = IsBrowser(_packagePath);
            if (isBrowser)
                capabilities.SetCapability(MobileCapabilityType.BrowserName, "Chrome");
            else
            {
                InstalledPackageName = GetAndroidPackageName(_packagePath, _aaptExePath);
                if (String.IsNullOrEmpty(InstalledPackageName)) Console.WriteLine("Could not find Android Package Name");
                capabilities.SetCapability(MobileCapabilityType.App, _packagePath);
            }

            var dr = new AndroidDriver<AndroidElement>(_appiumServerUri, capabilities, _commandTimeOut);
            return dr;
        }


        public AndroidDriverBuilder PackagePath(string packagePath)
        {
            _packagePath = packagePath;
            return this;
        }

        public AndroidDriverBuilder AppiumUri(Uri appiumUri)
        {
            _appiumServerUri = appiumUri;
            return this;
        }

        public AndroidDriverBuilder AaptExePath(string aaptPath)
        {
            _aaptExePath = aaptPath;
            return this;
        }

        public AndroidDriverBuilder CommandTimeout(TimeSpan timeout)
        {
            _commandTimeOut = timeout;
            return this;
        }

        public AndroidDriverBuilder DeviceTarget(DeviceTarget deviceTarget)
        {
            _deviceTarget = deviceTarget;
            return this;
        }

        private static string GetPlatformVersion(DeviceTarget deviceTarget)
        {
            return deviceTarget.VersionMajor + "." + deviceTarget.VersionMinor + (deviceTarget.VersionSubminor != "" ? "." + deviceTarget.VersionSubminor : "");
        }

        private static bool IsBrowser(string packagePath)
        {
            return !(packagePath.EndsWith(".app.zip") || packagePath.EndsWith(".ipa") || packagePath.EndsWith(".apk"));
        }

        /// <summary>
        /// aapt.exe is used to Decrypt Package Name from apk based on http://stackoverflow.com/questions/6289149/read-the-package-name-of-an-android-apk
        /// </summary>
        /// <param name="packagePath"></param>
        /// <param name="aaptPath"></param>
        /// <returns></returns>
        private static string GetAndroidPackageName(string packagePath, string aaptPath)
        {
            string returnValue = "";

            try
            {
                if (!packagePath.EndsWith(".apk")) return "";

                var localPackageFile = packagePath;

                Process p = new Process();
                StreamWriter sw;
                StreamReader sr;
                StreamReader err;
                string response = "";
                ProcessStartInfo psI = new ProcessStartInfo("cmd")
                {
                    UseShellExecute = false,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };
                p.StartInfo = psI;
                p.Start();
                sw = p.StandardInput;
                sr = p.StandardOutput;
                err = p.StandardError;
                sw.AutoFlush = true;

                if (String.IsNullOrEmpty(aaptPath)) return "";
                var cmd = aaptPath + " dump badging \"" + localPackageFile + "\"";
                sw.WriteLine(cmd);
                sw.Close();
                response = sr.ReadToEnd();
                sr.Close();

                p.Close();

                if (!String.IsNullOrEmpty(response))
                {
                    if (response.Contains("package: name='"))
                    {
                        Console.WriteLine("Parse dump to find packageName");

                        var a = response.IndexOf("package: name='");
                        var sub1 = response.Substring(a, response.Length - a);
                        var b = sub1.IndexOf("'");
                        var sub2 = sub1.Substring(b + 1, sub1.Length - (b + 1));
                        var c = sub2.IndexOf("'");
                        returnValue = sub2.Substring(0, c);
                        Console.WriteLine("Android packageName : " + returnValue);
                    }
                }
            }
            catch (Exception e)
            {
            }
            return returnValue;
        }
    }
}
