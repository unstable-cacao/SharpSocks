using System.Net;
using System.Net.Sockets;

namespace SharpSocks
{
    public interface ISocketAdapter
    {
        Socket Create (AddressFamily domain, SocketType type, ProtocolType protocol);

        void Connect (Socket socket, string file);

		void Bind(Socket socket, string file);

        void Listen(Socket socket);

        Socket Accept(Socket socket);

        void SetNonBlocking(Socket socket);

        void Close(Socket socket);

        string Read(Socket socket, int length);

        void Write (Socket socket, string message);
    }
}