using Microsoft.Build.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace APKToolGUI.Utils
{
    internal class ProcessUtils
    {
        public static void KillAllProcessesSpawnedBy(UInt32 parentProcessId)
        {
            // NOTE: Process Ids are reused!
            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(
                "SELECT * " +
                "FROM Win32_Process " +
                "WHERE ParentProcessId=" + parentProcessId))
            {
                using (ManagementObjectCollection collection = searcher.Get())
                {
                    if (collection.Count > 0)
                    {
                        foreach (var item in collection)
                        {
                            UInt32 childProcessId = (UInt32)item["ProcessId"];
                            if ((int)childProcessId != Process.GetCurrentProcess().Id)
                            {
                                Debug.WriteLine($"Kill child process {childProcessId}");

                                // Recursively kill child processes
                                KillAllProcessesSpawnedBy(childProcessId);

                                // Kill and dispose the child process
                                try
                                {
                                    using (Process childProcess = Process.GetProcessById((int)childProcessId))
                                    {
                                        childProcess.Kill();
                                    }
                                }
                                catch (ArgumentException)
                                {
                                    // Process already exited
                                    Debug.WriteLine($"Process {childProcessId} already exited");
                                }
                                catch (InvalidOperationException ex)
                                {
                                    // Process is terminating or has exited
                                    Debug.WriteLine($"Process {childProcessId} is terminating: {ex.Message}");
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
