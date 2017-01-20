using System;
using System.Diagnostics;
using System.Threading.Tasks;
using NServiceBus.Features;
using Prometheus;

namespace NServiceBus.Prometheus
{
    class PrometheusFeature : Feature
    {
        private MetricServer server;

        public PrometheusFeature()
        {
            EnableByDefault();
        }

        protected override void Setup(FeatureConfigurationContext context)
        {
            server = new MetricServer("localhost", 1234);

            context.Pipeline.Register(new InspectionBehavior(context.Settings.LocalAddress()), "Behavior which inspects metrics for prometheus.");

            context.RegisterStartupTask(new NotificationsObversation(context.Settings.Get<Notifications>()));
            context.RegisterStartupTask(new UpDown(context.Settings.EndpointName(), context.Settings.LocalAddress()));

            context.RegisterStartupTask(new MetricServerStartAndStop(server));
        }

        class MetricServerStartAndStop : FeatureStartupTask
        {
            private MetricServer server;

            public MetricServerStartAndStop(MetricServer server)
            {
                this.server = server;
            }

            protected override Task OnStart(IMessageSession session)
            {
                server.Start();
                return Task.CompletedTask;
            }

            protected override async Task OnStop(IMessageSession session)
            {
                // await for scraping (cheating)
                await Task.Delay(TimeSpan.FromSeconds(5));
                server.Stop();
            }
        }
    }
}
