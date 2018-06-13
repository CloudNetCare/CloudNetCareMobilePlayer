using System;
using System.Collections.Generic;

namespace CloudNetCare.AppiumPilot
{
    public interface IDevicePilot : IDisposable
    {
        bool StartDevice(DeviceTarget deviceTarget, string packagePath, string appiumHost, int appiumPort, string aaptPath);
        CommandReturn ExecuteCommand(string command, string target, string value, string condition, int? timeout, ref Dictionary<string, string> persistantVariables, out string stepValue);
    }
}