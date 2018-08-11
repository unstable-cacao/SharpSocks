using System;
using System.Collections.Generic;
using System.Net.Sockets;
using SharpSocks.Exceptions;

namespace SharpSocks
{
    public class Client : IClient
    {
        private const long BIG_FLOAT = 5000000000.0;

        private string file;

        private IClientPlugin plugin;
        
        private string buffer;
        
        private ISocketAdapter conn;
        
        private Socket ioSocket;
        
        private List<Socket> allSockets;
        
        public Client(ISocketAdapter connection, string file = null, IClientPlugin plugin = null)
        {
            this.conn = connection;
        }

        ~Client ()
        {
            this.close();
        }

        public void setFile(string path)
        {
            this.file = path;
        }

        public string getFile()
        {
            return String.IsNullOrEmpty(this.file) ? "" : this.file;
        }

        
        public bool tryConnect()
        {
            try
            {
                this.connect();
            }
            catch (UnixSocketException ex)
            {
                return false;
            }
            
            return true;
        }

        public void connect()
        {
            this.validateClosed();
            var conn = this.conn.Create(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified);

            this.validateFile();
            this.conn.Connect(conn, this.file);

            this.ioSocket = conn;
            this.allSockets = new List<Socket> { conn };
        }

        public void accept(long? timeoutMS = null)
        {
            this.validateClosed();
            var conn = this.conn.Create(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified);

            this.validateFile();

            this.conn.Bind(conn, this.file);
            this.conn.Listen(conn);

            if (timeoutMS is null)
            {
                this.ioSocket = this.conn.Accept(conn);
                this.allSockets = new List<Socket> { this.ioSocket, conn };
            }
            else
            {
                var timeoutUntil = DateTime.Now.AddMilliseconds(timeoutMS.Value);
                this.conn.SetNonBlocking(conn);

                while (DateTime.Now < timeoutUntil)
                {
                    var client = this.conn.Accept(conn);

                    if (client != null)
                    {
                        this.ioSocket = client;
                        this.allSockets = new List<Socket> {client, conn};
                    }
                }
            }

            if (this.ioSocket != null)
                this.conn.SetNonBlocking(this.ioSocket);
        }

        public void tryAccept(long? timeout = null)
        {
            
        }

        void close();
        
        bool isOpen();
        bool isClosed();
        
        Socket getSocket();
        
        
        bool hasInput();
        
        string read(int? maxLength = 1024, long? timeout = null);
        string readExactly(int? length = 1024, long? timeout = null);
        string readLine(long? timeout = null, int? maxLength = null);

        string readUntil(string stop, long? timeout = null, int? maxLength = null);
        
        void write(string input);
        void writeLine(string input);

    }
}