using Godot;

namespace Moonvalk.Accessory
{
    /// <summary>
    /// Helper functions for detecting devices.
    /// </summary>
    public static class DeviceHelpers
    {
        /// <summary>
        /// All available device parsing states.
        /// </summary>
        public enum DeviceState
        {
            None,
            Desktop,
            Mobile,
            Html5
        }

        /// <summary>
        /// Stores the current device state.
        /// </summary>
        private static DeviceState _currentState = DeviceState.None;

        /// <summary>
        /// Determines if the device is mobile or desktop based on operating system.
        /// </summary>
        /// <returns>Returns true if the device is determined mobile.</returns>
        public static bool IsDeviceMobile()
        {
            SetDeviceState();
            return _currentState == DeviceState.Mobile;
        }

        /// <summary>
        /// Determines if the device is mobile or desktop based on operating system.
        /// </summary>
        /// <returns>Returns true if the device is determined mobile.</returns>
        public static bool IsDeviceHtml5()
        {
            SetDeviceState();
            return _currentState == DeviceState.Html5;
        }

        /// <summary>
        /// Determines if the device is mobile or desktop based on operating system.
        /// </summary>
        /// <returns>Returns true if the device is determined mobile.</returns>
        public static bool IsDeviceDesktop()
        {
            SetDeviceState();
            return _currentState == DeviceState.Desktop;
        }

        /// <summary>
        /// Helper for determining the current device.
        /// </summary>
        private static void SetDeviceState()
        {
            if (_currentState != DeviceState.None)
            {
                return;
            }

            var os = OS.GetName();
            switch (os)
            {
                case "Windows":
                case "OSX":
                case "macOS":
                case "Linux":
                case "Server":
                case "UWP":
                    _currentState = DeviceState.Desktop;
                    break;
                case "HTML5":
                case "Web":
                    _currentState = DeviceState.Html5;
                    break;
                case "Android":
                case "iOS":
                case "BlackBerry 10":
                    _currentState = DeviceState.Mobile;
                    break;
            }
        }
    }
}