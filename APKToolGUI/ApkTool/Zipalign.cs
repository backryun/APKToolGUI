using APKToolGUI.Properties;
using APKToolGUI.Utils;
using System;
using System.Diagnostics;
using System.IO;

namespace APKToolGUI
{
    public class Zipalign : IDisposable
    {
        Process processZipalign;
        private bool disposed = false;

        static class Keys
        {
            public const string CheckOnly = " -c";
            public const string OverwriteOutputFile = " -f";
            public const string VerboseOut = " -v";
            public const string Recompress = " -z";
        }

        public event DataReceivedEventHandler OutputDataReceived
        {
            add { processZipalign.OutputDataReceived += value; }
            remove { processZipalign.OutputDataReceived -= value; }
        }
        public event DataReceivedEventHandler ErrorDataReceived
        {
            add { processZipalign.ErrorDataReceived += value; }
            remove { processZipalign.ErrorDataReceived -= value; }
        }
        public event EventHandler Exited;
        public int ExitCode { get { return processZipalign.ExitCode; } }

        string _zipalignFileName;
        public Zipalign(string zipalignFileName)
        {
            _zipalignFileName = zipalignFileName;
            processZipalign = new Process();
            processZipalign.EnableRaisingEvents = true;
            processZipalign.StartInfo.FileName = zipalignFileName;
            processZipalign.StartInfo.UseShellExecute = false; // Disable shell execution to read output data
            processZipalign.StartInfo.RedirectStandardOutput = true; // Allow output redirection
            processZipalign.StartInfo.RedirectStandardError = true; // Allow error redirection
            processZipalign.StartInfo.CreateNoWindow = true; // Do not create window for the launched program
            processZipalign.Exited += processZipalign_Exited;
        }

        void processZipalign_Exited(object sender, EventArgs e)
        {
            processZipalign.CancelOutputRead();
            processZipalign.CancelErrorRead();
            if (this.Exited != null)
                Exited(this, new EventArgs());
        }

        public void Cancel()
        {
            try
            {
                foreach (var process in Process.GetProcessesByName("zipalign"))
                {
                    if (process.Id == processZipalign.Id)
                    {
                        ProcessUtils.KillAllProcessesSpawnedBy((uint)processZipalign.Id);
                        process.Kill();
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Zipalign] Cancel failed: {ex.Message}");
                // 프로세스 종료 실패는 치명적이지 않으므로 계속 진행
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
                    if (processZipalign != null)
                    {
                        try
                        {
                            if (!processZipalign.HasExited)
                            {
                                processZipalign.Kill();
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"[Zipalign] Error disposing process: {ex.Message}");
                        }
                        finally
                        {
                            processZipalign.Dispose();
                            processZipalign = null;
                        }
                    }
                }
                disposed = true;
            }
        }

        ~Zipalign()
        {
            Dispose(false);
        }

        public int Align(string input, string output)
        {
            string keyCheckOnly = null, keyVerbose = null, keyRecompress = null, keyOverwriteOutputFile = null, keyOutputFile = null;

            if (Settings.Default.Zipalign_Verbose)
                keyVerbose = Keys.VerboseOut;
            if (Settings.Default.Zipalign_CheckOnly)
            {
                keyCheckOnly = Keys.CheckOnly;
            }
            else
            {
                if (Settings.Default.Zipalign_Recompress)
                    keyRecompress = Keys.Recompress;
                if (Settings.Default.Zipalign_OverwriteOutputFile)
                {
                    keyOverwriteOutputFile = Keys.OverwriteOutputFile;
                }
                //if (Settings.Default.Zipalign_OverwriteOutputFile)
                keyOutputFile = String.Format(" \"{0}\"", PathUtils.GetDirectoryNameWithoutExtension(output) + "_align_temp.apk");
                //else
                //    keyOutputFile = String.Format(" \"{0}\"", output);
            }

            string args = String.Format("{0}{1}{2}{3} {4} \"{5}\" {6}", keyCheckOnly, keyOverwriteOutputFile, keyVerbose, keyRecompress, Settings.Default.Zipalign_AlignmentInBytes, input, keyOutputFile);

            Log.v("Zipalign: " + _zipalignFileName + " " + args);

            processZipalign.StartInfo.Arguments = args;
            processZipalign.Start();
            processZipalign.BeginOutputReadLine();
            processZipalign.BeginErrorReadLine();
            processZipalign.WaitForExit();

            // Handle temp file (only when not in CheckOnly mode)
            if (!Settings.Default.Zipalign_CheckOnly)
            {
                string tempFile = PathUtils.GetDirectoryNameWithoutExtension(output) + "_align_temp.apk";
                
                try
                {
                    // 1. Delete output file
                    if (File.Exists(output))
                    {
                        File.Delete(output);
                        Debug.WriteLine($"[Zipalign] Deleted existing output: {output}");
                    }
                    
                    // 2. Check temp file existence and move
                    if (File.Exists(tempFile))
                    {
                        File.Move(tempFile, output);
                        Debug.WriteLine($"[Zipalign] Moved temp file to output: {tempFile} -> {output}");
                    }
                    else
                    {
                        Debug.WriteLine($"[Zipalign] Warning: Temp file not found: {tempFile}");
                        return 1; // Return failure code
                    }
                }
                catch (IOException ex)
                {
                    Debug.WriteLine($"[Zipalign] Failed to process output file: {ex.Message}");
                    
                    // Attempt to cleanup temp file
                    try
                    {
                        if (File.Exists(tempFile))
                        {
                            File.Delete(tempFile);
                            Debug.WriteLine($"[Zipalign] Cleaned up temp file: {tempFile}");
                        }
                    }
                    catch (Exception cleanupEx)
                    {
                        Debug.WriteLine($"[Zipalign] Failed to cleanup temp file: {cleanupEx.Message}");
                    }
                    
                    return 1;
                }
                catch (UnauthorizedAccessException ex)
                {
                    Debug.WriteLine($"[Zipalign] Access denied: {ex.Message}");
                    return 1;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[Zipalign] Unexpected error processing output: {ex.Message}");
                    return 1;
                }
            }

            return ExitCode;
        }
    }
}
