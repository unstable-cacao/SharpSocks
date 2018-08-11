using System.Net.Sockets;

namespace SharpSocks.Exceptions
{
    public class ConnectionLostException : UnixSocketException
    {
        public ConnectionLostException(SocketError error) : base(error)
        {
        }
    }
}