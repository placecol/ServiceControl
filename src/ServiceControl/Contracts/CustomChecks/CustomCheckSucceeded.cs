﻿namespace ServiceControl.Contracts.CustomChecks
{
    using System;
    using NServiceBus;
    using ServiceBus.Management.MessageAuditing;

    public class CustomCheckSucceeded : IEvent
    {
        public string CustomCheckId { get; set; }
        public string Category { get; set; }
        public DateTime SucceededAt { get; set; }
        public EndpointDetails OriginatingEndpoint { get; set; }

    }
}
