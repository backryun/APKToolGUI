﻿using APKToolGUI.Properties;
using APKToolGUI.Utils;
using Java;
using Microsoft.Build.Framework.XamlTypes;
using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Shapes;

namespace APKToolGUI
{
    public class Adb : IDisposable
    {
        Process processAdb;
        private bool disposed = false;

        static class Keys
        {
            public const string Devices = " devices -l"; //list connected devices (-l for long output)
            public const string Serial = " -s"; // use device with given serial (overrides $ANDROID_SERIAL)
            public const string Vendor = " -i"; //Vendor
            public const string ApkPath = " -r";
            public const string Abi = " --abi"; //override platform's default ABI
        }

        public event DataReceivedEventHandler OutputDataReceived
        {
            add { processAdb.OutputDataReceived += value; }
            remove { processAdb.OutputDataReceived -= value; }
        }

        public event DataReceivedEventHandler ErrorDataReceived
        {
            add { processAdb.ErrorDataReceived += value; }
            remove { processAdb.ErrorDataReceived -= value; }
        }

        public event EventHandler Exited;
        public int ExitCode { get { return processAdb.ExitCode; } }
        string adbFileName = null;
        public Adb(string AdbFileName)
        {
            adbFileName = AdbFileName;
            processAdb = new Process();
            processAdb.EnableRaisingEvents = true;
            processAdb.StartInfo.FileName = AdbFileName;
            processAdb.StartInfo.UseShellExecute = false; // Disable shell execution to read output data
            processAdb.StartInfo.RedirectStandardOutput = true; // Allow output redirection
            processAdb.StartInfo.RedirectStandardError = true; // Allow error redirection
            processAdb.StartInfo.CreateNoWindow = true; // Do not create window for the launched program
            processAdb.Exited += processAdb_Exited;
        }

        void processAdb_Exited(object sender, EventArgs e)
        {
            processAdb.CancelOutputRead();
            processAdb.CancelErrorRead();
            if (this.Exited != null)
                Exited(this, new EventArgs());
        }

        public void Cancel()
        {
            try
            {
                foreach (var process in Process.GetProcessesByName("adb"))
                {
                    if (process.Id == processAdb.Id)
                    {
                        ProcessUtils.KillAllProcessesSpawnedBy((uint)processAdb.Id);
                        process.Kill();
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Adb] Cancel failed: {ex.Message}");
                // Process termination failure is not critical, so continue
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    if (processAdb != null)
                    {
                        try
                        {
                            if (!processAdb.HasExited)
                            {
                                processAdb.Kill();
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"[Adb] Error disposing process: {ex.Message}");
                        }
                        finally
                        {
                            processAdb.Dispose();
                            processAdb = null;
                        }
                    }
                }
                disposed = true;
            }
        }

        ~Adb()
        {
            Dispose(false);
        }

        public int Install(string device, string inputApk)
        {
            Regex regex = new Regex(@"^(\S+)\s+.*model:(\w+).*");
            Match mdevice = regex.Match(device);

            string setVendor = null, abi = null;
            if (Settings.Default.Adb_SetVendor)
                setVendor = $"{Keys.Vendor} com.android.vending {Keys.ApkPath}";
            if (Settings.Default.Adb_SetOverrideAbi)
            {
                switch (Settings.Default.Adb_OverrideAbi)
                {
                    case 0:
                        abi = Keys.Abi + " arm64-v8a";
                        break;
                    case 1:
                        abi = Keys.Abi + " armeabi-v7a";
                        break;
                    case 2:
                        abi = Keys.Abi + " x86";
                        break;
                    case 3:
                        abi = Keys.Abi + " x86_64";
                        break;
                }
            }

            string args = String.Format($"{Keys.Serial} {mdevice.Groups[1].Value} install {setVendor} {abi} \"{inputApk}\"");

            Log.d("ADB: " + adbFileName + " " + args);
            Debug.WriteLine("Adb: " + args);

            processAdb.EnableRaisingEvents = false;
            processAdb.StartInfo.Arguments = args;
            processAdb.Start();
            processAdb.BeginOutputReadLine();
            processAdb.BeginErrorReadLine();
            processAdb.WaitForExit();

            return ExitCode;
        }

        public string GetDevices()
        {
            Log.d("ADB: " + adbFileName + " " + Keys.Devices);

            using (Process process = new Process())
            {
                process.EnableRaisingEvents = true;
                process.StartInfo.FileName = adbFileName;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.CreateNoWindow = true;
                process.EnableRaisingEvents = false;
                process.StartInfo.Arguments = Keys.Devices;
                process.Start();
                string devices = process.StandardOutput.ReadToEnd();
                process.WaitForExit();
                return devices;
            }
        }

        public void KillProcess()
        {
            foreach (var process in Process.GetProcessesByName("adb"))
            {
                process.Kill();
            }
        }
    }
}
