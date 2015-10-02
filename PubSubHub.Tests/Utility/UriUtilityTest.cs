using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Utils;

namespace PubSubHub.Tests.Utility
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class UriUtilityTest
    {
        [TestMethod]
        public void CombineTest()
        {
            string[] uris = { "foo", "bar", "/foo/bar/", "abc/", "/cba", "/cba/abc", "foo/" };
            string expected = "foo/bar/foo/bar/abc/cba/cba/abc/foo";
            Assert.AreEqual<string>(expected, UriExtensions.Combine(uris));

            uris[uris.Length - 1] = "foo";
            Assert.AreEqual<string>(expected, UriExtensions.Combine(uris));
        }
    }
}
