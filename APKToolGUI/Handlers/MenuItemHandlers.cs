using APKToolGUI.Languages;
using APKToolGUI.Properties;
using Dark.Net;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace APKToolGUI.Handlers
{
    internal class MenuItemHandlers
    {
        private static FormMain main;
        public MenuItemHandlers(FormMain Main)
        {
            main = Main;
            main.saveLogToFileToolStripMenuItem.Click += SaveLogItem_Click;
            main.settingsToolStripMenuItem.Click += MenuItemSettings_Click;
            main.exitToolStripMenuItem.Click += MenuItemExit_Click;
            main.openTempFolderToolStripMenuItem.Click += OpenTempFolderToolStripMenuItem_Click;
            main.checkForUpdateToolStripMenuItem.Click += MenuItemCheckUpdate_Click;
            main.aboutToolStripMenuItem.Click += MenuItemAbout_Click;
            main.apktoolIssuesToolStripMenuItem.Click += ApktoolIssuesLinkItem_Click;
            main.baksmaliIssuesToolStripMenuItem.Click += BaksmaliIssuesLinkItem_Click;
            main.reportAnIsuueToolStripMenuItem.Click += ReportAnIsuueToolStripMenuItem_Click;
            main.newInsToolStripMenuItem.Click += NewInsToolStripMenuItem_Click;
        }

        private void NewInsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start(System.Reflection.Assembly.GetExecutingAssembly().Location);
        }

        private void SaveLogItem_Click(object sender, EventArgs e)
        {
            using (var sfd = new SaveFileDialog())
            {
                sfd.FileName = "APK Tool GUI logs";
                sfd.Filter = Language.TextFile + " (*.txt)|*.txt";
                sfd.FilterIndex = 2;

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    File.WriteAllText(sfd.FileName, main.logTxtBox.Text);
                }
            }
        }

        private void MenuItemSettings_Click(object sender, EventArgs e)
        {
            Theme theme = (Theme)Settings.Default.Theme;

            if (Program.IsWin10OrAbove())
                DarkNet.Instance.SetCurrentProcessTheme(theme);

            FormSettings frm = new FormSettings();

            if (Program.IsWin10OrAbove())
                DarkNet.Instance.SetWindowThemeForms(frm, theme);
            frm.ShowDialog();
        }

        private void MenuItemExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void OpenTempFolderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Directory.Exists(Program.TEMP_PATH))
                Process.Start("explorer.exe", Program.TEMP_PATH);
            else
            {
                Directory.CreateDirectory(Program.TEMP_PATH);
                Process.Start("explorer.exe", Program.TEMP_PATH);
            }
        }

        private void MenuItemCheckUpdate_Click(object sender, EventArgs e)
        {
            main.updateCheker.CheckAsync();
        }

        private void MenuItemAbout_Click(object sender, EventArgs e)
        {
            Theme theme = (Theme)Settings.Default.Theme;

            if (Program.IsWin10OrAbove())
                DarkNet.Instance.SetCurrentProcessTheme(theme);

            FormAboutBox frm = new FormAboutBox();
            if (Program.IsWin10OrAbove())
                DarkNet.Instance.SetWindowThemeForms(frm, theme);
            frm.ShowDialog();
        }

        private void ApktoolIssuesLinkItem_Click(object sender, EventArgs e)
        {
            Process.Start("https://github.com/iBotPeaches/Apktool/issues?q=is%3Aissue");
        }

        private void BaksmaliIssuesLinkItem_Click(object sender, EventArgs e)
        {
            Process.Start("https://github.com/JesusFreke/smali/issues?q=is%3Aissue");
        }

        private void ReportAnIsuueToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start("https://github.com/AndnixSH/APKToolGUI/issues/new/choose");
        }
    }
}
