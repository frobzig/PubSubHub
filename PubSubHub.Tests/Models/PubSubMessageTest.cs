using System;
using System.Diagnostics.CodeAnalysis;
using System.Dynamic;
using System.Net.Http.Headers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using PubSubHub.Models;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace PubSubHub.Tests
{
    [ExcludeFromCodeCoverage]
    public class TestContent
    {
        public string TestName { get; set; }

        public int TestValue { get; set; }
    }

    [TestClass]
    [ExcludeFromCodeCoverage]
    public class PubSubMessageTest
    {
        [TestMethod]
        public void ConstructorTest1()
        {
            IPubSubMessage message = new PubSubMessage();
            Assert.IsNotNull(message);
            Assert.AreNotEqual<Guid>(Guid.Empty, message.MessageId);
        }

        [TestMethod]
        public void ConstructorTest2()
        {
            IPubSubMessage templateMessage = new PubSubMessage()
            {
                TopicId = "template topic",
                Content = "This is a test!"
            };

            IPubSubMessage newMessage = new PubSubMessage(templateMessage);

            Assert.AreNotEqual<Guid>(Guid.Empty, newMessage.MessageId);
            Assert.AreNotEqual<Guid>(templateMessage.MessageId, newMessage.MessageId);
            Assert.AreEqual<string>(templateMessage.TopicId, newMessage.TopicId);
            Assert.AreEqual<string>(templateMessage.MappedContent, newMessage.MappedContent);
        }

        [TestMethod]
        public void MappedContentTest1()
        {
            IPubSubMessage message = new PubSubMessage();

            message.MappedContent = @"{""TestName"":""TestValue""}";

            Assert.AreEqual<string>("TestValue", (string)message.Content.TestName);

            message.MappedContent = null;
            Assert.IsNull(message.MappedContent);
        }

        [TestMethod]
        public void MappedContentTest2()
        {
            IPubSubMessage message = new PubSubMessage();

            message.MappedContent = @"this is a test";

            Assert.AreEqual<string>("this is a test", (string)message.Content);

            message.MappedContent = null;
            Assert.IsNull(message.MappedContent);
        }

        [TestMethod]
        public void FormattedPublishedDateTimeTest()
        {
            IPubSubMessage msg = new PubSubMessage();
            DateTime publishedDateTime = DateTime.UtcNow;
            msg.PublishedDateTime = publishedDateTime;

            Assert.AreEqual(publishedDateTime.ToString(), msg.FormattedPublishedDateTime);

            msg.FormattedPublishedDateTime = publishedDateTime.ToString();

            Assert.AreEqual(publishedDateTime.ToString(), msg.PublishedDateTime.ToString());

            msg.FormattedPublishedDateTime = null;

            Assert.IsNotNull(msg.PublishedDateTime);

            Assert.AreEqual(default(DateTime).ToString(), msg.PublishedDateTime.ToString());
        }

        [TestMethod]
        public void JsonStringifyTest()
        {
            IPubSubMessage msg = new PubSubMessage();

            msg.Content = new { face = "test", dude = "man" };

            string jsonText = JsonConvert.SerializeObject(msg);

            Assert.IsNotNull(jsonText);

            IPubSubMessage deserializedMsg = JsonConvert.DeserializeObject<PubSubMessage>(jsonText);

            Assert.AreEqual(jsonText, JsonConvert.SerializeObject(deserializedMsg));
        }
    }
}
