﻿namespace ServiceControl.Alerts.MessageFailures
{
    using System.Collections.Generic;
    using Contracts.Alerts;
    using Contracts.MessageFailures;
    using NServiceBus;
    using Raven.Client;

    class MessageFailedHandler : IHandleMessages<MessageFailed>
    {
        public IBus Bus { get; set; }
        public IDocumentStore DocumentStore { get; set; }

        public void Handle(MessageFailed message)
        {
            using (var session = DocumentStore.OpenSession())
            {
                session.Advanced.UseOptimisticConcurrency = true;

                var relatedToList = new List<string>
                {
                    string.Format("/failedMessageId/{0}", message.Id),
                    string.Format("/endpoint/{0}/{1}", message.Endpoint, message.Machine)
                };

                var alert = new Alert
                {
                    RaisedAt = message.FailedAt,
                    Severity = Severity.Error,
                    Description = message.Reason,
                    Category = Category.MessageFailures,
                    RelatedTo = relatedToList
                };

                session.Store(alert);
                session.SaveChanges();

                Bus.Publish<AlertRaised>(m =>
                {
                    m.RaisedAt = alert.RaisedAt;
                    m.Severity = alert.Severity;
                    m.Description = alert.Description;
                    m.Id = alert.Id;
                    m.Category = alert.Category;
                    m.RelatedTo = alert.RelatedTo;
                });
            }
        }
    }
}
