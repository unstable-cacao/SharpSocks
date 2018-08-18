using System.Collections.Generic;

namespace SharpSocks
{
    public class ClientBuilder : IClientBuilder
    {
        private ClientPluginManager manager = null;
        
        private List<IClientPlugin> plugins = new List<IClientPlugin>();

	
        private void CreateManager()
        {
            this.manager = new ClientPluginManager(this.plugins);
        }
        
        
        public void AddPlugin(IClientPlugin plugin)
        {
            this.manager = null;
            this.plugins.Add(plugin);
        }
        
        public IClient GetClient(ISocketAdapter socket = null)
        {
            if (this.manager is null)
                this.CreateManager();
            
            return new Client(socket, null, this.manager);
        }
    }
}