using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace PubSubHub.Models
{
    [Table("Subscriptions")]
    [DataContract]
    public class SubscriptionInfo : ISubscriptionInfo
    {
        #region ctor
        public SubscriptionInfo()
        {
            this.Id = TimestampGuid.Create();

            this.LastRefresh = DateTime.UtcNow;
        }

        public SubscriptionInfo(Guid clientId, Uri uri, string topicId, string groupId = null, int level = 0)
            : this()
        {
            this.Uri = uri;
            this.Client = clientId;
            this.Topic = topicId;
            this.Group = groupId;
            this.Level = level;
        }
        #endregion

        #region properties
        [Required]
        [DataMember(EmitDefaultValue = false, Name = "id", IsRequired = true)]
        [JsonProperty(PropertyName = "id", Required = Required.Always, DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
        public Guid Id { get; set; }

        [NotMapped]
        [JsonIgnore]
        public Uri Uri { get; set; }

        [Required(AllowEmptyStrings = true)]
        [DataMember(EmitDefaultValue = false, Name = "callback", IsRequired = true)]
        [JsonProperty(PropertyName = "callback", Required = Required.Always, DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
        public string MappedUri
        {
            get
            {
                if (this.Uri == null)
                    return String.Empty;
                else
                    return this.Uri.ToString();
            }

            set
            {
                Uri uri;

                if (Uri.TryCreate(value, UriKind.Absolute, out uri))
                    this.Uri = uri;
                else
                    this.Uri = null;
            }
        }

        [Required]
        [DataMember(EmitDefaultValue = false, Name = "client", IsRequired = true)]
        [JsonProperty(PropertyName = "client", Required = Required.Always, DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
        public Guid Client { get; set; }

        [Required]
        [DataMember(EmitDefaultValue = false, Name = "lastrefresh", IsRequired = true)]
        [JsonProperty(PropertyName = "lastrefresh", Required = Required.Always, DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
        public DateTime LastRefresh { get; set; }

        [Required(AllowEmptyStrings = false)]
        [DataMember(EmitDefaultValue = false, Name = "topic", IsRequired = true)]
        [JsonProperty(PropertyName = "topic", Required = Required.Always)]
        public string Topic { get; set; }

        [DataMember(EmitDefaultValue = false, Name = "group")]
        [JsonProperty(PropertyName = "group", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
        public string Group { get; set; }

        [DataMember(EmitDefaultValue = false, Name = "level")]
        [JsonProperty(PropertyName = "level", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
        public int Level { get; set; }

        [DataMember(EmitDefaultValue = false, Name = "failcount")]
        [JsonProperty(PropertyName = "failcount", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
        public int FailureCount { get; set; }
        #endregion
    }
}
