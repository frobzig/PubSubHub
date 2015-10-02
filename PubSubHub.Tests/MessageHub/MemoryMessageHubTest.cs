using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PubSubHub.Models;
using Utils;

namespace PubSubHub.Tests
{
    [ExcludeFromCodeCoverage]
    internal class TestMessageHub : MemoryMessageHub
    {
        public bool IsLoaded { get; set; }

        public List<IPubSubMessage> PublishedMessages { get; set; }
        public List<ISubscriptionInfo> PublishedCallbacks { get; set; }

        public TestMessageHub()
        {
            this.PublishedCallbacks = new List<ISubscriptionInfo>();
            this.PublishedMessages = new List<IPubSubMessage>();

            this.MessagePublished += TestMessageHub_MessagePublished;
        }

        void TestMessageHub_MessagePublished(ISubscriptionInfo cbInfo, IPubSubMessage message)
        {
            this.PublishedMessages.Add(message);
            this.PublishedCallbacks.Add(cbInfo);
        }

        protected override void Load()
        {
            base.Load();
            this.IsLoaded = true;
        }

        protected override IEnumerable<ISubscriptionInfo> GetArchivedSubscriptions()
        {
            return new List<ISubscriptionInfo>()
            {
                new SubscriptionInfo(
                    TimestampGuid.NewGuid(),
                    new Uri(MemoryMessageHubTest.Address),
                    MemoryMessageHubTest.TestTopic)
            };
        }

        public MessageCollection ExposedGetArchivedMessagesSince(DateTime sinceDate, string topicId)
        {
            return base.GetArchivedMessagesSince(sinceDate, topicId);
        }

        public IPubSubMessage ExposedGetArchivedMessage(Guid messageId)
        {
            return base.GetArchivedMessage(messageId);
        }

        public IEnumerable<ISubscriptionInfo> ExposedGetArchivedSubscriptions()
        {
            return base.GetArchivedSubscriptions();
        }
    }

    [TestClass]
    [ExcludeFromCodeCoverage]
    public class MemoryMessageHubTest
    {
        public const string Address = "http://localhost:21032/PubSubListenerTest";
        public const string TestTopic = "TestTopic";

        [TestMethod]
        public void ConstructorTest()
        {
            TestMessageHub hub = new TestMessageHub();

            Assert.IsNotNull(hub);
            Assert.IsTrue(hub.IsLoaded);
        }

        [TestMethod]
        public void GetArchivedMessagesSinceTest()
        {
            TestMessageHub hub = new TestMessageHub();

            DateTime sinceDate = DateTime.UtcNow;

            MessageCollection messages = hub.ExposedGetArchivedMessagesSince(sinceDate, null);

            Assert.AreEqual<int>(0, messages.Count);
            Assert.AreEqual<DateTime>(sinceDate, messages.SinceDate);
        }

        [TestMethod]
        public void GetInvalidArchivedMessageTest()
        {
            TestMessageHub hub = new TestMessageHub();

            IPubSubMessage message = hub.ExposedGetArchivedMessage(TimestampGuid.NewGuid());
            Assert.IsNull(message);
        }

        [TestMethod]
        public void GetValidArchivedMessageTest()
        {
            TestMessageHub hub = new TestMessageHub();

            IPubSubMessage message = new PubSubMessage()
            {
                TopicId = "validArchivedMessageTest"
            };

            hub.PublishMessage(Guid.Empty, message);

            IPubSubMessage archived = hub.ExposedGetArchivedMessage(message.MessageId);
            Assert.IsNotNull(message);
            Assert.AreEqual<Guid>(message.MessageId, archived.MessageId);
        }

        [TestMethod]
        public void GetArchivedSubscriptions()
        {
            TestMessageHub hub = new TestMessageHub();

            IEnumerable<ISubscriptionInfo> callbacks = hub.ExposedGetArchivedSubscriptions();

            Assert.IsNotNull(callbacks);
            Assert.AreEqual<int>(0, callbacks.Count());
        }

        [TestMethod]
        public void PubSubTest()
        {
            Guid clientId1 = TimestampGuid.NewGuid();
            Guid clientId2 = TimestampGuid.NewGuid();

            TestMessageHub hub = new TestMessageHub();

            #region publish to the subscriber loaded internally in testmessagehub
            IPubSubMessage message1 = new PubSubMessage();

            message1.Content = "Hello, PubSubHub!";
            message1.TopicId = TestTopic;

            hub.PublishMessage(Guid.Empty, message1);

            Assert.AreEqual<int>(1, hub.PublishedMessages.Count);
            Assert.AreEqual<int>(1, hub.PublishedCallbacks.Count);
            Assert.AreEqual<Uri>(new Uri(Address), hub.PublishedCallbacks[0].Uri);
            Assert.AreEqual<IPubSubMessage>(message1, hub.PublishedMessages[0]);
            #endregion

            hub.PublishedMessages.Clear();
            hub.PublishedCallbacks.Clear();

            #region add a subscriber to the same topic as the first and publish a new message to it
            Uri uri1 = new Uri("http://tempuri.org/1");
            hub.Subscribe(clientId1, uri1, TestTopic);

            // test refresh my subscription while we're here
            hub.Subscribe(clientId1, uri1, TestTopic);

            IPubSubMessage message2 = new PubSubMessage(message1);
            hub.PublishMessage(Guid.Empty, message2);

            Assert.AreEqual<int>(2, hub.PublishedMessages.Count);
            Assert.AreEqual<int>(2, hub.PublishedCallbacks.Count);

            Assert.AreEqual<Uri>(new Uri(Address), hub.PublishedCallbacks[1].Uri);
            Assert.AreEqual<Uri>(uri1, hub.PublishedCallbacks[0].Uri);
            Assert.AreEqual<IPubSubMessage>(message2, hub.PublishedMessages[1]);
            Assert.AreEqual<IPubSubMessage>(message2, hub.PublishedMessages[0]);
            #endregion

            hub.PublishedMessages.Clear();
            hub.PublishedCallbacks.Clear();

            #region add previous subscriber to another topic and publish a message to the new topic
            const string Message3Topic = "Message3Topic";
            hub.Subscribe(clientId1, uri1, Message3Topic);

            IPubSubMessage message3 = new PubSubMessage(message1);
            message3.TopicId = Message3Topic;

            hub.PublishMessage(Guid.Empty, message3);

            Assert.AreEqual<int>(1, hub.PublishedMessages.Count);
            Assert.AreEqual<int>(1, hub.PublishedCallbacks.Count);
            Assert.AreEqual<Uri>(uri1, hub.PublishedCallbacks[0].Uri);
            Assert.AreEqual<IPubSubMessage>(message3, hub.PublishedMessages[0]);
            #endregion

            hub.PublishedMessages.Clear();
            hub.PublishedCallbacks.Clear();

            #region repeat previous test with a new topic
            const string Message4Topic = "Message4Topic";
            hub.Subscribe(clientId1, uri1, Message4Topic);

            IPubSubMessage message4 = new PubSubMessage(message1);
            message4.TopicId = Message4Topic;

            hub.PublishMessage(Guid.Empty, message4);

            Assert.AreEqual<int>(1, hub.PublishedMessages.Count);
            Assert.AreEqual<int>(1, hub.PublishedCallbacks.Count);
            Assert.AreEqual<IPubSubMessage>(message4, hub.PublishedMessages[0]);
            Assert.AreEqual<Uri>(uri1, hub.PublishedCallbacks[0].Uri);
            #endregion

            hub.PublishedMessages.Clear();
            hub.PublishedCallbacks.Clear();

            #region unubscribe previous test
            hub.Unsubscribe(clientId1, topicId: Message4Topic);

            IPubSubMessage message5 = new PubSubMessage(message4);
            hub.PublishMessage(Guid.Empty, message5);

            Assert.AreEqual<int>(0, hub.PublishedMessages.Count);
            Assert.AreEqual<int>(0, hub.PublishedCallbacks.Count);
            #endregion

            hub.PublishedMessages.Clear();
            hub.PublishedCallbacks.Clear();

            #region unsubscribe all of client1
            hub.Unsubscribe(clientId1);

            IPubSubMessage message6 = new PubSubMessage(message1);
            hub.PublishMessage(Guid.Empty, message6);

            Assert.AreEqual<int>(1, hub.PublishedMessages.Count);
            Assert.AreEqual<int>(1, hub.PublishedCallbacks.Count);
            Assert.AreEqual<Uri>(new Uri(Address), hub.PublishedCallbacks[0].Uri);
            Assert.AreEqual<IPubSubMessage>(message6, hub.PublishedMessages[0]);
            #endregion
        }

        [TestMethod]
        public void GetMessageTest()
        {
            TestMessageHub hub = new TestMessageHub();

            Assert.IsNull(hub.GetMessage(TimestampGuid.NewGuid()));

            IPubSubMessage message = new PubSubMessage()
            {
                Content = "Hello, GetMessageTest!",
                TopicId = "GetMessageTest",
            };

            hub.PublishMessage(Guid.Empty, message);

            Assert.IsNull(hub.GetMessage(TimestampGuid.NewGuid()));
            Assert.AreEqual<IPubSubMessage>(message, hub.GetMessage(message.MessageId));
        }

        [TestMethod]
        public void PublishMessageTest()
        {
            const string PublishMessageTestString = "PublishMessageTest";
            Guid myGuid = Guid.NewGuid();
            TestMessageHub hub = new TestMessageHub();

            hub.Subscribe(myGuid, new Uri(UriExtensions.EmptyUriString), PublishMessageTestString);

            IPubSubMessage message1 = new PubSubMessage()
            {
                Content = String.Format("Hello, {0}!", PublishMessageTestString),
                TopicId = PublishMessageTestString,
            };

            IPubSubMessage message2 = new PubSubMessage(message1);
            IPubSubMessage message3 = new PubSubMessage(message1);

            hub.PublishMessage(Guid.Empty, message1);
            Thread.Sleep(1);
            hub.PublishMessage(myGuid, message2);
            Thread.Sleep(1);
            hub.PublishMessage(Guid.Empty, message3);

            Assert.AreEqual(2, hub.PublishedMessages.Where(pm => pm.TopicId == PublishMessageTestString).Count());
        }

        [TestMethod]
        public void GetMessagesSinceTest()
        {
            const string GetMessagesSinceTestString = "GetMessagesSinceTest";
            MessageCollection messages;
            TestMessageHub hub = new TestMessageHub();

            messages = hub.GetMessagesSince(DateTime.MinValue, GetMessagesSinceTestString);
            Assert.AreEqual<int>(0, messages.Count);
            Assert.AreEqual<DateTime>(DateTime.MinValue, messages.SinceDate);

            IPubSubMessage message1 = new PubSubMessage()
            {
                Content = "Hello, GetMessageTest!",
                TopicId = GetMessagesSinceTestString,
            };

            IPubSubMessage message2 = new PubSubMessage(message1);
            IPubSubMessage message3 = new PubSubMessage(message1);

            hub.PublishMessage(Guid.Empty, message1);
            Thread.Sleep(1);
            hub.PublishMessage(Guid.Empty, message2);
            Thread.Sleep(1);
            hub.PublishMessage(Guid.Empty, message3);

            messages = hub.GetMessagesSince(DateTime.MinValue, GetMessagesSinceTestString);
            Assert.AreEqual<int>(3, messages.Count);
            Assert.AreEqual<DateTime>(DateTime.MinValue, messages.SinceDate);

            messages = hub.GetMessagesSince(message3.PublishedDateTime, GetMessagesSinceTestString);
            Assert.AreEqual<int>(1, messages.Count);
            Assert.AreEqual<IPubSubMessage>(message3, messages[0]);
            Assert.AreEqual<DateTime>(DateTime.MinValue, messages.SinceDate);
        }

        [TestMethod]
        public void MaxMessagesTest()
        {
            Assert.IsTrue(MemoryMessageHub.MaxMessages >= MemoryMessageHub.PruneCount);
            const int pruneOffset = MemoryMessageHub.MaxMessages - MemoryMessageHub.PruneCount;

            IPubSubMessage original = new PubSubMessage()
            {
                Content = "Hello, MaxMessagesTest!",
                TopicId = "MaxMessagesTest"
            };

            MemoryMessageHub hub = new MemoryMessageHub();
            PrivateObject phub = new PrivateObject(hub);
            Dictionary<Guid, IPubSubMessage> privateMessages = (Dictionary<Guid, IPubSubMessage>)phub.GetField("_messages", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField);

            hub.Subscribe(TimestampGuid.NewGuid(), new Uri("http://tempuri.org"), original.TopicId);

            List<IPubSubMessage> messages = new List<IPubSubMessage>();

            for (int i = 0; i < MemoryMessageHub.MaxMessages + MemoryMessageHub.PruneCount; i++)
            {
                IPubSubMessage message = new PubSubMessage(original);
                messages.Add(message);
            }

            for (int i = 0; i < MemoryMessageHub.MaxMessages; i++)
            {
                hub.PublishMessage(Guid.Empty, messages[i]);
                Thread.Sleep(1);
            }

            Assert.AreEqual<int>(MemoryMessageHub.MaxMessages, privateMessages.Count);

            for (int i = 0; i < MemoryMessageHub.MaxMessages; i++)
            {
                Assert.IsTrue(privateMessages.ContainsValue(messages[i]));
            }

            hub.PublishMessage(Guid.Empty, messages[MemoryMessageHub.MaxMessages]);
            Assert.AreEqual<int>(pruneOffset + 1, privateMessages.Count);

            for (int i = MemoryMessageHub.MaxMessages + 1; i < MemoryMessageHub.MaxMessages + MemoryMessageHub.PruneCount; i++)
            {
                hub.PublishMessage(Guid.Empty, messages[i]);
                Thread.Sleep(1);
            }

            Assert.AreEqual<int>(MemoryMessageHub.MaxMessages, privateMessages.Count);

            #region check for the most recently added messages
            for (int i = MemoryMessageHub.MaxMessages; i < MemoryMessageHub.MaxMessages + MemoryMessageHub.PruneCount; i++)
            {
                Assert.IsTrue(privateMessages.ContainsValue(messages[i]));
            }
            #endregion

            #region check for the remainder of the original group of messages
            for (int i = MemoryMessageHub.PruneCount; i < MemoryMessageHub.MaxMessages - MemoryMessageHub.PruneCount; i++)
            {
                Assert.IsTrue(privateMessages.ContainsValue(messages[i]));
            }
            #endregion
        }

        [TestMethod]
        public void BadSubscribeCallTest()
        {
            MemoryMessageHub hub = new MemoryMessageHub();
            bool asserted = false;

            try
            {
                hub.Subscribe(Guid.NewGuid(), null, Guid.NewGuid().ToString());
            }
            catch (ArgumentNullException e)
            {
                asserted = true;
                Assert.AreEqual("Value cannot be null.\r\nParameter name: callbackUri", e.Message);
            }

            Assert.IsTrue(asserted);
        }

        [TestMethod]
        public void ExpireSubscriptionTest()
        {
            MemoryMessageHub hub = new MemoryMessageHub();
            PrivateObject po = new PrivateObject(hub);

            SubscriptionDictionary subscriptions = (SubscriptionDictionary)po.GetField("_subscriptions");

            Uri callbackUri = new Uri(UriExtensions.EmptyUriString);
            Guid clientId = TimestampGuid.NewGuid();
            const string testTopic = "ExpireSubscriptionTestTopic";
            hub.Subscribe(clientId, callbackUri, testTopic);

            CallbackCollection callbacks;
            Assert.IsTrue(subscriptions.TryGetValue(testTopic, out callbacks));
            Assert.IsTrue(callbacks.Contains(clientId, callbackUri));

            IPubSubMessage message = new PubSubMessage()
            {
                TopicId = testTopic
            };

            hub.PublishMessage(Guid.Empty, message);

            Assert.IsTrue(subscriptions.TryGetValue(testTopic, out callbacks));
            Assert.IsTrue(callbacks.Contains(clientId, callbackUri));
            Assert.AreEqual<int>(1, callbacks.Count);

            callbacks[0].LastRefresh = DateTime.MinValue;

            hub.PublishMessage(Guid.Empty, new PubSubMessage(message));

            Assert.IsTrue(subscriptions.TryGetValue(testTopic, out callbacks));
            Assert.IsFalse(callbacks.Contains(clientId, callbackUri));
            Assert.AreEqual<int>(0, callbacks.Count);
        }

        [TestMethod]
        public void RefreshSubscriptionTest()
        {
            const string RefreshSubscriptionTestString = "RefreshSubscriptionTest";
            TestMessageHub hub = new TestMessageHub();
            Guid myGuid = Guid.NewGuid();
            ISubscriptionInfo info = hub.Subscribe(myGuid, new Uri(UriExtensions.EmptyUriString), RefreshSubscriptionTestString);


            Assert.IsTrue(hub.RefreshSubscription(info.Id));
            Assert.IsFalse(hub.RefreshSubscription(Guid.NewGuid()));
        }

        [TestMethod]
        public void UnsubscribeByIdTest()
        {
            const string UnsubscribeByIdTestString = "UnsubscribeByIdTest";
            TestMessageHub hub = new TestMessageHub();
            Guid myGuid = Guid.NewGuid();
            ISubscriptionInfo info = hub.Subscribe(myGuid, new Uri(UriExtensions.EmptyUriString), UnsubscribeByIdTestString);


            Assert.IsTrue(hub.UnsubscribeById(info.Id));
            //// Second check is here to determine if subscription is really gone
            Assert.IsFalse(hub.UnsubscribeById(info.Id));
            Assert.IsFalse(hub.UnsubscribeById(Guid.NewGuid()));
        }
    }
}
