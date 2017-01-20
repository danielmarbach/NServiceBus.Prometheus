using System;
using NServiceBus.Configuration.AdvanceExtensibility;
using NServiceBus.Settings;

namespace NServiceBus.Prometheus
{
    public class PrometheusSettings : ExposeSettings
    {
        private Settings prometheus;

        public PrometheusSettings(SettingsHolder settings) : base(settings)
        {
            if (!settings.TryGet(out this.prometheus))
            {
                prometheus = new Settings();
                settings.Set<Settings>(prometheus);
            }
        }

        public PrometheusSettings Server(Action<ServerSettings> customizations)
        {
            prometheus.ServerSettings.Use = true;
            customizations(prometheus.ServerSettings);
            return this;
        }
    }
}