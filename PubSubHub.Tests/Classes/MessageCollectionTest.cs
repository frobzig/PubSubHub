using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PubSubHub.Models;

namespace PubSubHub.Tests
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class MessageCollectionTest
    {
        [TestMethod]
        public void ConstructorTest1()
        {
            MessageCollection mc = new MessageCollection();
            Assert.IsNotNull(mc);
        }

        [TestMethod]
        public void ConstructorTest2()
        {
            List<PubSubMessage> messages = new List<PubSubMessage>()
            {
                new PubSubMessage(),
                new PubSubMessage(),
                new PubSubMessage()
            };

            MessageCollection mc = new MessageCollection(messages);

            Assert.AreSame(messages[0], mc[0]);
            Assert.AreSame(messages[1], mc[1]);
            Assert.AreSame(messages[2], mc[2]);
        }
    }
}
