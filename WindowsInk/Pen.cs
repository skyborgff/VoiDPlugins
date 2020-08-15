using System;
using System.Runtime.InteropServices;
using TabletDriverPlugin;

namespace WindowsInk
{
    public static class Pen
    {
        private static IntPtr _penHandle;
        private static POINTER_TYPE_INFO[] pointer;
        private static uint _pointerId;
        private static IntPtr _sourceDevice;

        public static void Init()
        {
            uint count = 0;
            NativeMethods.GetPointerDevices(ref count, out var pointerDevices);
            foreach (var device in pointerDevices)
            {
                if (device.pointerDeviceType == POINTER_DEVICE_TYPE.EXTERNAL_PEN ||
                    device.pointerDeviceType == POINTER_DEVICE_TYPE.INTEGRATED_PEN)
                {
                    _pointerId = (uint)device.startingCursorId;
                    _sourceDevice = device.device;
                }
            }

            Log.Write("WindowsInk", "Pen created", LogLevel.Debug);
            var _pointerInfo = new POINTER_INFO
            {
                pointerType = POINTER_INPUT_TYPE.PT_PEN,
                pointerId = _pointerId,
                frameId = 0,
                pointerFlags = POINTER_FLAGS.NONE,
                sourceDevice = IntPtr.Zero,
                hwndTarget = IntPtr.Zero,
                ptPixelLocation = new POINT(),
                ptPixelLocationRaw = new POINT(),
                dwTime = 0,
                historyCount = 0,
                dwKeyStates = 0,
                PerformanceCount = 0,
                ButtonChangeType = POINTER_BUTTON_CHANGE_TYPE.NONE
            };

            var _penInfo = new POINTER_PEN_INFO
            {
                pointerInfo = _pointerInfo,
                pointerFlags = PEN_FLAGS.NONE,
                penMask = PEN_MASK.PRESSURE,
                pressure = 512,
                rotation = 0,
                tiltX = 0,
                tiltY = 0
            };

            pointer = new POINTER_TYPE_INFO[]
            {
                new POINTER_TYPE_INFO
                {
                    type = POINTER_INPUT_TYPE.PT_PEN,
                    penInfo = _penInfo
                }
            };

            // Retrieve handle to custom pen
            _penHandle = NativeMethods.CreateSyntheticPointerDevice(POINTER_INPUT_TYPE.PT_PEN, 1, POINTER_FEEDBACK_MODE.INDIRECT);
            var err = Marshal.GetLastWin32Error();
            if (err < 0 || _penHandle == IntPtr.Zero)
                Log.Write("WindowsInk", "Failed creating synthetic pointer. Reason: " + err, LogLevel.Error);
            else
                Log.Write("WindowsInk", "Pen handle retrieved successfully", LogLevel.Debug);

            // Notify WindowsInk
            ClearPointerFlags(POINTER_FLAGS.NEW);
            Inject();

            // Back to normal state
            ClearPointerFlags(POINTER_FLAGS.INRANGE | POINTER_FLAGS.PRIMARY);
        }

        public static void Inject()
        {
            if (!NativeMethods.InjectSyntheticPointerInput(_penHandle, pointer, 1))
            {
                Log.Write("WindowsInk", "Injection Failed. Reason: " + Marshal.GetLastWin32Error());
            }
        }

        public static void SetPosition(POINT point)
        {
            pointer[0].penInfo.pointerInfo.ptPixelLocation = point;
            pointer[0].penInfo.pointerInfo.ptPixelLocationRaw = point;
            // pointer[0].penInfo.pointerInfo.ptHimetricLocation = point;
            // pointer[0].penInfo.pointerInfo.ptHimetricLocationRaw = point;
        }

        public static void SetPressure(uint pressure)
        {
            pointer[0].penInfo.pressure = pressure;
        }

        public static void SetPointerFlags(POINTER_FLAGS flags)
        {
            pointer[0].penInfo.pointerInfo.pointerFlags |= flags;
        }

        public static void UnsetPointerFlags(POINTER_FLAGS flags)
        {
            pointer[0].penInfo.pointerInfo.pointerFlags &= ~flags;
        }

        public static void ClearPointerFlags()
        {
            pointer[0].penInfo.pointerInfo.pointerFlags = 0;
        }

        public static void ClearPointerFlags(POINTER_FLAGS flags)
        {
            pointer[0].penInfo.pointerInfo.pointerFlags = flags;
        }
    }
}