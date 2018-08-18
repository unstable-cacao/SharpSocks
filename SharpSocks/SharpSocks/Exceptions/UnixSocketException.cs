using System.Net.Sockets;

namespace SharpSocks.Exceptions
{
    public class UnixSocketException : System.Exception
    {
        public UnixSocketException() {}
        
        public UnixSocketException(SocketError error) : base(error.ToString()) {}
    }
}