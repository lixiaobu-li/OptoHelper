using MvCamCtrl.NET;

namespace AutomationHelper
{
    internal static class HikSdk
    {
        private static bool _inited;

        public static void EnsureInit()
        {
            if (_inited) return;

            int ret = MyCamera.MV_CC_Initialize_NET();
            if (ret != MyCamera.MV_OK)
                throw new CameraException(
                    CameraErrorCode.ConnectFailed,
                    "海康 SDK 初始化失败");

            _inited = true;
        }
    }
}
