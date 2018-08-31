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
    class IosDriverBuilder
    {
        private string _packagePath;
        private Uri _appiumServerUri;
        private string _aaptExePath;
        private TimeSpan _commandTimeOut = TimeSpan.FromSeconds(300);
        private DeviceTarget _deviceTarget;

        public string InstalledPackageName { get; private set; }

        public AppiumDriver<AndroidElement> Build()
        {
            Console.WriteLine("Try to Start IOS_SIMULATOR");

            var platformVersion = GetPlatformVersion(_deviceTarget);

            Console.WriteLine($"AppiumPilot PlatformVersion:{platformVersion} DeviceName:{_deviceTarget.DeviceName}");

            var capabilities = new DesiredCapabilities();
            var dt = _deviceTarget.DeviceName.Split('_');
            var device = dt[3].Replace("-", " ");
            var versionMajor = dt[4];
            var versionMinor = dt[5];
            var versionSubMinor = "";
 

            capabilities.SetCapability(MobileCapabilityType.PlatformVersion, versionMajor + "." + versionMinor + (versionSubMinor != "" ? ("." + versionSubMinor) : ""));
            capabilities.SetCapability(MobileCapabilityType.FullReset, false);
            capabilities.SetCapability(MobileCapabilityType.DeviceName, device);
            capabilities.SetCapability("scaleFactor", (device.Contains("Plus") ? "0.33" : "0.5"));
            capabilities.SetCapability(MobileCapabilityType.AutomationName, "XCUITest");
            capabilities.SetCapability("autoAcceptAlerts ", true);

            var isBrowser = IsBrowser(_packagePath);
      

            capabilities.SetCapability(MobileCapabilityType.App, _packagePath);


            var dr = new AndroidDriver<AndroidElement>(_appiumServerUri, capabilities, _commandTimeOut);
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
