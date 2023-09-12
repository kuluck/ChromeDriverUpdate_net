using System;
using System.Collections.Generic;
using System.Text;

namespace ChromeDriverUpdate_net
{
    internal interface IUpdateHelper
    {
        string ChromeDriverName { get; }
        string ChromeDriverZipFileName { get; }
        Version GetChromeVersion();
    }
}
