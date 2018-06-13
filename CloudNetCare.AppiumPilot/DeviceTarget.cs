using System;
using System.Collections.Generic;
using System.Text;

namespace CloudNetCare.AppiumPilot
{
    public enum DevicePlatform
    {
        IOs,
        Android,
        Other
    }

    /// <summary>
    /// Defines Target Device to play test on
    /// </summary>
    public sealed class DeviceTarget
    {
        public DevicePlatform Platform { get; set; }

        public bool IsRealDevice { get; set; }

        public string DeviceName { get; set; }

        public string VersionMajor { get; set; }

        public string VersionMinor { get; set; }

        public string VersionSubminor { get; set; }
    }
}
