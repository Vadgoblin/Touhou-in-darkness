using System;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;
using System.Threading;
using System.IO;

namespace Touhou_in_darkness
{
    public partial class Form1 : Form
    {
        Process game = null;
        public Form1()
        {
            InitializeComponent();
        }

        [DllImport("user32.dll")]
        static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

        [DllImport("USER32.DLL")]
        public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("USER32.DLL")]
        public static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        private void Form1_Load(object sender, EventArgs e)
        {
            Visible = false;
            FormBorderStyle = FormBorderStyle.None;
            WindowState = FormWindowState.Maximized;

            string[] findth = Directory.GetFiles(".", "Touhou??.exe");

            if (findth.Length == 0)
            {
                MessageBox.Show("\"Touhou??.exe\" exe not found");
                Environment.Exit(0);
            }
            Process.Start(findth[0]);


            while (game == null)
            {
                Thread.Sleep(100);

                Process[] processes = Process.GetProcesses();
                foreach (Process proc in processes)
                {
                    var mwts = proc.MainWindowTitle.Split(' ');
                    if (mwts[0].ToLower() == "touhou" && mwts.Length > 2 && proc.StartTime > Process.GetCurrentProcess().StartTime && mwts[1].ToLower()!="community") game = proc;
                }
            }

            Cursor.Hide();
            WindowState = FormWindowState.Minimized;
            Visible = true;
            WindowState = FormWindowState.Maximized;
            SetParent(game.MainWindowHandle, this.Handle);

            try
            {
                Icon = Icon.ExtractAssociatedIcon(game.MainModule.FileName);
            }
            catch { }

            this.Name = game.MainWindowTitle;
            this.Text = game.MainWindowTitle;

            //making borderless
            int style = GetWindowLong(game.MainWindowHandle, -16);
            SetWindowLong(game.MainWindowHandle, -16, (style & ~(0x00800000 | 0x00400000)));

            var ard = CalcGameSizeAndPos();//AutoResData
            MoveWindow(game.MainWindowHandle, ard[0], ard[1], ard[2], ard[3],true);

            var exitcheck = new Thread(new ThreadStart(ExitCheck));
            exitcheck.Start();

            //while (!game.HasExited) Thread.Sleep(100);
            //Application.Exit();
        }

        private void ExitCheck()
        {
            while (!game.HasExited) Thread.Sleep(100);
            Application.Exit();
        }

        private int[] CalcGameSizeAndPos()
        {
            int cWidth = ClientSize.Width;
            int cHeight = ClientSize.Height;

            int gWidth;
            int gHeight;

            if (cWidth/(double)cHeight>4/(double)3)
            {
                gWidth = cHeight / 480 * 640;
                gHeight = cHeight / 480 * 480;
            }
            else
            {
                gWidth = cWidth / 640 * 640;
                gHeight = cWidth / 640 * 480;
            }
            
            

            int x = (cWidth - gWidth) / 2;
            int y = (cHeight - gHeight) / 2;

            return new int[] { x, y, gWidth, gHeight };
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!game.HasExited) game.Kill();
        }

        private void Form1_Activated(object sender, EventArgs e)
        {
            if (game != null) SetForegroundWindow(game.MainWindowHandle);
            WindowState = FormWindowState.Normal;
            WindowState = FormWindowState.Maximized;
        }
    }
}
