using NServiceBus.Configuration.AdvanceExtensibility;

namespace NServiceBus.Prometheus
{
    public static class PrometheusConfigurationExtensions
    {
        public static PrometheusSettings Prometheus(this EndpointConfiguration configuration)
        {
            return new PrometheusSettings(configuration.GetSettings());
        }
    }
}