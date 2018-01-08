﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace IB_Gateway_Go
{
    public partial class frmMain : Form
    {
        Process p;

        public frmMain()
        {
            InitializeComponent();
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            if(btnStart.Text == "Start")
            {
                p = StartGW();
            }
            else
            {
                StopGW(p);
            }
            
        }

        private Process StartGW()
        {
            string gwExePath = @"C:\Jts\ibgateway\963\ibgateway.exe";
            string javaOptions = //"-cp jts.jar:total.2013.jar - Dsun.java2d.noddraw = true - Xmx512M ";

            @"username=" + txtUserName.Text +
                           " password=" + txtPassword.Text;
            string date = DateTime.Now.ToLongDateString();
            string logFilePath = @"C:\logs\IB_GW_Runner_" + date + ".log";
            // ${ JAVAEXE} ${ JAVAOPTIONS} ${ IBJTSDIR} username =${ USER} password =${ PASS} > ${ LOGFILE} 2 > &1
            // -cp [java options] [IBJts directory] [username=XXX] [password=ZZZ]

            ProcessStartInfo psi = new ProcessStartInfo(gwExePath, javaOptions);//, javaString);
           // psi.WorkingDirectory = @"C:\Jts\ibgateway\963\jars"; 
            psi.CreateNoWindow = false;
            psi.UseShellExecute = false;
            
            //p.StartInfo = psi;
            if ((p = Process.Start(psi)) == null)
            {
                throw new InvalidOperationException("??");
            }
            else
            {
                lblGWStatus.Text = "Running";
                btnStart.Text = "Stop";
                return p;
            }

            //p.WaitForExit();
            //int exitCode = p.ExitCode;
            //p.Close();
        }

        private void StopGW(Process p)
        {
            if (p != null)
            {
                p.Close();
                btnStart.Text = "Start";
                lblGWStatus.Text = "Not Running";
            }
        }
    }
}