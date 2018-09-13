using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Android;
using OpenQA.Selenium.Appium.iOS;
using OpenQA.Selenium.Appium.Enums;
using OpenQA.Selenium.Remote;
using System.Linq;

namespace CloudNetCare.AppiumPilot.Builders
{
    class IosDriverBuilder
    {
        private string _packagePath;
        private Uri _appiumServerUri;
        private string _aaptExePath;
        private TimeSpan _commandTimeOut = TimeSpan.FromSeconds(300);
        private DeviceTarget _deviceTarget;

        public string InstalledPackageName { get; private set; }

        public AppiumDriver<IOSElement> Build()
        {
            Console.WriteLine("Try to Start IOS_SIMULATOR");
            
            var platformVersion = GetPlatformVersion(_deviceTarget);

            Console.WriteLine($"AppiumPilot PlatformVersion:{platformVersion} DeviceName:{_deviceTarget.DeviceName}");       
            Console.WriteLine("Device target : " + _deviceTarget.DeviceName);
            Console.WriteLine("deviceTarget.VersionMajor : " + _deviceTarget.VersionMajor);
            Console.WriteLine("_deviceTarget.VersionMinor : " + _deviceTarget.VersionMinor);
            Console.WriteLine("VersionSubminor : " + _deviceTarget.VersionSubminor);

            var capabilities = new DesiredCapabilities();
            capabilities.SetCapability(MobileCapabilityType.NewCommandTimeout, 300);
            capabilities.SetCapability(MobileCapabilityType.PlatformVersion, _deviceTarget.VersionMajor + "." + _deviceTarget.VersionMinor + (_deviceTarget.VersionSubminor != "" ? ("." + _deviceTarget.VersionSubminor) : ""));
            Console.WriteLine("versions :OK ");
            capabilities.SetCapability(MobileCapabilityType.FullReset, false);
            capabilities.SetCapability(MobileCapabilityType.DeviceName, _deviceTarget.DeviceName);
            Console.WriteLine("Device name OK" );
            capabilities.SetCapability("scaleFactor", (_deviceTarget.DeviceName.Contains("Plus") ? "0.33" : "0.5"));
            Console.WriteLine("scaleOK");
            capabilities.SetCapability(MobileCapabilityType.AutomationName, "XCUITest");
            Console.WriteLine("automation OK");
            capabilities.SetCapability("autoAcceptAlerts ", true);
            capabilities.SetCapability(MobileCapabilityType.App, _packagePath);
            Console.WriteLine("capabilities OK");

            Console.WriteLine($"Start iOS emulator");
            var dr = new IOSDriver<IOSElement>(_appiumServerUri, capabilities, _commandTimeOut);
            dr.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);

            return dr;
        }

        private string GetIosPackageName(string packagePath, string aaptExePath)
        {

            return null;
        }

        public IosDriverBuilder PackagePath(string packagePath)
        {
            _packagePath = packagePath;
            return this;
        }

        public IosDriverBuilder AppiumUri(Uri appiumUri)
        {
            _appiumServerUri = appiumUri;
            return this;
        }

        public IosDriverBuilder AaptExePath(string aaptPath)
        {
            _aaptExePath = aaptPath;
            return this;
        }

        public IosDriverBuilder CommandTimeout(TimeSpan timeout)
        {
            _commandTimeOut = timeout;
            return this;
        }

        public IosDriverBuilder DeviceTarget(DeviceTarget deviceTarget)
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
    }
}
