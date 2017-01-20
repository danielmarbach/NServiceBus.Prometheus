namespace NServiceBus.Prometheus
{
    class Settings
    {
        public Settings()
        {
            ServerSettings = new ServerSettings();
        }

        public ServerSettings ServerSettings { get; }
    }
}