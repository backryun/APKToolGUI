﻿using APKToolGUI.Languages;
using APKToolGUI.Properties;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace APKToolGUI.Handlers
{
    internal class AdbControlEventHandlers
    {
        private static FormMain main;
        public AdbControlEventHandlers(FormMain Main)
        {
            main = Main;
            main.killAdbBtn.Click += KillAdbBtn_Click;
            main.installApkBtn.Click += InstallApkBtn_Click;
            main.refreshDevicesBtn.Click += RefreshDevicesBtn_Click;
            main.selApkAdbBtn.Click += SelApkAdbBtn_Click;
            main.devicesListBox.SelectedValueChanged += DevicesListBox_SelectedValueChanged;
            main.overrideAbiComboBox.SelectedIndexChanged += OverrideAbiComboBox_SelectedIndexChanged;
        }

        private void OverrideAbiComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            Settings.Default.Adb_OverrideAbi = main.overrideAbiComboBox.SelectedIndex;
        }

        private async void RefreshDevicesBtn_Click(object sender, EventArgs e)
        {
            await main.ListDevices();
        }

        private async void KillAdbBtn_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show(Language.ConfirmKillingAdbServer, Application.ProductName, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                main.adb.KillProcess();
                await main.ListDevices();
            }
        }

        private async void InstallApkBtn_Click(object sender, EventArgs e)
        {
            string inputFile = main.apkPathAdbTxtBox.Text;
            if (File.Exists(inputFile))
            {
                await main.Install(inputFile);
            }
            else
                MessageBox.Show(Language.ErrorSelectedFileNotExist, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void SelApkAdbBtn_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    main.apkPathAdbTxtBox.Text = ofd.FileName;
                }
            }
        }

        private void DevicesListBox_SelectedValueChanged(object sender, EventArgs e)
        {
            main.ToLog(ApktoolEventType.None, String.Format(Language.DeviceSelected, main.devicesListBox.SelectedItem));
            main.selAdbDeviceLbl.Text = main.devicesListBox.GetItemText(main.devicesListBox.SelectedItem);
        }
    }
}
