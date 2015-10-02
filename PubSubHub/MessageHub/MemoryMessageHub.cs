using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using PubSubHub.Models;

namespace PubSubHub
{
    public class MemoryMessageHub : IMessageHub
    {
        public static TimeSpan SubscriptionTimeout = TimeSpan.FromHours(24);

        public const int MaxMessages = 1000;
        public const int PruneCount = 100;

        private ReaderWriterLockSlim _subscriptionsLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
        private SubscriptionDictionary _subscriptions = new SubscriptionDictionary();

        private Dictionary<Guid, ISubscriptionInfo> _subscriptionsById = new Dictionary<Guid, ISubscriptionInfo>();

        private ReaderWriterLockSlim _messagesLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
        private Dictionary<Guid, IPubSubMessage> _messages = new Dictionary<Guid, IPubSubMessage>();

        private DateTime _oldestMessageDate = DateTime.MaxValue;

        public MemoryMessageHub()
        {
            this.MessagePublished += (d, m) => { };

            this.Load();
        }

        public void PublishMessage(Guid clientId, IPubSubMessage message)
        {
            this._subscriptionsLock.EnterUpgradeableReadLock();
            this._messagesLock.EnterWriteLock();
            
            try
            {
                if (message.PublishedDateTime == DateTime.MinValue)
                    message.PublishedDateTime = DateTime.UtcNow;

                if (message.PublishedDateTime < this._oldestMessageDate)
                {
                    this._oldestMessageDate = message.PublishedDateTime;
                }

                if (this._messages.Count >= MaxMessages)
                {
                    foreach (Guid key in this._messages.OrderBy(kvp => kvp.Value.PublishedDateTime).Take(PruneCount).Select(kvp => kvp.Key).ToList())
                    {
                        this._messages.Remove(key);
                    }
                }

                this._messages.Add(message.MessageId, message);
                this.OnMessageAdded(message);

                CallbackCollection callbacks;

                // Get callbacks for this topic or callbacks for any
                if (this._subscriptions.TryGetValue(message.TopicId, out callbacks))
                {
                    for (int i = callbacks.Count - 1; i >= 0; i--)
                    {
                        ISubscriptionInfo cbInfo = callbacks[i];

                        if (cbInfo.LastRefresh.Add(SubscriptionTimeout) <= DateTime.UtcNow)
                        {
                            this.RemoveSubscription(callbacks, cbInfo);
                        }
                        else if (clientId == Guid.Empty || cbInfo.Client != clientId)
                        {
                            //// This will execute only when the publish is global (clientId = Guid.Empty) or 
                            //// the subscriptionInfo.Client is not the publishing source (cbInfo.Client != clientId)
                            this.MessagePublished(cbInfo, message);
                        }
                    }
                }
            }
            finally
            {
                this._subscriptionsLock.ExitUpgradeableReadLock();
                this._messagesLock.ExitWriteLock();
            }
        }

        protected virtual void RemoveSubscription(CallbackCollection callbacks, ISubscriptionInfo cbInfo)
        {
            callbacks.Remove(cbInfo);
        }

        public bool RefreshSubscription(Guid id)
        {
            this._subscriptionsLock.EnterReadLock();

            try
            {
                ISubscriptionInfo cbInfo;

                if (this._subscriptionsById.TryGetValue(id, out cbInfo))
                {
                    cbInfo.LastRefresh = DateTime.UtcNow;
                    return true;
                }

                return false;
            }
            finally
            {
                this._subscriptionsLock.ExitReadLock();
            }
        }
        
        public ISubscriptionInfo Subscribe(Guid clientId, Uri callbackUri, string topicId, string groupId = null, int level = 0)
        {
            return this.Subscribe(clientId, callbackUri, topicId, groupId, Guid.Empty, level, true);
        }

        private ISubscriptionInfo Subscribe(Guid clientId, Uri callbackUri, string topicId, string groupId, Guid cbId, int level, bool notify)
        {
            if (callbackUri == null)
            {
                throw new ArgumentNullException("callbackUri");
            }

            this._subscriptionsLock.EnterWriteLock();

            try
            {
                CallbackCollection callbacks;

                if (!this._subscriptions.TryGetValue(topicId, out callbacks))
                {
                    callbacks = new CallbackCollection();
                    this._subscriptions.Add(topicId, callbacks);
                }

                ISubscriptionInfo cbInfo = callbacks.Find(clientId, callbackUri).FirstOrDefault();

                if (cbInfo == null)
                {
                    cbInfo = this.CreateNewSubscriptionInfo(clientId, callbackUri, topicId, groupId, level);

                    if (cbId != Guid.Empty)
                    {
                        cbInfo.Id = cbId;
                    }

                    callbacks.Add(cbInfo);
                    this._subscriptionsById.Add(cbInfo.Id, cbInfo);
                }
                else
                {
                    cbInfo.LastRefresh = DateTime.UtcNow;
                }

                if (notify)
                {
                    this.OnSubscribed(cbInfo);
                    return cbInfo;
                }

                return null;
            }
            finally
            {
                this._subscriptionsLock.ExitWriteLock();
            }
        }

        protected virtual ISubscriptionInfo CreateNewSubscriptionInfo(Guid clientId, Uri callbackUri, string topicId, string groupId, int level)
        {
            return new SubscriptionInfo(clientId, callbackUri, topicId, groupId, level);
        }

        public bool UnsubscribeById(Guid id)
        {
            this._subscriptionsLock.EnterWriteLock();

            try
            {
                ISubscriptionInfo cbInfo;

                if (this._subscriptionsById.TryGetValue(id, out cbInfo))
                {
                    this.Unsubscribe(cbInfo.Client, cbInfo.Uri, cbInfo.Topic, cbInfo.Group);
                    return true;
                }

                return false;
            }
            finally
            {
                this._subscriptionsLock.ExitWriteLock();
            }
        }

        public void Unsubscribe(Guid clientId, Uri callbackUri = null, string topicId = null, string groupId = null, int level = 0)
        {
            this._subscriptionsLock.EnterWriteLock();

            try
            {
                if (String.IsNullOrEmpty(topicId))
                {
                    foreach (CallbackCollection callbacks in this._subscriptions.Values)
                    {
                        List<ISubscriptionInfo> filteredList = callbacks.Find(clientId, callbackUri, treatNullAsWildcard: true).ToList();

                        foreach (ISubscriptionInfo cbInfo in filteredList)
                        {
                            callbacks.Remove(cbInfo);
                            _subscriptionsById.Remove(cbInfo.Id);
                            this.OnUnsubscribed(cbInfo);
                        }
                    }
                }
                else
                {
                    CallbackCollection callbacks;

                    if (this._subscriptions.TryGetValue(topicId, out callbacks))
                    {
                        List<ISubscriptionInfo> filteredList = callbacks.Find(clientId, callbackUri, treatNullAsWildcard: true).ToList();

                        foreach (ISubscriptionInfo cbInfo in filteredList)
                        {
                            callbacks.Remove(cbInfo);
                            _subscriptionsById.Remove(cbInfo.Id);
                            this.OnUnsubscribed(cbInfo);
                        }
                    }
                }
            }
            finally
            {
                this._subscriptionsLock.ExitWriteLock();
            }
        }
        
        public event MessagePublishedDelegate MessagePublished;

        public IPubSubMessage GetMessage(Guid messageId)
        {
            this._messagesLock.EnterReadLock();

            try
            {
                IPubSubMessage message;

                if (this._messages.TryGetValue(messageId, out message))
                {
                    return message;
                }
                else
                {
                    return this.GetArchivedMessage(messageId);
                }
            }
            finally
            {
                this._messagesLock.ExitReadLock();
            }
        }

        public MessageCollection GetMessagesSince(DateTime sinceDate, string topicId)
        {
            this._messagesLock.EnterReadLock();

            try
            {
                MessageCollection messages;

                if (sinceDate < this._oldestMessageDate)
                {
                    messages = this.GetArchivedMessagesSince(sinceDate, topicId);
                }
                else
                {
                    messages = new MessageCollection(this._messages.Values.Where(m => m.TopicId == topicId && m.PublishedDateTime >= sinceDate));
                }

                return messages;
            }
            finally
            {
                this._messagesLock.ExitReadLock();
            }
        }

        protected virtual void Load()
        {
            this._subscriptionsLock.EnterWriteLock();

            try
            {
                IEnumerable<ISubscriptionInfo> callbacks = this.GetArchivedSubscriptions();

                this._subscriptions.Clear();

                foreach (ISubscriptionInfo cbInfo in callbacks)
                {
                    this.Subscribe(cbInfo.Client, cbInfo.Uri, cbInfo.Topic, null, cbInfo.Id, 0, false);
                }
            }
            finally
            {
                this._subscriptionsLock.ExitWriteLock();
            }
        }

        protected virtual MessageCollection GetArchivedMessagesSince(DateTime sinceDate, string topicId)
        {
            this._messagesLock.EnterReadLock();

            try
            {
                return new MessageCollection(this._messages.Values) { SinceDate = sinceDate };
            }
            finally
            {
                this._messagesLock.ExitReadLock();
            }
        }

        protected virtual IPubSubMessage GetArchivedMessage(Guid messageId)
        {
            IPubSubMessage message;

            this._messagesLock.EnterReadLock();

            try
            {
                if (this._messages.TryGetValue(messageId, out message))
                    return message;
            }
            finally
            {
                this._messagesLock.ExitReadLock();
            }

            return null;
        }

        protected virtual IEnumerable<ISubscriptionInfo> GetArchivedSubscriptions()
        {
            return new List<ISubscriptionInfo>();
        }

        protected virtual void OnMessageAdded(IPubSubMessage message)
        {
        }

        protected virtual void OnSubscribed(ISubscriptionInfo cbInfo)
        {
        }

        protected virtual void OnUnsubscribed(ISubscriptionInfo cbInfo)
        {
        }

    }
}
