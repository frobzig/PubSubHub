using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PubSubHub.Models;
using Utils;

namespace PubSubHub.Tests
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class SubscriptionInfoTest
    {
        [TestMethod]
        public void Constructor1Test()
        {
            ISubscriptionInfo cbInfo = new SubscriptionInfo();

            Assert.AreNotEqual<Guid>(Guid.Empty, cbInfo.Id);
            Assert.IsTrue(cbInfo.LastRefresh <= DateTime.UtcNow);
        }

        [TestMethod]
        public void Constructor2Test()
        {
            Guid clientId = TimestampGuid.Create();
            Uri uri = new Uri("http://tempuri.org/1");
            string topicId = "TestTopic";

            ISubscriptionInfo cbInfo = new SubscriptionInfo(clientId, uri, topicId);

            Assert.AreNotEqual<Guid>(Guid.Empty, cbInfo.Id);
            Assert.IsTrue(cbInfo.LastRefresh <= DateTime.UtcNow);

            Assert.AreEqual<Guid>(clientId, cbInfo.Client);
            Assert.AreEqual<Uri>(uri, cbInfo.Uri);
            Assert.AreEqual<string>(topicId, cbInfo.Topic);
        }

        [TestMethod]
        public void PropertiesTest()
        {
            ISubscriptionInfo cbInfo = new SubscriptionInfo();

            int failureCount = 1;
            string topicId = "TestTopic";
            DateTime lastRefresh = DateTime.UtcNow;
            Guid clientId = TimestampGuid.Create();
            Uri uri = new Uri("http://tempuri.org/1");
            Guid id = TimestampGuid.Create();

            Assert.AreEqual<int>(0, cbInfo.FailureCount);
            Assert.AreEqual<string>(null, cbInfo.Topic);
            Assert.AreNotEqual<DateTime>(new DateTime(), cbInfo.LastRefresh);
            Assert.AreEqual<Guid>(Guid.Empty, cbInfo.Client);
            Assert.AreEqual<Uri>(null, cbInfo.Uri);
            Assert.AreNotEqual<Guid>(Guid.Empty, cbInfo.Id);
            Assert.AreEqual<string>(String.Empty, cbInfo.MappedUri);

            cbInfo.FailureCount = failureCount;
            cbInfo.Topic = topicId;
            cbInfo.LastRefresh = lastRefresh;
            cbInfo.Client = clientId;
            cbInfo.Uri = uri;
            cbInfo.Id = id;

            Assert.AreEqual<int>(failureCount, cbInfo.FailureCount);
            Assert.AreEqual<string>(topicId, cbInfo.Topic);
            Assert.AreEqual<DateTime>(lastRefresh, cbInfo.LastRefresh);
            Assert.AreEqual<Guid>(clientId, cbInfo.Client);
            Assert.AreEqual<Uri>(uri, cbInfo.Uri);
            Assert.AreEqual<Guid>(id, cbInfo.Id);
            Assert.AreEqual<string>(uri.ToString(), cbInfo.MappedUri);
        }

        [TestMethod]
        public void InvalidMappedUriTest()
        {
            ISubscriptionInfo cbInfo = new SubscriptionInfo();

            cbInfo.MappedUri = "123 this is not a uri";

            Assert.IsNull(cbInfo.Uri);
        }

        [TestMethod]
        public void MappedUriTest()
        {
            ISubscriptionInfo cbInfo = new SubscriptionInfo();

            cbInfo.MappedUri = "http://www.google.com";

            Assert.IsNotNull(cbInfo.Uri);
        }
    }
}
