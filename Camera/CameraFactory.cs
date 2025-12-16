namespace AutomationHelper
{
    public static class CameraFactory
    {
        public static ICamera Create(HikDeviceInfo device)
        {
            return new HikCamera(device);
        }
    }
}
