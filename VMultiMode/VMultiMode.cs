using System.Numerics;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.Plugin.Output;
using OpenTabletDriver.Plugin.Platform.Pointer;
using static VoiDPlugins.VMultiMode.VMulti;

namespace VoiDPlugins.VMultiMode
{
    [PluginName("VMulti Absolute Output Mode"), SupportedPlatform(PluginPlatform.Windows)]
    public class VMultiAbsMode : AbsoluteOutputMode
    {
        private readonly IVirtualTablet AbsHandler = new VMultiAbsHandler();
        public override IVirtualTablet VirtualTablet => AbsHandler;
    }

    [PluginName("VMulti Relative Output Mode"), SupportedPlatform(PluginPlatform.Windows)]
    public class VMultiRelMode : AbsoluteOutputMode
    {
        private readonly IVirtualTablet RelHandler = new VMultiRelHandler();
        public override IVirtualTablet VirtualTablet => RelHandler;
    }

    public class VMultiAbsHandler : VMultiHandler<VMultiAbsReport>, IVirtualTablet
    {
        private readonly float Width = Info.Driver.VirtualScreen.Width;
        private readonly float Height = Info.Driver.VirtualScreen.Height;

        public VMultiAbsHandler()
        {
            Init("VMultiAbs", 0x09);
        }

        public void SetPosition(Vector2 pos)
        {
            Report.X = (ushort)(pos.X / Width * 32767);
            Report.Y = (ushort)(pos.Y / Height * 32767);
            VMultiDev.Write(Report.ToBytes());
        }
    }

    public class VMultiRelHandler : VMultiHandler<VMultiRelReport>, IVirtualTablet
    {
        private ushort prevX, prevY;

        public VMultiRelHandler()
        {
            Init("VMultiRel", 0x04);
        }

        public void SetPosition(Vector2 pos)
        {
            var X = (ushort)pos.X;
            var Y = (ushort)pos.Y;
            Report.X = (byte)(X - prevX);
            Report.Y = (byte)(Y - prevY);
            prevX = X;
            prevY = Y;
            VMultiDev.Write(Report.ToBytes());
        }
    }
}