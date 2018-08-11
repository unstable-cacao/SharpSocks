using System.Net.Sockets;

namespace SharpSocks
{
    public interface IClient
    {
        void setFile(string path);
        string getFile();
        
        bool tryConnect();
        void connect();
        void accept(long? timeout = null);
        void tryAccept(long? timeout = null);
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