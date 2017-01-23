using System;
using System.Diagnostics;
using System.Threading.Tasks;
using NServiceBus.Pipeline;
using Prometheus;

namespace NServiceBus.Prometheus
{
    class InspectionBehavior : IBehavior<ITransportReceiveContext,ITransportReceiveContext>
    {
        private Counter messagesProcessed;
        private string localAddress;
        private Histogram processingTime;
        private string endpointName;
        private Gauge messagesInProgress;

        const double ticksPerMicrosecond = 10;

        public InspectionBehavior(string endpointName, string localAddress)
        {
            this.endpointName = endpointName;
            this.localAddress = localAddress;

            messagesProcessed = Metrics.CreateCounter("nservicebus_messages_processed_total", "Messages processed", "name", "local_address", "message_type", "message_intent", "queue");
            processingTime = Metrics.CreateHistogram("nservicebus_message_processing_duration_microseconds", "Message processing time in microseconds", new[] { 0.5, 0.9, 0.99 }, "name", "local_address", "message_type", "message_intent", "queue");
            messagesInProgress = Metrics.CreateGauge("nservicebus_messages_in_progress", "Messages in progress",
                "name", "local_address");
        }

        public async Task Invoke(ITransportReceiveContext context, Func<ITransportReceiveContext, Task> next)
        {
            var messageIntent = context.Message.Headers[Headers.MessageIntent];
            var messageType = context.Message.Headers[Headers.EnclosedMessageTypes];

            var stopWatch = Stopwatch.StartNew();

            try
            {
                messagesInProgress.Labels(endpointName, localAddress).Inc();

                await next(context).ConfigureAwait(false);
            }
            finally
            {
                stopWatch.Stop();

                var elapsedTotalMicroSeconds = stopWatch.Elapsed.Ticks / ticksPerMicrosecond;

                processingTime.Labels(endpointName, localAddress, messageType, messageIntent, localAddress).Observe(elapsedTotalMicroSeconds);

                messagesInProgress.Labels(endpointName, localAddress).Dec();
            }

            messagesProcessed.Labels(endpointName, localAddress, messageType, messageIntent, localAddress).Inc();
        }
    }
}