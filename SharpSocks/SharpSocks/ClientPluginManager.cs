using System.Collections.Generic;

namespace SharpSocks
{
    public class ClientPluginManager : IClientPlugin
    {
        private List<IClientPlugin> plugins;
        
        public ClientPluginManager(List<IClientPlugin> plugins = null)
        {
            if (plugins is null)
                plugins = new List<IClientPlugin>();

            this.plugins = plugins;
        }
        
        
        public void Connected(IClient client)
        {
            foreach (var plugin in this.plugins)
            {
                plugin.Connected(client);
            }
        }
        
        public void Disconnected(IClient client)
        {
            foreach (var plugin in this.plugins)
            {
                plugin.Disconnected(client);
            }
        }
        
        public void Read(IClient client, string input)
        {
            foreach (var plugin in this.plugins)
            {
                plugin.Read(client, input);
            }
        }
        
        public void Write(IClient client, string output)
        {
            foreach (var plugin in this.plugins)
            {
                plugin.Write(client, output);
            }
        }
    }
}