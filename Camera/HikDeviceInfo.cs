namespace AutomationHelper
{
    public sealed class HikDeviceInfo
    {
        internal MvCamCtrl.NET.MyCamera.MV_CC_DEVICE_INFO Raw;

        public string Ip { get; internal set; }
        public string Model { get; internal set; }
        public string SerialNumber { get; internal set; }
    }
}
