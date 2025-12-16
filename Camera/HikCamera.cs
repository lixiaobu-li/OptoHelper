using MvCamCtrl.NET;

namespace AutomationHelper
{
    internal sealed class HikCamera : ICamera
    {
        private readonly HikDeviceInfo _device;
        private MyCamera _camera;
        private bool _connected;

        public string Name => _device.Model;
        public bool IsConnected => _connected;

        public HikCamera(HikDeviceInfo device)
        {
            _device = device;
            HikSdk.EnsureInit();
        }

        public bool Connect()
        {
            _camera = new MyCamera();

            Check(_camera.MV_CC_CreateDevice_NET(ref _device.Raw),
                "CreateDevice 失败");
            Check(_camera.MV_CC_OpenDevice_NET(),
                "OpenDevice 失败");
            Check(_camera.MV_CC_StartGrabbing_NET(),
                "StartGrabbing 失败");

            _connected = true;
            return true;
        }

        public byte[] Capture()
        {
            throw new System.NotImplementedException();
        }

        public bool Capture(string savePath)
        {
            throw new System.NotImplementedException();
        }

        public void Disconnect()
        {
            if (!_connected) return;

            _camera.MV_CC_StopGrabbing_NET();
            _camera.MV_CC_CloseDevice_NET();
            _camera.MV_CC_DestroyDevice_NET();

            _connected = false;
        }

        public void Dispose() => Disconnect();

        private static void Check(int ret, string msg)
        {
            if (ret != MyCamera.MV_OK)
                throw new CameraException(
                    CameraErrorCode.CaptureFailed,
                    msg + $" (0x{ret:X})");
        }
    }
}
