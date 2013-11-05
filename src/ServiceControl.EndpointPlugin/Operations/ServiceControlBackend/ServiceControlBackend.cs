﻿namespace ServiceControl.EndpointPlugin.Operations.ServiceControlBackend
{
    using System;
    using System.Configuration;
    using System.IO;
    using Messages.CustomChecks;
    using Messages.Heartbeats;
    using NServiceBus;
    using NServiceBus.Config;
    using NServiceBus.MessageInterfaces.MessageMapper.Reflection;
    using NServiceBus.Serializers.Json;
    using NServiceBus.Transports;

    public class ServiceControlBackend
    {
        public ServiceControlBackend()
        {
            var messageMapper = new MessageMapper();
            messageMapper.Initialize(new[] {typeof(EndpointHeartbeat), typeof(ReportCustomCheckResult)});

            serializer = new JsonMessageSerializer(messageMapper);

            serviceControlBackendAddress = GetServiceControlAddress();
        }

        public ISendMessages MessageSender { get; set; }

        public void Send(object messageToSend, TimeSpan timeToBeReceived)
        {
            var message = new TransportMessage {TimeToBeReceived = timeToBeReceived};

            using (var stream = new MemoryStream())
            {
                serializer.Serialize(new[] {messageToSend}, stream);
                message.Body = stream.ToArray();
            }

            MessageSender.Send(message, serviceControlBackendAddress);
        }

        public void Send(object messageToSend)
        {
            Send(messageToSend, TimeSpan.MaxValue);
        }

        static Address GetServiceControlAddress()
        {
            var queueName = ConfigurationManager.AppSettings[@"ServiceControl/Queue"];
            if (!String.IsNullOrEmpty(queueName))
            {
                return Address.Parse(queueName);
            }


            var errorAddress = ConfigureFaultsForwarder.ErrorQueue;
            if (errorAddress != null)
            {
                return new Address("Particular.ServiceControl", errorAddress.Machine);
            }

            if (VersionChecker.CoreVersionIsAtLeast(4, 1))
            {
                //audit config was added in 4.1
                Address address;
                if (TryGetAuditAddress(out address))
                {
                    return new Address("Particular.ServiceControl", address.Machine);
                }
            }

            return null;
        }

        static bool TryGetAuditAddress(out Address address)
        {
            var auditConfig = Configure.GetConfigSection<AuditConfig>();
            if (auditConfig != null && !string.IsNullOrEmpty(auditConfig.QueueName))
            {
                var forwardAddress = Address.Parse(auditConfig.QueueName);

                {
                    address = forwardAddress;

                    return true;
                }
            }
            address = null;

            return false;
        }

        readonly JsonMessageSerializer serializer;
        readonly Address serviceControlBackendAddress;
    }
}