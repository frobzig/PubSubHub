using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using PubSubHub.Models;

namespace PubSubHub
{
    public class CallbackCollection : Collection<ISubscriptionInfo>
    {
        #region by uri and clientid
        public IEnumerable<ISubscriptionInfo> Find(Guid clientId, Uri uri, string groupId = null, int level = 0, bool treatNullAsWildcard = false)
        {
            return this.Where(
                cbi =>
                    ((treatNullAsWildcard && uri == null) || cbi.Uri == uri) &&
                    ((treatNullAsWildcard && clientId == null) || cbi.Client == clientId) &&
                    (groupId == null || cbi.Group == groupId) && 
                    (level == 0 || cbi.Level >= level));
        }

        public bool Contains(Guid clientId, Uri uri, string groupId = null, int level=0, bool treatNullAsWildcard = false)
        {
            return this.Find(clientId, uri, groupId, level, treatNullAsWildcard).FirstOrDefault() != null;
        }

        public void Remove(Guid clientId, Uri uri, string groupId = null, int level = 0, bool treatNullAsWildcard = false)
        {
            foreach (ISubscriptionInfo cbInfo in this.Find(clientId, uri, groupId, level, treatNullAsWildcard).ToList())
            {
                this.Remove(cbInfo);
            }
        }
        #endregion
    }
}
