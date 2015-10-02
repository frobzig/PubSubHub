// <copyright file="PubSubHub.cs" company="Cyrious">
//     Copyright (c) Cyrious Software.  All rights reserved.
// </copyright>

namespace PubSubHub
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public static class UriUtility
    {
        /// <summary>
        /// A valid Uri string that can be used as a placeholder.
        /// </summary>
        public const string EmptyUriString = @"http://tempuri.org";

        /// <summary>
        /// Combines the specified URIs without duplicating slashes.
        /// </summary>
        /// <param name="uris">The URIs to combine.</param>
        /// <returns>A string of URIs delimited by slash.</returns>
        public static string Combine(params string[] uris)
        {
            StringBuilder builder = new StringBuilder();

            foreach (string uri in uris)
            {
                string myUri = uri;

                myUri = myUri.Trim('/');

                builder.Append(myUri + "/");
            }

            return builder.ToString().TrimEnd('/');
        }
    }
}
