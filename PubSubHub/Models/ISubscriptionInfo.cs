using System;
namespace PubSubHub.Models
{
    public interface ISubscriptionInfo
    {
        Guid Client { get; set; }
        int FailureCount { get; set; }
        string Group { get; set; }
        Guid Id { get; set; }
        DateTime LastRefresh { get; set; }
        int Level { get; set; }
        string MappedUri { get; set; }
        string Topic { get; set; }
        Uri Uri { get; set; }
    }
}
