using System;

namespace MainDen.Modules.IO
{
    public class EchoException : Exception
    {
        public EchoException() : base() { }
        
        public EchoException(string message) : base(message) { }
        
        public EchoException(string message, Exception innerException) : base(message, innerException) { }
    }

    public class EchoWriteException : EchoException
    {
        public EchoWriteException() : base()
        {
            _outputs = Echo.Outputs.None;
        }
        
        public EchoWriteException(string message) : base(message)
        {
            _outputs = Echo.Outputs.None;
        }
        
        public EchoWriteException(string message, Exception innerException) : base(message, innerException)
        {
            _outputs = Echo.Outputs.None;
        }
        
        public EchoWriteException(Echo.Outputs outputs, string message) : base(message)
        {
            _outputs = outputs;
        }
        
        public EchoWriteException(Echo.Outputs outputs, string message, Exception innerException) : base(message, innerException)
        {
            _outputs = outputs;
        }
        
        public EchoWriteException(Echo.Outputs outputs) : this(outputs, $"Unable write to {outputs}.") { }
        
        public EchoWriteException(Echo.Outputs outputs, Exception innerException) : this(outputs, $"Unable write to {outputs}.", innerException) { }
        
        private readonly Echo.Outputs _outputs;
        
        public Echo.Outputs Outputs
        {
            get => _outputs;
        }
    }

    public class EchoSettingsException : EchoException
    {
        public EchoSettingsException() : base() { }
        
        public EchoSettingsException(string message) : base(message) { }
        
        public EchoSettingsException(string message, Exception innerException) : base(message, innerException) { }
    }

    public class Echo
    {
        [Flags]
        public enum Outputs
        {
            None = 0,
            Custom = 1,
            Console = 2,
        }

        private readonly object _lSettings = new object();

        private string _MessageFormat = "{0}\n";

        private bool _WriteToCustom = true;

        private bool _WriteToConsole = true;

        private bool _AllowWriteNullMessages = false;

        private bool _AutoDisableWriteOutputs = true;

        private bool _IgnoreWriteExceptions = false;

        private Action<string> _Custom;

        public string MessageFormat
        {
            get
            {
                lock (_lSettings)
                    return _MessageFormat;
            }
            set
            {
                lock (_lSettings)
                {
                    if (value is null)
                        throw new ArgumentNullException();

                    try
                    {
                        GetEchoMessage(value, "");

                        _MessageFormat = value;
                    }
                    catch (Exception e)
                    {
                        throw new EchoSettingsException("Invalid message format.", e);
                    }
                }
            }
        }

        public bool WriteToCustom
        {
            get
            {
                lock (_lSettings)
                    return _WriteToCustom;
            }
            set
            {
                lock (_lSettings)
                    _WriteToCustom = value;
            }
        }

        public bool WriteToConsole
        {
            get
            {
                lock (_lSettings)
                    return _WriteToConsole;
            }
            set
            {
                lock (_lSettings)
                    _WriteToConsole = value;
            }
        }

        public bool AllowWriteNullMessages
        {
            get
            {
                lock (_lSettings)
                    return _AllowWriteNullMessages;
            }
            set
            {
                lock (_lSettings)
                    _AllowWriteNullMessages = value;
            }
        }

        public bool AutoDisableWriteOutputs
        {
            get
            {
                lock (_lSettings)
                    return _AutoDisableWriteOutputs;
            }
            set
            {
                lock (_lSettings)
                    _AutoDisableWriteOutputs = value;
            }
        }

        public bool IgnoreWriteExceptions
        {
            get
            {
                lock (_lSettings)
                    return _IgnoreWriteExceptions;
            }
            set
            {
                lock (_lSettings)
                    _IgnoreWriteExceptions = value;
            }
        }

        public event Action<string> Custom
        {
            add
            {
                lock (_lSettings)
                {
                    if (value is null)
                        throw new ArgumentNullException();

                    _Custom += value;
                }
            }
            remove
            {
                lock (_lSettings)
                {
                    if (value is null)
                        throw new ArgumentNullException();

                    _Custom -= value;
                }
            }
        }

        public string GetEchoMessage(string message)
        {
            lock (_lSettings)
            {
                if (message is null)
                    throw new ArgumentNullException(nameof(message));

                return GetEchoMessage(_MessageFormat, message);
            }
        }

        private void WriteBase(string echoMessage)
        {
            lock (_lSettings)
            {
                if (echoMessage is null)
                    throw new ArgumentNullException(nameof(echoMessage));

                Outputs outputs = Outputs.None;

                if (_WriteToCustom)
                    try
                    {
                        _Custom?.Invoke(echoMessage);
                    }
                    catch
                    {
                        outputs |= Outputs.Custom;
                    }

                if (_WriteToConsole)
                    try
                    {
                        Console.Write(echoMessage);
                    }
                    catch
                    {
                        outputs |= Outputs.Console;
                    }

                if (_AutoDisableWriteOutputs)
                {
                    if (outputs.HasFlag(Outputs.Custom))
                        _WriteToCustom = false;
                    if (outputs.HasFlag(Outputs.Console))
                        _WriteToConsole = false;
                }

                if (!_IgnoreWriteExceptions)
                    if (outputs != Outputs.None)
                        throw new EchoWriteException(outputs);
            }
        }

        public void WriteCustomEchoMessage(string echoMessage)
        {
            lock (_lSettings)
            {
                if (echoMessage is null)
                    throw new ArgumentNullException(nameof(echoMessage));

                WriteBase(echoMessage);
            }
        }

        public void Write(string message)
        {
            lock (_lSettings)
            {
                if (message is null)
                    if (_AllowWriteNullMessages)
                        message = "null";
                    else
                        throw new ArgumentNullException(nameof(message));

                string echoMessage = GetEchoMessage(message);
                WriteBase(echoMessage);
            }
        }

        private static readonly object _lStaticSettings = new object();

        private static Echo _Default;

        public static Echo Default
        {
            get
            {
                lock (_lStaticSettings)
                    return _Default ?? (_Default = new Echo());
            }
        }

        public static string GetEchoMessage(string messageFormat, string message)
        {
            if (messageFormat is null)
                throw new ArgumentNullException(nameof(messageFormat));
            if (message is null)
                throw new ArgumentNullException(nameof(message));

            return string.Format(
                messageFormat,
                message);
        }
    }
}