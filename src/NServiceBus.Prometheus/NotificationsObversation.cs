using System.Threading.Tasks;
using NServiceBus.Faults;
using NServiceBus.Features;
using Prometheus;

namespace NServiceBus.Prometheus
{
    public class NotificationsObversation : FeatureStartupTask
    {
        private Notifications notifications;

        private Counter messagesSentToErrorQueue;
        private Counter messagesFailedAnImmediateRetryAttempt;
        private Counter messagesSentToDelayedRetries;

        public NotificationsObversation(Notifications notifications)
        {
            this.notifications = notifications;

            messagesSentToErrorQueue = Metrics.CreateCounter("nservicebus_messages_sent_to_error_queue_total", "Messages sent to the error queue", new[] { "message_type", "message_intent", "exception_type" });
            messagesFailedAnImmediateRetryAttempt = Metrics.CreateCounter("nservicebus_messages_failed_immediate_retry_attempt_total", "Messages that failed immediate retry attempt", new[] { "message_type", "message_intent", "exception_type" });
            messagesSentToDelayedRetries = Metrics.CreateCounter("nservicebus_messages_failed_delayed_retry_attempt_total", "Messages that failed delayed retry attempt", new[] { "message_type", "message_intent", "exception_type" });
        }
        protected override Task OnStart(IMessageSession session)
        {
            notifications.Errors.MessageHasBeenSentToDelayedRetries += ErrorsOnMessageHasBeenSentToDelayedRetries;
            notifications.Errors.MessageHasFailedAnImmediateRetryAttempt += ErrorsOnMessageHasFailedAnImmediateRetryAttempt;
            notifications.Errors.MessageSentToErrorQueue += ErrorsOnMessageSentToErrorQueue;

            return Task.CompletedTask;
        }

        protected override Task OnStop(IMessageSession session)
        {
            notifications.Errors.MessageHasBeenSentToDelayedRetries -= ErrorsOnMessageHasBeenSentToDelayedRetries;
            notifications.Errors.MessageHasFailedAnImmediateRetryAttempt -= ErrorsOnMessageHasFailedAnImmediateRetryAttempt;
            notifications.Errors.MessageSentToErrorQueue -= ErrorsOnMessageSentToErrorQueue;
            return Task.CompletedTask;
        }

        void ErrorsOnMessageSentToErrorQueue(object sender, FailedMessage failedMessage)
        {
            var messageIntent = failedMessage.Headers[Headers.MessageIntent];
            var messageType = failedMessage.Headers[Headers.EnclosedMessageTypes];
            var exceptionType = failedMessage.Exception.GetType().FullName;

            messagesSentToErrorQueue.Inc();
            messagesSentToErrorQueue.Labels(messageType, messageIntent, exceptionType).Inc();
        }

        void ErrorsOnMessageHasFailedAnImmediateRetryAttempt(object sender, ImmediateRetryMessage immediateRetryMessage)
        {
            var messageIntent = immediateRetryMessage.Headers[Headers.MessageIntent];
            var messageType = immediateRetryMessage.Headers[Headers.EnclosedMessageTypes];
            var exceptionType = immediateRetryMessage.Exception.GetType().FullName;

            messagesFailedAnImmediateRetryAttempt.Inc();
            messagesFailedAnImmediateRetryAttempt.Labels(messageType, messageIntent, exceptionType).Inc();
        }

        void ErrorsOnMessageHasBeenSentToDelayedRetries(object sender, DelayedRetryMessage delayedRetryMessage)
        {
            var messageIntent = delayedRetryMessage.Headers[Headers.MessageIntent];
            var messageType = delayedRetryMessage.Headers[Headers.EnclosedMessageTypes];
            var exceptionType = delayedRetryMessage.Exception.GetType().FullName;

            messagesSentToDelayedRetries.Inc();
            messagesSentToDelayedRetries.Labels(messageType, messageIntent, exceptionType).Inc();
        }
    }
}