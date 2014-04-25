using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace CPF
{
    public partial class CPFForm : Form
    {
        public CPFForm()
        {
            InitializeComponent();
        }

        private void CPFNotifyIcon_DoubleClick(object sender, EventArgs e)
        {
            ShowInTaskbar = true;
            CPFNotifyIcon.Visible = false;
            WindowState = FormWindowState.Normal;    
        }

        private void CPFForm_Resize(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
            {
                ShowInTaskbar = false;
                CPFNotifyIcon.Visible = true;
                CPFNotifyIcon.ShowBalloonTip(1000);
            }
        }
    }
}