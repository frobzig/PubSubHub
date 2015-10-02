using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PubSubHub.Models;
using Utils;

namespace PubSubHub.Tests
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class CallbackCollectionTest
    {
        [TestMethod]
        public void ConstructorTest()
        {
            CallbackCollection callbacks = new CallbackCollection();
            Assert.IsNotNull(callbacks);
        }

        [TestMethod]
        public void FindTest()
        {
            Guid clientId = TimestampGuid.NewGuid();

            string topicId = "Testing";

            Uri uri1 = new Uri("http://tempuri.org/1", UriKind.Absolute);
            Uri uri2 = new Uri("http://tempuri.org/2", UriKind.Absolute);
            Uri uri3 = new Uri("http://tempuri.org/3", UriKind.Absolute);
            Uri uri4 = new Uri("http://tempuri.org/4", UriKind.Absolute);
            Uri uri5 = new Uri("http://tempuri.org/5", UriKind.Absolute);

            CallbackCollection callbacks = new CallbackCollection()
            {
                new SubscriptionInfo(clientId, uri1, topicId),
                new SubscriptionInfo(clientId, uri2, topicId),
                new SubscriptionInfo(clientId, uri3, topicId),
                new SubscriptionInfo(clientId, uri4, topicId),
                new SubscriptionInfo(clientId, uri5, topicId),
            };

            IList<ISubscriptionInfo> results;

            results = callbacks.Find(clientId, uri1).ToList();

            Assert.AreEqual<int>(1, results.Count);
            Assert.AreEqual<Uri>(uri1, results.First().Uri);

            results = callbacks.Find(clientId, null, treatNullAsWildcard: true).ToList();

            Assert.AreEqual<int>(5, results.Count);

            results = callbacks.Find(Guid.NewGuid(), null, treatNullAsWildcard: true).ToList();

            Assert.AreEqual<int>(0, results.Count);
        }

        [TestMethod]
        public void ContainsTest()
        {
            Guid clientId = TimestampGuid.NewGuid();

            string topicId = "Testing";

            Uri uri1 = new Uri("http://tempuri.org/1", UriKind.Absolute);
            Uri uri2 = new Uri("http://tempuri.org/2", UriKind.Absolute);
            Uri uri3 = new Uri("http://tempuri.org/3", UriKind.Absolute);
            Uri uri4 = new Uri("http://tempuri.org/4", UriKind.Absolute);
            Uri uri5 = new Uri("http://tempuri.org/5", UriKind.Absolute);

            CallbackCollection callbacks = new CallbackCollection()
            {
                new SubscriptionInfo(clientId, uri1, topicId),
                new SubscriptionInfo(clientId, uri2, topicId),
                new SubscriptionInfo(clientId, uri3, topicId),
                new SubscriptionInfo(clientId, uri4, topicId),
                new SubscriptionInfo(clientId, uri5, topicId),
            };

            Assert.IsTrue(callbacks.Contains(clientId, uri1));
            Assert.IsTrue(callbacks.Contains(clientId, null, treatNullAsWildcard: true));
            Assert.IsFalse(callbacks.Contains(clientId, null));
            Assert.IsFalse(callbacks.Contains(Guid.NewGuid(), null, treatNullAsWildcard: true));
        }

        [TestMethod]
        public void RemoveTest()
        {
            Guid clientId = TimestampGuid.NewGuid();

            string topicId = "Testing";

            Uri uri1 = new Uri("http://tempuri.org/1", UriKind.Absolute);
            Uri uri2 = new Uri("http://tempuri.org/2", UriKind.Absolute);
            Uri uri3 = new Uri("http://tempuri.org/3", UriKind.Absolute);
            Uri uri4 = new Uri("http://tempuri.org/4", UriKind.Absolute);
            Uri uri5 = new Uri("http://tempuri.org/5", UriKind.Absolute);

            ISubscriptionInfo cbInfo1 = new SubscriptionInfo(clientId, uri1, topicId);
            ISubscriptionInfo cbInfo2 = new SubscriptionInfo(clientId, uri2, topicId);
            ISubscriptionInfo cbInfo3 = new SubscriptionInfo(clientId, uri3, topicId);
            ISubscriptionInfo cbInfo4 = new SubscriptionInfo(clientId, uri4, topicId);
            ISubscriptionInfo cbInfo5 = new SubscriptionInfo(clientId, uri5, topicId);

            CallbackCollection callbacks = new CallbackCollection()
            {
                cbInfo1,
                cbInfo2,
                cbInfo3,
                cbInfo4,
                cbInfo5
            };

            IList<ISubscriptionInfo> results;

            results = callbacks.Find(clientId, uri1).ToList();
            Assert.AreEqual<int>(1, results.Count);
            Assert.AreEqual<Uri>(uri1, results.First().Uri);

            results = callbacks.Find(clientId, null, treatNullAsWildcard: true).ToList();
            Assert.AreEqual<int>(5, results.Count);

            results = callbacks.Find(Guid.NewGuid(), null, treatNullAsWildcard: true).ToList();
            Assert.AreEqual<int>(0, results.Count);

            callbacks.Remove(clientId, uri1);
            callbacks.Remove(Guid.NewGuid(), uri2);

            Assert.IsFalse(callbacks.Contains(clientId, uri1));
            Assert.IsTrue(callbacks.Contains(clientId, uri2));

            callbacks.Remove(clientId, null, treatNullAsWildcard: true);

            Assert.AreEqual<int>(0, callbacks.Count);
        }
    }
}
