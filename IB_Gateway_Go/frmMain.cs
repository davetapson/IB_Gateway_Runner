using NLog;
using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace IB_Gateway_Runner
{
    public partial class frmMain : Form
    {
        DateTime started;
        private static Logger logger = LogManager.GetCurrentClassLogger();

        string GWProcessName = "ibgateway";

        public frmMain()
        {
            InitializeComponent();
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            if (btnStart.Text == "Start")
            {
                StartGW();
            }
            else
            {
                StopGW();
            }
        }

        public void StartGW()
        {
            logger.Info("GW started.");

            SaveSettings();

            if (IsRunning(GWProcessName))
            {
                SetControlsToRunning();

                MessageBox.Show("GW is already running.", 
                                "IB GW Runner",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Exclamation);
                return;
            }

            string gwExePath = @"C:\Jts\ibgateway\963\ibgateway.exe";
            string javaOptions = //"-cp jts.jar:total.2013.jar - Dsun.java2d.noddraw = true - Xmx512M ";

            @"username=" + txtUserName.Text +
                           " password=" + txtPassword.Text;
            
            //string logFilePath = @"C:\logs\IB_GW_Runner_" + date + ".log";
            // ${ JAVAEXE} ${ JAVAOPTIONS} ${ IBJTSDIR} username =${ USER} password =${ PASS} > ${ LOGFILE} 2 > &1
            // -cp [java options] [IBJts directory] [username=XXX] [password=ZZZ]

            ProcessStartInfo psi = new ProcessStartInfo(gwExePath, javaOptions);//, javaString);
           // psi.WorkingDirectory = @"C:\Jts\ibgateway\963\jars"; 
            psi.CreateNoWindow = false;
            psi.UseShellExecute = false;

            //p.StartInfo = psi;
            Process p = Process.Start(psi);

            if (p == null)
            {
                logger.Error("GW Process failed.");
                throw new InvalidOperationException("GW Process failed.");
            }
            else
            {
                SetControlsToRunning();
            }

            //p.WaitForExit();
            //int exitCode = p.ExitCode;
            //p.Close();
        }

        private void SetControlsToRunning()
        {

            lblGWStatus.Text = "Running";
            btnStart.Text = "Stop";
            lblStarted.Text =  DateTime.Now.ToString("yyyy-dd-MM HH:mm:ss"); ;
            tmrTimer.Start();
        }

        private void SetControlsToNotRunning()
        {

            lblGWStatus.Text = "Not Running";
            btnStart.Text = "Start";
            lblStarted.Text = lblRunTime.Text = "...";
        }

        private void SaveSettings()
        {
            Properties.GW_Runner.Default.UserName = txtUserName.Text;
            Properties.GW_Runner.Default.Password = txtPassword.Text;
            Properties.GW_Runner.Default.Enabled = chkEnabled.Checked;
            Properties.GW_Runner.Default.RunGWOnStartup = chkRunGWOnStartUp.Checked;
            Properties.GW_Runner.Default.Save();
        }

        public void StopGW()
        {
            logger.Info("GW Runner stopped.");

            //tmrTimer.Stop();
            btnStart.Text = "Start";
            lblGWStatus.Text = "Not Running";

            Process p = IBGWRunnerProcess(GWProcessName);
            if (p != null)
            {
                p.Kill();                
            }
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            InitialiseApp();
        }

        private void InitialiseApp()
        {
            logger.Info("IB GW Runner started.");

            started = DateTime.Now;
            txtUserName.Text = Properties.GW_Runner.Default.UserName;
            txtPassword.Text = Properties.GW_Runner.Default.Password;
            chkEnabled.Checked = Properties.GW_Runner.Default.Enabled;
            chkRunGWOnStartUp.Checked = Properties.GW_Runner.Default.RunGWOnStartup;
            tmrTimer.Interval = Properties.GW_Runner.Default.TimerPeriod;

            //if (chkEnabled.Checked)
            //{
            //    btnStart.Enabled = true;
            //}
            //else
            //{
            //    btnStart.Enabled = false;
            //}
        }

        private void chkEnabled_CheckedChanged(object sender, EventArgs e)
        {
            if (chkEnabled.Checked)
            {
                //btnStart.Enabled = true;
                Properties.GW_Runner.Default.Enabled = true;
            }
            else
            {
                //if(!IsRunning(GWProcessName)) btnStart.Enabled = false;
                Properties.GW_Runner.Default.Enabled = false;
            }

            Properties.GW_Runner.Default.Save();
        }

        private void tmrTimer_Tick(object sender, EventArgs e)
        {
            TimeSpan runTime = (DateTime.Now - started).Duration();

            if (btnStart.Text == "Stop")
            lblRunTime.Text = (runTime.Days.ToString()).PadLeft(3, '0') + "d " +
                              (runTime.Hours.ToString()).PadLeft(2, '0') + ":" +
                              (runTime.Minutes.ToString()).PadLeft(2, '0') + ":" +
                              (runTime.Seconds.ToString()).PadLeft(2, '0');

            CheckGatewayIsRunning();
        }

        private void CheckGatewayIsRunning()
        {
       
             if (chkEnabled.Checked && !IsRunning(GWProcessName) && btnStart.Text == "Stop")
            {
                LogFailure();
                StartGW();
                return;
            }
            if (!chkEnabled.Checked && !IsRunning(GWProcessName))
            {
                SetControlsToNotRunning();
            }
        }

        private void LogFailure()
        {
            logger.Error("GW failed.");
        }

        bool IsRunning(string name) => Process.GetProcessesByName(name).Length > 0;

        Process IBGWRunnerProcess(string name) => Process.GetProcessesByName(name)[0];

        private void frmMain_FormClosed(object sender, FormClosedEventArgs e)
        {
            SaveSettings();

            logger.Info("IB GW Runner closed: " + e.CloseReason);
            logger.Info("***************************************\n");
        }

        private void frmMain_Shown(object sender, EventArgs e)
        {
           
            if (IsRunning(GWProcessName))
            {
                logger.Info("IB GW Runner already running.");
                SetControlsToRunning();

                return;
            }

            if (chkRunGWOnStartUp.Checked)
            {
                logger.Info("IB GW Runner started on startup.");
                StartGW();
            }
        }
    }
}
