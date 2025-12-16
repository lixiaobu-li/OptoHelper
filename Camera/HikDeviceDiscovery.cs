using AutomationHelper;
using MvCamCtrl.NET;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace AutomationHelper
{
    public static class HikDeviceDiscovery
    {
        public static IReadOnlyList<HikDeviceInfo> Enumerate()
        {
            HikSdk.EnsureInit();
            
            MyCamera.MV_CC_DEVICE_INFO_LIST list =
                new MyCamera.MV_CC_DEVICE_INFO_LIST();

            int ret = MyCamera.MV_CC_EnumDevices_NET(
                MyCamera.MV_GIGE_DEVICE
              | MyCamera.MV_USB_DEVICE
              | MyCamera.MV_GENTL_GIGE_DEVICE,
                ref list);

            if (ret != MyCamera.MV_OK)
                throw new CameraException(
                    CameraErrorCode.ConnectFailed,
                    "枚举设备失败");

            List<HikDeviceInfo> result = new();

            for (int i = 0; i < list.nDeviceNum; i++)
            {
                var dev = (MyCamera.MV_CC_DEVICE_INFO)
                    Marshal.PtrToStructure(
                        list.pDeviceInfo[i],
                        typeof(MyCamera.MV_CC_DEVICE_INFO));

                if (dev.nTLayerType == MyCamera.MV_GIGE_DEVICE)
                {
                    var gige = (MyCamera.MV_GIGE_DEVICE_INFO)
                        Marshal.PtrToStructure(
                            dev.SpecialInfo.stGigEInfo,
                            typeof(MyCamera.MV_GIGE_DEVICE_INFO));

                    result.Add(new HikDeviceInfo
                    {
                        Raw = dev,
                        Ip = UInt32ToIp(gige.nCurrentIp),
                        Model = gige.chModelName,
                        SerialNumber = gige.chSerialNumber
                    });
                }
            }

            return result;
        }

        private static string UInt32ToIp(uint ip)
        {
            return $"{(ip >> 24) & 0xFF}.{(ip >> 16) & 0xFF}.{(ip >> 8) & 0xFF}.{ip & 0xFF}";
        }
    }
}
