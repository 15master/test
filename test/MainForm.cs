using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace RetroConsoleApp
{
    public class MainForm : Form
    {
        private const int WH_KEYBOARD_LL = 13;
        private const int WH_MOUSE_LL = 14;
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_SYSKEYDOWN = 0x0104;
        private const int WM_LBUTTONDOWN = 0x0201;
        private const int WM_RBUTTONDOWN = 0x0204;
        private const int WM_MBUTTONDOWN = 0x0207;

        private readonly TextBox _output;
        private IntPtr _keyboardHookId = IntPtr.Zero;
        private IntPtr _mouseHookId = IntPtr.Zero;
        private LowLevelKeyboardProc _keyboardProc;
        private LowLevelMouseProc _mouseProc;

        public MainForm()
        {
            // Configure the main window
            this.Text = "Retro Console";
            this.Size = new Size(500, 350);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.Black;

            // Create the console output area
            _output = new TextBox
            {
                Multiline = true,
                ReadOnly = true,
                Dock = DockStyle.Fill,
                BackColor = Color.Black,
                ForeColor = Color.FromArgb(0, 255, 0), // Bright matrix green
                Font = new Font("Consolas", 11),
                BorderStyle = BorderStyle.None,
                ScrollBars = ScrollBars.Vertical,
                WordWrap = false
            };

            this.Controls.Add(_output);

            // Display the startup message
            WriteLine("Application Started... Listening for local input:");

            // Install global low-level hooks
            _keyboardProc = KeyboardHookCallback;
            _keyboardHookId = SetWindowsHookEx(WH_KEYBOARD_LL, _keyboardProc, GetModuleHandle(Process.GetCurrentProcess().MainModule.ModuleName), 0);

            _mouseProc = MouseHookCallback;
            _mouseHookId = SetWindowsHookEx(WH_MOUSE_LL, _mouseProc, GetModuleHandle(Process.GetCurrentProcess().MainModule.ModuleName), 0);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            // Clean up hooks when the app closes
            if (_keyboardHookId != IntPtr.Zero) UnhookWindowsHookEx(_keyboardHookId);
            if (_mouseHookId != IntPtr.Zero) UnhookWindowsHookEx(_mouseHookId);
            base.OnFormClosing(e);
        }

        private IntPtr KeyboardHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && (wParam == (IntPtr)WM_KEYDOWN || wParam == (IntPtr)WM_SYSKEYDOWN))
            {
                int vkCode = Marshal.ReadInt32(lParam);
                Keys key = (Keys)vkCode;

                if (key >= Keys.A && key <= Keys.Z)
                {
                    WriteLine(key.ToString());
                }
                else if (key >= Keys.D0 && key <= Keys.D9)
                {
                    WriteLine(key.ToString());
                }
                else if (key >= Keys.NumPad0 && key <= Keys.NumPad9)
                {
                    WriteLine(key.ToString());
                }
                else if (key == Keys.Space) WriteLine("Space");
                else if (key == Keys.Enter) WriteLine("Enter");
                else if (key == Keys.Back) WriteLine("Back");
                else if (key == Keys.Tab) WriteLine("Tab");
            }

            return CallNextHookEx(_keyboardHookId, nCode, wParam, lParam);
        }

        private IntPtr MouseHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                int msg = wParam.ToInt32();

                if (msg == WM_LBUTTONDOWN)
                {
                    WriteLine("Left Click");
                }
                else if (msg == WM_RBUTTONDOWN)
                {
                    WriteLine("Right Click");
                }
                else if (msg == WM_MBUTTONDOWN)
                {
                    WriteLine("Middle Click");
                }
            }

            return CallNextHookEx(_mouseHookId, nCode, wParam, lParam);
        }

        private void WriteLine(string text)
        {
            // Marshals to UI thread to avoid cross-thread access exceptions
            if (_output.IsHandleCreated)
            {
                _output.BeginInvoke(new Action(() =>
                {
                    _output.AppendText(text + Environment.NewLine);
                    _output.SelectionStart = _output.Text.Length;
                    _output.ScrollToCaret();
                }));
            }
        }

        #region Win32 API imports

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);
        private delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelMouseProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        #endregion
    }
}
