namespace NServiceBus.Prometheus
{
    public class ServerSettings
    {
        internal string Host { get; set; }
        internal int Port { get; set; }
        internal bool Use { get; set; }

        public ServerSettings HostAddress(string hostAddress)
        {
            Host = hostAddress;
            return this;
        }

        public ServerSettings HostPort(int port)
        {
            Port = port;
            return this;
        }
    }
}