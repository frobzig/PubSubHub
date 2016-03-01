namespace PubSubHub
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;

    public static class UriUtility
    {
        public static string EmptyUriString { get { return "http://tempuri.org"; } }

        public static IEnumerable<string> GetSegments(string uri)
        {
            return new Uri(UriUtility.Combine(EmptyUriString, uri)).Segments;
        }

        /// <summary>
        /// Combines the specified URIs without duplicating slashes.
        /// </summary>
        /// <param name="parts">The URIs to combine.</param>
        /// <returns>A string of URIs delimited by slash.</returns>
        public static string Combine(params string[] parts)
        {
            if (parts == null)
                throw new ArgumentNullException("parts");

            if (parts.Count() < 1)
                throw new InvalidOperationException("parts must contain at least one parameter");

            StringBuilder builder = new StringBuilder();

            foreach (string part in parts)
            {
                string myUri = part;

                myUri = myUri.Trim('/');

                builder.Append(myUri + "/");
            }

            return builder.ToString().TrimEnd('/');
        }

        public static string GetQueryParameter(string uriString, string key)
        {
            Regex queryPattern = new Regex(String.Format(@"[\?\&]({0})=([^\&]*)", key));

            Match match = queryPattern.Match(uriString);

            if (match.Success)
                return match.Groups[2].Value;
            else
                return null;
        }
    }
}