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
        private Summary processingTime;

        const int ticksPerMicrosecond = 10;

        public InspectionBehavior(string localAddress)
        {
            this.localAddress = localAddress;

            messagesProcessed = Metrics.CreateCounter("nservicebus_messages_processed_total", "Messages processed", new[] {"message_type", "message_intent", "queue"});
            processingTime = Metrics.CreateSummary("nservicebus_message_processing_duration_microseconds", "Message processing time in microseconds",
                new[] {"message_type", "message_intent", "queue"});
        }

        public async Task Invoke(ITransportReceiveContext context, Func<ITransportReceiveContext, Task> next)
        {
            var messageIntent = context.Message.Headers[Headers.MessageIntent];
            var messageType = context.Message.Headers[Headers.EnclosedMessageTypes];

            var stopWatch = Stopwatch.StartNew();

            try
            {
                await next(context).ConfigureAwait(false);
            }
            finally
            {
                stopWatch.Stop();

                var elapsedTotalMicroSeconds = Math.Floor(stopWatch.Elapsed.Ticks % TimeSpan.TicksPerMillisecond / (double)ticksPerMicrosecond);

                processingTime.Observe(elapsedTotalMicroSeconds);
                processingTime.Labels(messageType, messageIntent, localAddress).Observe(elapsedTotalMicroSeconds);
            }

            messagesProcessed.Labels(messageType, messageIntent, localAddress).Inc();
        }
    }
}