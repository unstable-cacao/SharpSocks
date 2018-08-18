using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using SharpSocks.Exceptions;

namespace SharpSocks
{
    public class Client : IClient
    {
        private const double BIG_FLOAT = 5000000000.0;

        private static string[] ENDLINE_CHARS = {"\n", "\r"};

        private string File;

        private IClientPlugin Plugin;
        
        private string Buffer;
        
        private ISocketAdapter Connection;
        
        private Socket IoSocket;
        
        private List<Socket> AllSockets;


        private bool ReadIntoInternalBuffer(int maxLength = 1024, string data = null)
        {
            this.ValidateOpen();
            
            try
            {
                data = this.Connection.Read(this.IoSocket, maxLength);
            }
            catch (ConnectionLostException ex)
            {
                this.Close();
                throw ex;
            }
            
            if (String.IsNullOrEmpty(data))
                return false;

            this.Buffer = data;
            return true;
        }
        
        private string GetFromBuffer(int maxLength)
        {
            if (String.IsNullOrEmpty(this.Buffer))
                return null;
            
            string result = null;

            if (this.Buffer.Length < maxLength)
            {
                result = this.Buffer;
                this.Buffer = "";
            }
            else
            {
                result = this.Buffer.Substring(0, maxLength);
                this.Buffer = this.Buffer.Substring(maxLength);
            }
            
            return result;
        }
        
        private void ValidateOpen()
        {
            if (this.IsClosed())
            {
                throw new NoConnectionException();
            }
        }
        
        private void ValidateClosed()
        {
            if (!this.IsClosed())
            {
                throw new ConnectionAlreadyOpenException();
            }
        }
        
        private void ValidateFile()
        {
            if (String.IsNullOrEmpty(this.File))
            {
                throw new FatalUnixSocksException("File not set");
            }
        }
        
        private void ValidateTimeout(ref double? timeout)
        {
            if (timeout != null && timeout < 0)
                throw new FatalUnixSocksException("Timeout must be 0 or bigger, or null");

            if (timeout is null)
                timeout = BIG_FLOAT;
        }
        
        private void ValidateLength(int? length)
        {
            if (length != null && length <= 0)
                throw new FatalUnixSocksException("Length must be null or bigger than 0");
        }
        
        public Client(ISocketAdapter connection, string file = null, IClientPlugin plugin = null)
        {
            this.Connection = connection ?? new StandartSocketAdapter();
            this.File = file;
            this.Plugin = plugin;
        }

        ~Client ()
        {
            this.Close();
        }

        public void SetFile(string path)
        {
            this.File = path;
        }

        public string GetFile()
        {
            return String.IsNullOrEmpty(this.File) ? "" : this.File;
        }

        
        public bool TryConnect()
        {
            try
            {
                this.Connect();
            }
            catch (UnixSocketException)
            {
                return false;
            }
            
            return true;
        }

        public void Connect()
        {
            this.ValidateClosed();
            var conn = this.Connection.Create(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified);

            this.ValidateFile();
            this.Connection.Connect(conn, this.File);

            this.IoSocket = conn;
            this.AllSockets = new List<Socket> { conn };
        }

        public void Accept(double? timeoutMS = null)
        {
            this.ValidateClosed();
            var conn = this.Connection.Create(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified);

            this.ValidateFile();

            this.Connection.Bind(conn, this.File);
            this.Connection.Listen(conn);

            if (timeoutMS is null)
            {
                this.IoSocket = this.Connection.Accept(conn);
                this.AllSockets = new List<Socket> { this.IoSocket, conn };
            }
            else
            {
                var timeoutUntil = DateTime.Now.AddMilliseconds(timeoutMS.Value);
                this.Connection.SetNonBlocking(conn);

                while (DateTime.Now < timeoutUntil)
                {
                    var client = this.Connection.Accept(conn);

                    if (client != null)
                    {
                        this.IoSocket = client;
                        this.AllSockets = new List<Socket> {client, conn};
                    }
                }
            }

            if (this.IoSocket != null)
                this.Connection.SetNonBlocking(this.IoSocket);
        }

        public bool TryAccept(double? timeout = null)
        {
            try
            {
                this.Accept(timeout);
            }
            catch (UnixSocketException)
            {
                return false;
            }
            return true;
        }

        public void Close()
        {
            foreach (var socket in this.AllSockets)
            {
                this.Connection.Close(socket);
            }
            
            if (this.File != null && System.IO.File.Exists(this.File))
            {
                System.IO.File.Delete(this.File);
            }
            
            this.Buffer = "";
            this.AllSockets = null; // [];
            this.IoSocket = null;
        }
        
        public bool IsOpen()
        {
            return this.IoSocket != null;
        }

        public bool IsClosed()
        {
            return this.IoSocket == null;
        }
        
        public Socket GetSocket()
        {
            return this.IoSocket;
        }
        
        
        public bool HasInput()
        {
            if (String.IsNullOrEmpty(Buffer))
            {
                this.ReadIntoInternalBuffer();
            }

            return !(String.IsNullOrEmpty(this.Buffer));
        }
        
        public string Read(int? maxLength = 1024, double? timeout = null)
        {
            this.ValidateOpen();
            this.ValidateTimeout(ref timeout);
            this.ValidateLength(maxLength);
            
            //double timeoutTime = DateTime.UtcNow.Ticks/TimeSpan.TicksPerMillisecond + timeout.Value;
            DateTimeOffset timeoutTime = DateTime.UtcNow.AddMilliseconds(timeout.Value);

            bool isRunning = true;
            while (isRunning)
            {
                if (this.ReadIntoInternalBuffer(maxLength.Value))
                    break;
                
                if (DateTime.UtcNow.CompareTo(timeoutTime) > 0)
                    break;
                
                Thread.Sleep(10);
            }
            
            var result = this.GetFromBuffer(maxLength.Value);
            
            if (this.Plugin != null)
                this.Plugin.Read(this, result);
            
            return result;
        }

        public string ReadExactly(int? length = 1024, double? timeout = null)
        {
            this.ValidateOpen();
            this.ValidateTimeout(ref timeout);
            this.ValidateLength(length);
            
            DateTimeOffset timeoutTime = DateTime.UtcNow.AddMilliseconds(timeout.Value);
            bool isRunning = true;
            
            while (isRunning)
            {
                this.ReadIntoInternalBuffer(length.Value);
                
                if (DateTime.UtcNow.CompareTo(timeoutTime) > 0)
                    break;

                if (this.Buffer.Length >= length.Value)
                    break;
                
                Thread.Sleep(10);
            }
            
            string result = null;

            if (this.Buffer.Length < length.Value)
                result = null;
            else
                result = this.GetFromBuffer(length.Value);
            
            if (this.Plugin != null)
                this.Plugin.Read(this, result);
            
            return result;
        }

        public string ReadLine(double? timeout = null, int? maxLength = null)
        {
		    return this.ReadUntil(ENDLINE_CHARS, timeout, maxLength);
        }

        public string ReadUntil(string[] stop, double? timeout = null, int? maxLength = null)
        {
            this.ValidateOpen();
            
            if (stop is null || stop.Length <= 0)
                throw new FatalUnixSocksException("Stop parameter required");
            
            this.ValidateTimeout(ref timeout);
            this.ValidateLength(maxLength);
            
            string result = null;
            int stopPosition = -1;

            if (maxLength is null)
            {
                bool breakWhenEmpty = false;
                
                if (timeout.Value == 0)
                {
                    timeout = 5000000;
                    breakWhenEmpty = true;
                }
                
                DateTimeOffset timeoutTime = DateTime.UtcNow.AddMilliseconds(timeout.Value);
                bool isRunning = true;
                
                while (isRunning)
                {
                    string readFromSocket = null;

                    this.ReadIntoInternalBuffer(1024, readFromSocket);
                        
                    foreach (var stopString in stop)
                    {
                        int position = this.Buffer.IndexOf(stopString);
                        //position = strpos($this->buffer, $stopString);
                        
                        if (position >= 0)
                        {
                            // TODO stopPosition or stopString ?
                            stopPosition = stopPosition < 0 ? position : Math.Min(stopPosition, position);
						    //$stopPosition = is_null($stopPosition) ? $position : min($stopString, $position);
                        }
                    }
                    
                    if (String.IsNullOrEmpty(readFromSocket) && breakWhenEmpty)
                        break;
                    
                    if (DateTime.UtcNow.CompareTo(timeoutTime) > 0)
                        break;
                    
                    if (stopPosition >= 0)
                        break;
                    
                    Thread.Sleep(10);
                }
            }
            else
            {
                DateTimeOffset timeoutTime = DateTime.UtcNow.AddMilliseconds(timeout.Value);
                bool isRunning = true;
                
                while (isRunning)
                {
                    this.ReadIntoInternalBuffer(maxLength.Value);
                    
                    foreach (var stopString in stop)
                    {
                        int position = this.Buffer.IndexOf(stopString);
                        
                        if (position >= 0)
                        {
                            stopPosition = stopPosition < 0 ? position : Math.Min(stopPosition, position);
                        }
                    }
                    
                    if (DateTime.UtcNow.CompareTo(timeoutTime) > 0)
                        break;
                    
                    if (this.Buffer.Length >= maxLength.Value)
                        break;
                    
                    if (stopPosition >= 0)
                        break;
                    
                    Thread.Sleep(10);
                }
            }
		
            // Both $maxLength and $stopPosition are null
            if (maxLength is null && stopPosition < 0)
                result = null;
            // Both $maxLength and $stopPosition are NOT null
            else if (stopPosition >= 0 && maxLength.HasValue && stopPosition > maxLength.Value)
                result = this.GetFromBuffer(maxLength.Value);
            // $maxLength is null, $stopPosition is NOT null
            else if (stopPosition >= 0)
                result = this.GetFromBuffer(stopPosition + 1);
            // $maxLength is NOT null, $stopPosition is null
            else 
                result = this.GetFromBuffer(maxLength.Value);
            
            if (this.Plugin != null)
                this.Plugin.Read(this, result);
            
            return result;
        }
        
        public void Write(string input)
        {
            this.ValidateOpen();
            
            try
            {
                this.Connection.Write(this.IoSocket, input);
            }
            catch (ConnectionLostException)
            {
                this.Close();
            }
            
            if (this.Plugin != null)
                this.Plugin.Write(this, input);
        }

        public void WriteLine(string input)
        {
		    this.Write(input + Environment.NewLine);
        }

    }
}