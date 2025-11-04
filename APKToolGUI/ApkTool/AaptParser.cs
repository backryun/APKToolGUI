using APKToolGUI.Web;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace APKToolGUI.Utils
{
    public class AaptParser
    {
        public string ApkFile;

        public string RealApkFile;

        public string Armv7ApkFile;

        public string Arm64ApkFile;

        public string AppName;

        public string PackageName;

        public string VersionName;

        public string VersionCode;

        public string MinSdkVersionDetailed;

        public string TargetSdkVersionDetailed;

        public string MinSdkVersion;

        public string TargetSdkVersion;

        public string LaunchableActivity;

        public string Permissions;

        public string Screens;

        public string Locales;

        public string Densities;

        public string NativeCode;

        public string PlayStoreLink;

        public string ApkComboLink;

        public string ApkPureLink;

        public string ApkAioLink;

        public string ApkGkLink;

        public string ApkSupportLink;

        public string ApkSosLink;

        public string ApkMirrorLink;

        public string ApkDlLink;

        public string FullInfo;

        internal string AppIcon = null;

        internal string AppIcon120 = null;

        internal string AppIcon160 = null;

        internal string AppIcon240 = null;

        internal string AppIcon320 = null;

        internal string AppIcon480 = null;

        internal string AppIcon640 = null;

        internal string AppIcon65534 = null;

        public bool Parse(string file)
        {
            bool result = true;

            string info = ParseApkInfo(file);

            FullInfo = info;

            if (!String.IsNullOrEmpty(info))
            {
                string[] lines = info.Split(
                    new string[] { "\r\n", "\r", "\n" },
                    StringSplitOptions.None);

                List<string> nativecode = new List<string> { };
                List<string> nativecode2 = new List<string> { };
                StringBuilder permissionsBuilder = new StringBuilder();
                foreach (string line in lines)
                {
                    switch (line.Split(':')[0])
                    {
                        case "package":
                            PackageName = StringExt.RegexExtract(@"(?<=package: name=\')(.*?)(?=\')", line);
                            VersionName = StringExt.RegexExtract(@"(?<=versionName=\')(.*?)(?=\')", line);
                            VersionCode = StringExt.RegexExtract(@"(?<=versionCode=\')(.*?)(?=\')", line);
                            break;
                        case "uses-permission":
                            permissionsBuilder.AppendLine(StringExt.RegexExtract(@"(?<=name=\')(.*?)(?=\')", line));
                            break;
                        case "sdkVersion":
                            MinSdkVersionDetailed = SdkToAndroidVer(StringExt.RegexExtract(@"(?<=sdkVersion:\')(.*?)(?=\')", line));
                            MinSdkVersion = StringExt.RegexExtract(@"(?<=sdkVersion:\')(.*?)(?=\')", line);
                            break;
                        case "targetSdkVersion":
                            TargetSdkVersionDetailed = SdkToAndroidVer(StringExt.RegexExtract(@"(?<=targetSdkVersion:\')(.*?)(?=\')", line));
                            TargetSdkVersion = StringExt.RegexExtract(@"(?<=targetSdkVersion:\')(.*?)(?=\')", line);
                            break;
                        case "application-label":
                            AppName = StringExt.RegexExtract(@"(?<=application-label:\')(.*?)(?=\')", line);
                            break;
                        case "launchable-activity":
                            LaunchableActivity = StringExt.RegexExtract(@"(?<=name=\')(.*?)(?=\')", line);
                            break;
                        case "supports-screens":
                            var screens = Regex.Matches(line.Split(':')[1], @"(?<= \')(.*?)(?=\')").Cast<Match>().Select(m => m.Value).ToList();
                            Screens = string.Join(", ", screens);
                            break;
                        case "locales":
                            var locales = Regex.Matches(line.Split(':')[1], @"(?<= \')(.*?)(?=\')").Cast<Match>().Select(m => m.Value).ToList();
                            Locales = string.Join(", ", locales);
                            break;
                        case "densities":
                            var densities = Regex.Matches(line.Split(':')[1], @"(?<= \')(.*?)(?=\')").Cast<Match>().Select(m => m.Value).ToList();
                            Densities = string.Join(", ", densities);
                            break;
                        case "alt-native-code":
                            nativecode2 = Regex.Matches(line.Split(':')[1], @"(?<= \')(.*?)(?=\')").Cast<Match>().Select(m => m.Value).ToList();
                            break;
                        case "native-code":
                            nativecode = Regex.Matches(line.Split(':')[1], @"(?<= \')(.*?)(?=\')").Cast<Match>().Select(m => m.Value).ToList();
                            break;
                    }
                }

                Permissions = permissionsBuilder.ToString();
                List<string> combinedList = nativecode2.Concat(nativecode).ToList();
                NativeCode += string.Join(", ", combinedList);
                ApkFile = file;
                PlayStoreLink = "https://play.google.com/store/apps/details?id=" + PackageName;
                ApkComboLink = "https://apkcombo.com/a/" + PackageName;
                ApkPureLink = "https://apkpure.com/a/" + PackageName;
                ApkSupportLink = "https://apk.support/app/" + PackageName;
                ApkMirrorLink = "https://www.apkmirror.com/?post_type=app_release&searchtype=apk&s=" + PackageName;
                ApkGkLink = "https://apkgk.com/" + PackageName + "/download";

                AppIcon120 = StringExt.RegexExtract(@"(?<=application-icon-120:\')(.*?)(?=\')", FullInfo);
                AppIcon160 = StringExt.RegexExtract(@"(?<=application-icon-160:\')(.*?)(?=\')", FullInfo);
                AppIcon240 = StringExt.RegexExtract(@"(?<=application-icon-240:\')(.*?)(?=\')", FullInfo);
                AppIcon320 = StringExt.RegexExtract(@"(?<=application-icon-320:\')(.*?)(?=\')", FullInfo);
                AppIcon480 = StringExt.RegexExtract(@"(?<=application-icon-480:\')(.*?)(?=\')", FullInfo);
                AppIcon640 = StringExt.RegexExtract(@"(?<=application-icon-640:\')(.*?)(?=\')", FullInfo);
                AppIcon65534 = StringExt.RegexExtract(@"(?<=application-icon-65534:\')(.*?)(?=\')", FullInfo);

                result = true;
            }
            else
                result = false;

            return result;
        }

        private string ParseApkInfo(string path)
        {
            //For some reason, aapt2 hangs, so we will only use aapt2 when aapt1 fails to read UTF-8 character
            string apkinfo = CMD.ProcessStartWithOutput(Program.AAPT_PATH, "dump badging \"" + path + "\"");
            if (String.IsNullOrEmpty(apkinfo))
            {
                string apkinfo2 = CMD.ProcessStartWithOutput(Program.AAPT2_PATH, "dump badging \"" + path + "\"");
                if (!String.IsNullOrEmpty(apkinfo2))
                {
                    return apkinfo2;
                }
                else
                    return "";
            }
            else
                return apkinfo;
        }

        public string GetIcon(string apkPath)
        {
            string[] png = { "mipmap-xxxhdpi-v4", "mipmap-xxhdpi-v4", "mipmap-xhdpi-v4", "mipmap-hdpi-v4", "mipmap-mdpi-v4", "mipmap-xhdpi", "mipmap-hdpi", "drawable-xxxhdpi-v4", "drawable-xxhdpi-v4", "drawable-xhdpi-v4", "drawable-hdpi-v4", "drawable-mdpi-v4" };
            string icon = "";

            if (!string.IsNullOrEmpty(AppIcon65534))
                icon = AppIcon65534;
            else if (!string.IsNullOrEmpty(AppIcon640))
                icon = AppIcon640;
            else if (!string.IsNullOrEmpty(AppIcon480))
                icon = AppIcon480;
            else if (!string.IsNullOrEmpty(AppIcon320))
                icon = AppIcon320;
            else if (!string.IsNullOrEmpty(AppIcon240))
                icon = AppIcon240;
            else if (!string.IsNullOrEmpty(AppIcon160))
                icon = AppIcon160;
            else if (!string.IsNullOrEmpty(AppIcon120))
                icon = AppIcon120;

            icon = icon.Replace(".xml", ".png");

            Debug.WriteLine("Icon: " + icon);

            string cacheDir = Path.Combine(Program.TEMP_PATH, PackageName);
            string iconLocation = Path.Combine(cacheDir, Path.GetFileName(icon));
            Directory.CreateDirectory(cacheDir);

            if (icon.Contains("anydpi-v26"))
            {
                foreach (string Png in png)
                {
                    string icon2 = icon.Replace("mipmap-anydpi-v26", Png).Replace("drawable-anydpi-v26", Png);
                    ZipUtils.ExtractFile(apkPath, icon2, cacheDir);
                    if (File.Exists(iconLocation))
                    {
                        break;
                    }
                }
            }
            else if (icon.Contains("v26"))
            {
                string icon2 = icon.Replace("v26", "v4");
                ZipUtils.ExtractFile(apkPath, icon2, cacheDir);
                icon2 = icon.Replace("-v26", "");
                ZipUtils.ExtractFile(apkPath, icon2, cacheDir);
            }
            else
            {
                ZipUtils.ExtractFile(apkPath, icon, cacheDir);
            }

            if (!File.Exists(iconLocation))
            {
                try
                {
                    WebDownload w = new WebDownload();
                    string ps = w.DownloadString("https://play.google.com/store/apps/details?id=" + PackageName);
                    //File.WriteAllText("R:\\t.txt", ps);
                    string icondl = Path.Combine(cacheDir, "icon.png");
                    Directory.CreateDirectory(cacheDir);
                    w.DownloadFile(StringExt.RegexExtract(@"(?<=\""image\"":\"")(.*?)(?=\"",\"")", ps), icondl);
                    iconLocation = icondl;
                }
                catch (System.Net.WebException ex)
                {
                    Debug.WriteLine($"[AaptParser] Failed to download icon from web: {ex.Message}");
                    // Icon download failure is not critical, use default value
                }
                catch (IOException ex)
                {
                    Debug.WriteLine($"[AaptParser] Failed to save icon file: {ex.Message}");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[AaptParser] Unexpected error getting icon: {ex.Message}");
                }
            }

            return iconLocation;
        }

        //https://apilevels.com/
        public string SdkToAndroidVer(string sdk)
        {
            switch (sdk)
            {
                case "36":
                    return sdk + ": Android 16";
                case "35":
                    return sdk + ": Android 15";
                case "34":
                    return sdk + ": Android 14";
                case "33":
                    return sdk + ": Android 13";
                case "32":
                    return sdk + ": Android 12.0L";
                case "31":
                    return sdk + ": Android 12";
                case "30":
                    return sdk + ": Android 11";
                case "29":
                    return sdk + ": Android 10";
                case "28":
                    return sdk + ": Android 9 (Pie)";
                case "27":
                    return sdk + ": Android 8.1 (Oreo)";
                case "26":
                    return sdk + ": Android 8.0 (Oreo)";
                case "25":
                    return sdk + ": Android 7.1 (Nougat)";
                case "24":
                    return sdk + ": Android 7.0 (Nougat)";
                case "23":
                    return sdk + ": Android 6 (Marshmallow)";
                case "22":
                    return sdk + ": Android 5.1 (Lollipop)";
                case "21":
                    return sdk + ": Android 5.0 (Lollipop)";
                case "20":
                    return sdk + ": Android 4.4W (KitKat Watch)";
                case "19":
                    return sdk + ": Android 4.4 (KitKat)";
                case "18":
                    return sdk + ": Android 4.3 (Jelly Bean)";
                case "17":
                    return sdk + ": Android 4.2 (Jelly Bean)";
                case "16":
                    return sdk + ": Android 4.1 (Jelly Bean)";
                case "15":
                    return sdk + ": Android 4.0.3 (Ice Cream Sandwich)";
                case "14":
                    return sdk + ": Android 4.0 (Ice Cream Sandwich)";
                case "13":
                    return sdk + ": Android 3.2 (Honeycomb)";
                case "12":
                    return sdk + ": Android 3.1 (Honeycomb)";
                case "11":
                    return sdk + ": Android 3.0 (Honeycomb)";
                case "10":
                    return sdk + ": Android 2.3.3 Gingerbread";
                case "9":
                    return sdk + ": Android 2.3 (Gingerbread)";
                case "8":
                    return sdk + ": Android 2.2 (Froyo)";
                case "7":
                    return sdk + ": Android 2.1 (Eclair)";
                case "6":
                    return sdk + ": Android 2.0.1 (Eclair)";
                case "5":
                    return sdk + ": Android 2.0 (Eclair)";
                case "4":
                    return sdk + ": Android 1.6 (Donut)";
                case "3":
                    return sdk + ": Android 1.5 (Cupcake)";
                case "2":
                    return sdk + ": Android 1.1 (Base 1.1)";
                case "1":
                    return sdk + ": Android 1.0 (Base)";
                default:
                    return sdk;
            }
        }
    }
}
