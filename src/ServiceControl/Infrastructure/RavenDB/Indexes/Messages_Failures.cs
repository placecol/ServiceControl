﻿namespace ServiceControl.Infrastructure.RavenDB.Indexes
{
    using System.Linq;
    using Raven.Client.Indexes;
    using ServiceBus.Management.MessageAuditing;

    public class Messages_Failures : AbstractIndexCreationTask<Message>
    {
        public Messages_Failures()
        {
            Map = messages => from message in messages
                where message.Status == MessageStatus.Failed
                select new
                {
                    message.ReceivingEndpoint.Name,
                    message.ReceivingEndpoint.Machine,
                    message.MessageType,
                    message.FailureDetails.Exception.ExceptionType,
                    message.FailureDetails.Exception.Message,
                    message.TimeSent
                };
        }
    }
}