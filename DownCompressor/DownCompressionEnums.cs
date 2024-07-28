using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace PerformanceCEO.DownCompressor;

internal class DownCompressionEnums
{
    public enum DownscaleLevel
    {
        [Description("Full Quality")]
        Original,
        [Description("Downscale2X - Recommended")]
        Downscale2X,
        [Description("Downscale4X - Aggressive")]
        Downscale4X,
    }

    public enum DownscaleMethod
    {
        FastGPU,
        SlowSafe,
    }
}
