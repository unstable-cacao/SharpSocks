using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using SharpSocks.Exceptions;

namespace SharpSocks
{
    public class StandardSocketAdapter : ISocketAdapter
    {
        public Socket Create (AddressFamily domain, SocketType type, ProtocolType protocol)
        {
            return new Socket(domain, type, protocol);
        }

        public void Connect (Socket socket, EndPoint address)
        {
            socket.Connect(address);
        }

        public void Bind(Socket socket, EndPoint address)
        {
            socket.Bind(address);
        }

        public void Listen(Socket socket)
        {
            socket.Listen((int)SocketOptionName.MaxConnections);
        }

        public Socket Accept(Socket socket)
        {
            return socket.Accept();
        }

        public void SetNonBlocking(Socket socket)
        {
            socket.Blocking = false;
        }

        public void Close(Socket socket)
        {
            socket.Close();
        }

        public string Read(Socket socket, int length)
        {
            var buffer = new byte[length];

            try
            {
                socket.Receive(buffer, length, SocketFlags.None);
            }
            catch(SocketException ex)
            {
                throw new UnixSocketException(ex.SocketErrorCode);
            }
            catch (ObjectDisposedException ex)
            {
                throw new UnixSocketException(SocketError.Shutdown);
            }

            string readData = Encoding.UTF8.GetString(buffer);

            if (string.IsNullOrWhiteSpace(readData))
            {
                try
                {
                    socket.Receive(buffer, length, SocketFlags.Partial);
                }
                catch(SocketException ex)
                {
                    throw new UnixSocketException(ex.SocketErrorCode);
                }
                catch (ObjectDisposedException ex)
                {
                    throw new UnixSocketException(SocketError.Shutdown);
                }

                readData = Encoding.UTF8.GetString(buffer);
            }

            return readData.Trim(' ');
        }

        public void Write (Socket socket, string message)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(message);

            int bytesSent = 0;
            try
            {
                bytesSent = socket.Send(bytes);
            }
            catch(SocketException ex)
            {
                throw new UnixSocketException(ex.SocketErrorCode);
            }
            catch (ObjectDisposedException ex)
            {
                throw new UnixSocketException(SocketError.Shutdown);
            }

            if (bytesSent != bytes.Length)
                throw new UnixSocketException(SocketError.MessageSize);
        }
    }
}