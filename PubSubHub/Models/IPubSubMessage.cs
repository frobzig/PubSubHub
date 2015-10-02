namespace PubSubHub.Models
{
    using System;
    using System.Collections.Generic;
    
    public interface IPubSubMessage
    {
        Guid ClientId { get; set; }
        dynamic Content { get; set; }
        string FormattedPublishedDateTime { get; set; }
        string GroupId { get; set; }
        int Level { get; set; }
        string MappedContent { get; set; }
        Guid MessageId { get; set; }
        DateTime PublishedDateTime { get; set; }
        Guid SubscriptionId { get; set; }
        string TopicId { get; set; }
    }
}
