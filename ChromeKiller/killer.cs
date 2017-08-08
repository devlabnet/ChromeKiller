using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

// Just a Test for GITHUB !!

namespace ChromeKiller
{
    class killer
    {
        enum RecycleFlags : int
        {
            SHERB_NOCONFIRMATION = 0x00000001, // Don't ask for confirmation
            SHERB_NOPROGRESSUI = 0x00000001, // Don't show progress
            SHERB_NOSOUND = 0x00000004 // Don't make sound when the action is executed
        }
        [DllImport("Shell32.dll", CharSet = CharSet.Unicode)]
        static extern uint SHEmptyRecycleBin(IntPtr hwnd, string pszRootPath, RecycleFlags dwFlags);
        [DllImport("shell32.dll")]
        static extern int SHQueryRecycleBin(string pszRootPath, ref SHQUERYRBINFO pSHQueryRBInfo);
        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        public struct SHQUERYRBINFO
        {
            public int cbSize;
            public long i64Size;
            public long i64NumItems;
        }
        private int fileDeleted;
        private int dirDeleted;
        string appDataLocalDir;

        public void killAndCleanChrome()
        {
            appDataLocalDir = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            AppendText("application Data :" + string.Format(" Local: {0}", appDataLocalDir));
            killChrome();
        }
        private int GetRecycleBinCount()
        {
            SHQUERYRBINFO sqrbi = new SHQUERYRBINFO();
            sqrbi.cbSize = Marshal.SizeOf(typeof(SHQUERYRBINFO));
            int hresult = SHQueryRecycleBin(string.Empty, ref sqrbi);
            return (int)sqrbi.i64NumItems;
        }
        private void cleanChromePrefFile(string fileName)
        {
            AppendText("------------------------------------------------------");
            AppendText(string.Format("cleanChromePrefFile: {0}", fileName));
            string str = System.IO.File.ReadAllText(fileName);
            str = str.Replace("exit_type\":\"Crashed", "exit_type\":\"None");
            System.IO.File.WriteAllText(fileName, str);
            AppendText("------------------------------------------------------");
        }
        private void AppendText(string v)
        {
            Console.WriteLine(v);
        }
        private void emptyRecycleBin()
        {
            int rbc = GetRecycleBinCount();
            if (rbc > 0)
            {
                try
                {
                    uint IsSuccess = SHEmptyRecycleBin(IntPtr.Zero, null, RecycleFlags.SHERB_NOCONFIRMATION);
                    AppendText("Empty Recycle Bin ...");
                    AppendText("------------------------------------------------------");
                }
                catch (Exception ex)
                {
                    AppendText("EXCEPTION Empty Recycle Bin ..." + ex.Message);
                    AppendText("------------------------------------------------------");
                }
            }
        }
        private void killChrome()
        {
            Process[] proc = Process.GetProcessesByName("chrome");
            AppendText("------------------------------------------------------");
            if (proc.Length > 0)
            {
                AppendText("kill Chrome Processes ...");
                foreach (Process theprocess in proc)
                {
                    try
                    {
                        AppendText("Kill Process " + theprocess.ToString());
                        theprocess.Kill();
                    }
                    catch (Exception ex)
                    {
                        AppendText("EXCEPTION Kill Process: " + ex.Message);
                    }
                }
                System.Threading.Thread.Sleep(100);
                AppendText("------------------------------------------------------");
                AppendText("clear Cache for Chrome ...");
                fileDeleted = 0;
                dirDeleted = 0;
                string UserName = Environment.UserName;
                //Remove - Item - path "C:\Users\$($_.Name)\AppData\Local\Google\Chrome\User Data\Default\Cache\*" - Recurse - Force - EA SilentlyContinue #-Verbose
                //String dirName = @"C:\Users\" + UserName + @"\AppData\Local\Google\Chrome\User Data\Default\Cache";
                string dirName = appDataLocalDir + @"\Google\Chrome\User Data\Default\Cache";
                DeleteDirectory(dirName);
                //Remove-Item - path "C:\Users\$($_.Name)\AppData\Local\Google\Chrome\User Data\Default\Cache2\entries\*" - Recurse - Force - EA SilentlyContinue #-Verbose
                //dirName = @"C:\Users\" + UserName + @"\AppData\Local\Google\Chrome\User Data\Default\Cache2\entries";
                dirName = appDataLocalDir + @"\Google\Chrome\User Data\Default\Cache2\entries";
                DeleteDirectory(dirName);
                //Remove-Item - path "C:\Users\$($_.Name)\AppData\Local\Google\Chrome\User Data\Default\Cookies" - Recurse - Force - EA SilentlyContinue #-Verbose
                //dirName = @"C:\Users\" + UserName + @"\AppData\Local\Google\Chrome\User Data\Default\Cookies";
                dirName = appDataLocalDir + @"\Google\Chrome\User Data\Default\Cookies";
                DeleteDirectory(dirName);
                //Remove-Item - path "C:\Users\$($_.Name)\AppData\Local\Google\Chrome\User Data\Default\Media Cache" - Recurse - Force - EA SilentlyContinue #-Verbose
                //dirName = @"C:\Users\" + UserName + @"\AppData\Local\Google\Chrome\User Data\Default\Media Cache";
                dirName = appDataLocalDir + @"\Google\Chrome\User Data\Default\Media Cache";
                DeleteDirectory(dirName);
                //Remove-Item - path "C:\Users\$($_.Name)\AppData\Local\Google\Chrome\User Data\Default\Cookies-Journal" - Recurse - Force - EA SilentlyContinue #-Verbose
                //dirName = @"C:\Users\" + UserName + @"\AppData\Local\Google\Chrome\User Data\Default\Cookies-Journal";
                dirName = appDataLocalDir + @"\Google\Chrome\User Data\Default\Cookies-Journal";
                DeleteDirectory(dirName);
                // Clear Chrome History
                // C:\Users\xxxxxx\AppData\Local\Google\Chrome\User Data\Default\history
                // C:\Users\Christian\AppData\Local\Google\Chrome\User Data\Default
                DeleteFile(appDataLocalDir + @"\Google\Chrome\User Data\Default\History");
                // Clear Session Restore Data
                AppendText("clear Session Data for Chrome ...");
                //cleanPrefFile - pref "C:\Users\$($_.Name)\AppData\Local\Google\Chrome\User Data\Default\Preferences"
                cleanChromePrefFile(appDataLocalDir + @"\Google\Chrome\User Data\Default\Preferences");
                AppendText(string.Format("Done, Files Deleted: {0}", fileDeleted));
                AppendText("------------------------------------------------------");
                emptyRecycleBin();
            }
            else
            {
                AppendText("Chrome Process Not Found");
                AppendText("------------------------------------------------------");
            }
        }
        private void DeleteFile(string file)
        {
            try
            {
                AppendText("Delete File " + file);
               File.SetAttributes(file, FileAttributes.Normal);
                File.Delete(file);
            }
            catch (Exception ex)
            {
                AppendText("EXCEPTION Delete File: " + ex.Message);
            }
            fileDeleted++;
        }
        private void DeleteDirectory(string path)
        {
            if (Directory.Exists(path))
            {
                //AppendText("ok delete dir: " + path, Color.Blue);
                //Delete all files from the Directory
                foreach (string file in Directory.GetFiles(path))
                {
                    try
                    {
                        AppendText("Delete File " + file);
                        File.SetAttributes(file, FileAttributes.Normal);
                        File.Delete(file);
                    }
                    catch (Exception ex)
                    {
                        AppendText("EXCEPTION Delete File: " + ex.Message);
                    }
                    fileDeleted++;
                }
                //Delete all child Directories
                foreach (string directory in Directory.GetDirectories(path))
                {
                    try
                    {
                        AppendText("DeleteDirectory Dir " + path);
                        DeleteDirectory(directory);
                    }
                    catch (Exception ex)
                    {
                        AppendText("EXCEPTION DeleteDirectory: " + ex.Message);
                    }
                }
                System.Threading.Thread.Sleep(1);
                try
                {
                     AppendText("Delete Dir " + path);
                   Directory.Delete(path);
                }
                catch (Exception ex)
                {
                    AppendText("EXCEPTION Delete Dir: " + ex.Message);
                }
                dirDeleted++;
            }
        }
    }
}
