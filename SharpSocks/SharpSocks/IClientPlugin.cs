namespace SharpSocks
{
    public interface IClientPlugin
    {
        void Connected(IClient client);
        void Disconnected(IClient client);
        void Read(IClient client, string input);
        void Write(IClient client, string output);
    }
}