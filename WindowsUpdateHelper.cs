using Microsoft.Win32;
using System;

namespace ChromeDriverUpdate_net
{
    internal class WindowsUpdateHelper : IUpdateHelper
    {
        public string ChromeDriverName => "chromedriver.exe";
        public string ChromeDriverZipFileName => "chromedriver-win32.zip";

        public Version GetChromeVersion()
        {
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey("Software\\Google\\Chrome\\BLBeacon"))
            {
                if (key != null)
                {
                    object versionObject = key.GetValue("version");

                    if (versionObject != null)
                    {
                        Version version = new Version(versionObject as String);

                        return version;
                    }
                }
            }

            return null;
        }
    }
}
