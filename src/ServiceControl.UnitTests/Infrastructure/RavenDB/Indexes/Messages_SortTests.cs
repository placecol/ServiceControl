﻿using System;
using System.Linq;
using NUnit.Framework;
using ServiceBus.Management.Infrastructure.RavenDB.Indexes;
using ServiceBus.Management.MessageAuditing;

[TestFixture]
public class Messages_SortTests
{
    [Test]
    public void Simple()
    {
        using (var documentStore = InMemoryStoreBuilder.GetInMemoryStore())
        {
            documentStore.Initialize();

            var customIndex = new Messages_Sort();
            customIndex.Execute(documentStore);

            using (var session = documentStore.OpenSession())
            {
                var timeSent = DateTime.Now;
                session.Store(new Message
                    {
                        Id = "id",
                        MessageType = "MessageType",
                        TimeSent = timeSent,
                        Status = MessageStatus.Successful,
                        ConversationId = "ConversationId",
                        IsSystemMessage = true,
                        ReceivingEndpoint = new EndpointDetails{Name = "EndpointName"},
                    });
                session.SaveChanges();

                var results = session.Query<Messages_Sort.Result, Messages_Sort>()
                    .Customize(x => x.WaitForNonStaleResults())
                    .OfType<Message>()
                    .ToList();
                Assert.AreEqual(1, results.Count);
                var message = results.First();
                Assert.AreEqual("id", message.Id);
                Assert.AreEqual("MessageType", message.MessageType);
                Assert.AreEqual(timeSent, message.TimeSent);
                Assert.AreEqual(MessageStatus.Successful, message.Status);
                Assert.AreEqual("ConversationId", message.ConversationId);
                Assert.IsTrue(message.IsSystemMessage);
            }

        }
    }
}