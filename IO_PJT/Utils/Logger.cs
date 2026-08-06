using System;
using System.Windows.Forms;

namespace IO_PJT.Utils
{
    public class Logger
    {
        private readonly RichTextBox _logControl;

        public Logger(RichTextBox logControl)
        {
            _logControl = logControl ?? throw new ArgumentNullException(nameof(logControl));
        }

        public void Log(string message)
        {
            if (_logControl.IsDisposed) return;

            if (_logControl.InvokeRequired)
            {
                _logControl.Invoke(new Action<string>(Log), message);
                return;
            }

            string timestamp = DateTime.Now.ToString("HH:mm:ss");
            _logControl.AppendText($"[{timestamp}] {message}\n");
            _logControl.ScrollToCaret();
            Application.DoEvents();
        }

        public void Info(string message) => Log($"ℹ️ {message}");
        public void Success(string message) => Log($"✅ {message}");
        public void Warning(string message) => Log($"⚠️ {message}");
        public void Error(string message) => Log($"❌ {message}");
        public void Debug(string message) => Log($"🔍 {message}");
    }
}