﻿using System;
using System.Linq;
using NUnit.Framework;
using ServiceBus.Management.Infrastructure.RavenDB.Indexes;
using ServiceBus.Management.MessageAuditing;

[TestFixture]
public class Conversations_SortedTests
{
    [Test]
    public void Simple()
    {
        using (var documentStore = InMemoryStoreBuilder.GetInMemoryStore())
        {
            documentStore.Initialize();
            
            var customIndex = new Conversations_Sorted();
            customIndex.Execute(documentStore);

            using (var session = documentStore.OpenSession())
            {
                var now = DateTime.Now;
                session.Store(new Message
                {
                    Id = "id",
                    MessageType = "MessageType",
                    TimeSent = now,
                    Status = MessageStatus.Successful,
                    ConversationId = "ConversationId",
                    ProcessedAt = now,
                    OriginatingEndpoint = new EndpointDetails{Name = "foo"},
                    Recoverable = true,
                });
                session.SaveChanges();

                var results = session.Query<Conversations_Sorted.Result, Conversations_Sorted>()
                    .Customize(x => x.WaitForNonStaleResults())
                    .OfType<Message>()
                    .ToList();
                Assert.AreEqual(1, results.Count);
                var message = results.First();
                Assert.AreEqual("id", message.Id);
                Assert.AreEqual("MessageType", message.MessageType);
                Assert.AreEqual(now, message.TimeSent);
                Assert.AreEqual(now, message.ProcessedAt);
                Assert.AreEqual(MessageStatus.Successful, message.Status);
                Assert.IsTrue(message.Recoverable);
                Assert.AreEqual("foo", message.OriginatingEndpoint.Name);
                Assert.AreEqual("ConversationId", message.ConversationId);
            }

        }
    }
}