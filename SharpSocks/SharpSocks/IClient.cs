using System.Net.Sockets;

namespace SharpSocks
{
    public interface IClient
    {
        void SetFile(string path);
        string GetFile();
        
        bool TryConnect();
        void Connect();
        void Accept(double? timeout = null);
        bool TryAccept(double? timeout = null);
        void Close();
        
        bool IsOpen();
        bool IsClosed();
        
        Socket GetSocket();
        
        
        bool HasInput();
        
        string Read(int? maxLength = 1024, double? timeout = null);
        string ReadExactly(int? length = 1024, double? timeout = null);
        string ReadLine(double? timeout = null, int? maxLength = null);

        string ReadUntil(string[] stop, double? timeout = null, int? maxLength = null);
        
        void Write(string input);
        void WriteLine(string input);
    }
}