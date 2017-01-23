using System.Threading.Tasks;
using NServiceBus.Features;
using Prometheus;

namespace NServiceBus.Prometheus
{
    class UpDown : FeatureStartupTask
    {
        private string endpointName;
        private string localAddress;
        private Gauge endpointUpDown;

        public UpDown(string endpointName, string localAddress)
        {
            this.localAddress = localAddress;
            this.endpointName = endpointName;

            endpointUpDown = Metrics.CreateGauge("nservicebus_endpoint_up", "Messages sent to the error queue",
                new[] { "name", "local_address" });
        }

        protected override Task OnStart(IMessageSession session)
        {
            endpointUpDown.Labels(endpointName, localAddress).Set(1d);
            return Task.CompletedTask;
        }

        protected override Task OnStop(IMessageSession session)
        {
            endpointUpDown.Labels(endpointName, localAddress).Set(0d);
            return Task.CompletedTask;
        }
    }
}