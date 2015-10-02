using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PubSubHub.Models;

namespace PubSubHub
{
    public delegate void MessagePublishedDelegate(ISubscriptionInfo cbInfo, IPubSubMessage message);

    public interface IMessageHub
    {
        void PublishMessage(Guid clientId, IPubSubMessage message);

        ISubscriptionInfo Subscribe(Guid clientId, Uri callbackUri, string topicId, string groupId = null, int level = 0);

        bool RefreshSubscription(Guid id);

        void Unsubscribe(Guid clientId, Uri callbackUri = null, string topicId = null, string groupId = null, int level = 0);

        bool UnsubscribeById(Guid id);

        IPubSubMessage GetMessage(Guid messageId);

        MessageCollection GetMessagesSince(DateTime sinceDate, string topicId);

        event MessagePublishedDelegate MessagePublished;
    }
}
