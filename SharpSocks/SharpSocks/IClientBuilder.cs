namespace SharpSocks
{
    public interface IClientBuilder
    {
        void AddPlugin(IClientPlugin plugin);
        
        IClient GetClient(ISocketAdapter socket = null);
    }
}