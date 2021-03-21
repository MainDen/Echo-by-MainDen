using System;

namespace MainDen.Modules
{
    public class Echo
    {
        public class EchoException : Exception
        {
            public EchoException() : base() { }
            public EchoException(string message) : base(message) { }
            public EchoException(string message, Exception innerException) : base(message, innerException) { }
        }
        public class EchoWriteException : EchoException
        {
            public EchoWriteException() : base() { }
            public EchoWriteException(string message) : base(message) { }
            public EchoWriteException(string message, Exception innerException) : base(message, innerException) { }
        }
        public class EchoSettingsException : EchoException
        {
            public EchoSettingsException() : base() { }
            public EchoSettingsException(string message) : base(message) { }
            public EchoSettingsException(string message, Exception innerException) : base(message, innerException) { }
        }
        public Echo() { }
        [Flags]
        private enum Output
        {
            None = 0,
            Custom = 1,
            Console = 2,
        }
        private readonly object lSettings = new object();
        private string _MessageFormat = "{0}\n";
        public string MessageFormat
        {
            get
            {
                lock (lSettings)
                    return _MessageFormat;
            }
            set
            {
                if (value is null)
                    throw new ArgumentNullException(nameof(value));
                try
                {
                    GetMessage(value, "Message");
                    lock (lSettings)
                        _MessageFormat = value;
                }
                catch (Exception e) { throw new EchoSettingsException("Invalid message format.", e); }
            }
        }
        public string GetMessage(string message)
        {
            if (message is null)
                throw new ArgumentNullException(nameof(message));
            lock (lSettings)
                return string.Format(
                    _MessageFormat,
                    message);
        }
        private bool _WriteToCustom = true;
        public bool WriteToCustom
        {
            get
            {
                lock (lSettings)
                    return _WriteToCustom;
            }
            set
            {
                lock (lSettings)
                    _WriteToCustom = value;
            }
        }
        private bool _WriteToConsole = true;
        public bool WriteToConsole
        {
            get
            {
                lock (lSettings)
                    return _WriteToConsole;
            }
            set
            {
                lock (lSettings)
                    _WriteToConsole = value;
            }
        }
        private bool _AllowWriteNullMessages = false;
        public bool AllowWriteNullMessages
        {
            get
            {
                lock (lSettings)
                    return _AllowWriteNullMessages;
            }
            set
            {
                lock (lSettings)
                    _AllowWriteNullMessages = value;
            }
        }
        private Action<string> _CustomWrite;
        public event Action<string> CustomWrite
        {
            add
            {
                if (value is null)
                    throw new ArgumentNullException(nameof(value));
                lock (lSettings)
                    _CustomWrite += value;
            }
            remove
            {
                if (value is null)
                    throw new ArgumentNullException(nameof(value));
                lock (lSettings)
                    _CustomWrite -= value;
            }
        }
        private Output output;
        private void WriteBase(string echoMessage)
        {
            if (echoMessage is null)
                if (_AllowWriteNullMessages)
                    echoMessage = "null";
                else
                    throw new ArgumentNullException(nameof(echoMessage));
            lock (lSettings)
            {
                output = Output.None;
                if (WriteToCustom)
                    try
                    {
                        _CustomWrite?.Invoke(echoMessage);
                    }
                    catch { output |= Output.Custom; }
                if (WriteToConsole)
                    try
                    {
                        Console.Write(echoMessage);
                    }
                    catch { output |= Output.Console; }
                if (output != Output.None)
                    throw new EchoWriteException($"Unable write to {output}.");
            }
        }
        public void WriteCustom(string echoMessage)
        {
            if (echoMessage is null)
                if (_AllowWriteNullMessages)
                    echoMessage = "null";
                else
                    throw new ArgumentNullException(nameof(echoMessage));
            lock (lSettings)
            {
                WriteBase(echoMessage);
            }
        }
        public void Write(string message)
        {
            if (message is null)
                if (_AllowWriteNullMessages)
                    message = "null";
                else
                    throw new ArgumentNullException(nameof(message));
            lock (lSettings)
            {
                string echoMessage = GetMessage(message);
                WriteBase(echoMessage);
            }
        }
        public static string GetMessage(string messageFormat, string message)
        {
            if (messageFormat is null)
                throw new ArgumentNullException(nameof(messageFormat));
            if (message is null)
                throw new ArgumentNullException(nameof(message));
            return string.Format(
                messageFormat,
                message);
        }
        private static readonly Echo _Default = new Echo();
        public static Echo Default
        {
            get => _Default;
        }
    }
}