using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace APKToolGUI.Handlers
{
    internal class ApkinfoControlEventHandlers
    {
        private static FormMain main;
        public ApkinfoControlEventHandlers(FormMain Main)
        {
            main = Main;
            main.selApkFileInfoBtn.Click += SelApkFileInfoBtn_Click;
            main.psLinkBtn.Click += PsLinkBtn_Click;
            main.apkComboLinkBtn.Click += ApkComboLinkBtn_Click;
            main.apkPureLinkBtn.Click += ApkPureLinkBtn_Click;
            main.apkGkLinkBtn.Click += ApkGkLinkBtn_Click;
            main.apkSupportLinkBtn.Click += ApkSupportLinkBtn_Click;
            main.apkMirrorLinkBtn.Click += ApkMirrorLinkBtn_Click;
        }

        private async void SelApkFileInfoBtn_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    await main.GetApkInfo(ofd.FileName);
                }
            }
        }

        private void PsLinkBtn_Click(object sender, EventArgs e)
        {
            if (main.aapt != null)
                Process.Start(main.aapt.PlayStoreLink);
        }

        private void ApkComboLinkBtn_Click(object sender, EventArgs e)
        {
            if (main.aapt != null)
                Process.Start(main.aapt.ApkComboLink);
        }

        private void ApkPureLinkBtn_Click(object sender, EventArgs e)
        {
            if (main.aapt != null)
                Process.Start(main.aapt.ApkPureLink);
        }

        private void ApkGkLinkBtn_Click(object sender, EventArgs e)
        {
            if (main.aapt != null)
                Process.Start(main.aapt.ApkGkLink);
        }

        private void ApkSupportLinkBtn_Click(object sender, EventArgs e)
        {
            if (main.aapt != null)
                Process.Start(main.aapt.ApkSupportLink);
        }
        
        private void ApkMirrorLinkBtn_Click(object sender, EventArgs e)
        {
            if (main.aapt != null)
                Process.Start(main.aapt.ApkMirrorLink);
        }
    }
}
