using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Runtime.InteropServices;

namespace ChromeDriverUpdate_net
{
    public class ChromeDriverUpdate_net
    {
        internal const string CHROME_DRIVER_BASE_URL = "https://edgedl.me.gvt1.com/edgedl/chrome/chrome-for-testing";
        internal const string LastStable = "https://googlechromelabs.github.io/chrome-for-testing/LATEST_RELEASE_STABLE";
        private IUpdateHelper updateHelper;

        public void Update(string chromeDriverFullPath)
        {
            if (string.IsNullOrEmpty(chromeDriverFullPath))
            {
                throw new ArgumentException(nameof(chromeDriverFullPath));
            }

            chromeDriverFullPath = Path.GetFullPath(chromeDriverFullPath);

            if (!File.Exists(chromeDriverFullPath))
            {
                throw new FileNotFoundException();
            }

            updateHelper = GetUpdateHelper();

            Version chromeDriverVersion = GetChromeDriverVersion(chromeDriverFullPath);
            Version chromeVersion = updateHelper.GetChromeVersion();

            if (UpdateNecessary(chromeDriverVersion, chromeVersion))
            {
                if (updateHelper is WindowsUpdateHelper)
                {
                    ShutdownChromeDriver(chromeDriverFullPath);
                }

                string chromeDriverLastVer = null;
                try
                {
                    WebClient web = new WebClient();
                    chromeDriverLastVer = web.DownloadString(LastStable);
                }
                catch { }
                if (chromeVersion.ToString() == chromeDriverLastVer)
                {
                    UpdateChromeDriver(chromeDriverFullPath, chromeVersion);
                }
                else
                {
                    chromeVersion = new Version(chromeDriverLastVer);
                    UpdateChromeDriver(chromeDriverFullPath, chromeVersion);
                }
            }
        }

        internal IUpdateHelper GetUpdateHelper()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return new WindowsUpdateHelper();
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
            }

            throw new NotSupportedException();
        }

        internal Version GetChromeDriverVersion(string chromeDriverPath)
        {
            string output = new ProcessExecuter().Run($"{chromeDriverPath}", "-v");

            // output like this
            // ChromeDriver 88.0.4324.96 (68dba2d8a0b149a1d3afac56fa74648032bcf46b-refs/branch-heads/4324@{#1784})
            string versionStr = output.Split(' ')[1];
            Version version = new Version(versionStr);

            return version;
        }

        internal bool UpdateNecessary(Version chromeVersion, Version chromeDriverVersion)
        {
            return !CompareVersionMajorToBuild(chromeVersion, chromeDriverVersion);
        }

        internal bool CompareVersionMajorToBuild(Version v1, Version v2)
        {
            if (v1 == null)
            {
                throw new ArgumentNullException(nameof(v1));
            }
            if (v2 == null)
            {
                throw new ArgumentNullException(nameof(v2));
            }

            return v1.Major == v2.Major &&
                   v1.Minor == v2.Minor &&
                   v1.Build == v2.Build;
        }

        internal void ShutdownChromeDriver(string chromeDriverFullPath)
        {
            var processes = Process.GetProcesses();

            foreach (Process process in processes)
            {
                // find exact path
                if (process.MainWindowTitle == chromeDriverFullPath)
                {
                    process.Kill();
                }
            }
        }

        internal void UpdateChromeDriver(string chromeDriverFullPath, Version chromeVersion)
        {

            string zipFileDownloadPath = DownloadChromeDriverZipFile(chromeVersion);

            string newChromeDriverFullPath = GetNewChromeDriverFromZipFile(zipFileDownloadPath);

            File.Copy(newChromeDriverFullPath, chromeDriverFullPath, true);

            File.Delete(newChromeDriverFullPath);
        }

        internal void UnzipFile(string zipPath, string unzipPath, bool deleteZipFile = true)
        {
            if (Directory.Exists(unzipPath + "\\chromedriver-win32"))
            {
                Directory.Delete(unzipPath + "\\chromedriver-win32", true);
            }

            ZipFile.ExtractToDirectory(zipPath, unzipPath);

            if (deleteZipFile)
            {
                File.Delete(zipPath);
            }
        }

        internal string DownloadChromeDriverZipFile(Version chromeVersion)
        {
            string version = chromeVersion.ToString();

            string downloadUrl = $"{CHROME_DRIVER_BASE_URL}/{version}/win32/{updateHelper.ChromeDriverZipFileName}";
            //string downloadUrl = $"{CHROME_DRIVER_BASE_URL}/{version}/win32/chromedriver-win32.zip";
            string downloadZipFileFullPath = Path.Combine(Path.GetTempPath(), updateHelper.ChromeDriverZipFileName);

            DownloadFile(downloadUrl, downloadZipFileFullPath);

            return downloadZipFileFullPath;
        }

        internal void DownloadFile(string downloadUrl, string downloadPath)
        {
            new WebClient().DownloadFile(downloadUrl, downloadPath);
        }

        internal string GetNewChromeDriverFromZipFile(string zipFileDownloadPath)
        {
            //string unzipPath = Path.Combine(Path.GetDirectoryName(zipFileDownloadPath), Path.GetFileNameWithoutExtension(zipFileDownloadPath));
            string unzipPath = Path.GetDirectoryName(zipFileDownloadPath);

            UnzipFile(zipFileDownloadPath, unzipPath, true);

            return FindNewChromeDriverFullPathFromUnzipPath(Path.Combine(Path.GetDirectoryName(zipFileDownloadPath), Path.GetFileNameWithoutExtension(zipFileDownloadPath)));
        }

        internal string FindNewChromeDriverFullPathFromUnzipPath(string chromeDriverUnzipPath)
        {            
            DirectoryInfo directoryInfo = new DirectoryInfo(chromeDriverUnzipPath);

            FileInfo[] files = directoryInfo.GetFiles();

            foreach (FileInfo file in files)
            {
                // ignore case
                if (file.Name.ToLower() == updateHelper.ChromeDriverName.ToLower())
                {
                    string newPath = Path.Combine(Path.GetDirectoryName(chromeDriverUnzipPath), file.Name);

                    File.Copy(file.FullName, newPath, true);

                    Directory.Delete(chromeDriverUnzipPath, true);

                    return newPath;
                }
            }

            throw new Exception("Can't find chromedriver name. name: " + updateHelper.ChromeDriverName.ToLower());
        }
    }
}
