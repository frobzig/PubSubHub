using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Dynamic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PubSubHub.Models
{
    public class PubSubMessage : IPubSubMessage
    {
        #region fields
        private JToken _jsonContent;

        private object _content;
        #endregion

        #region ctor
        public PubSubMessage()
        {
            this.MessageId = TimestampGuid.Create();
        }

        public PubSubMessage(IPubSubMessage message)
            : this()
        {
            this.TopicId = message.TopicId;
            this.Content = message.Content;
        }
        #endregion

        #region properties
        [Key]
        [JsonProperty(DefaultValueHandling=DefaultValueHandling.Ignore, PropertyName="messageid")]
        public Guid MessageId { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate, PropertyName = "publisheddatetime")]
        public DateTime PublishedDateTime { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate, PropertyName = "formattedpublisheddatetime")]
        [NotMapped]
        public string FormattedPublishedDateTime
        {
            get
            {
                return this.PublishedDateTime.ToString("G");
            }

            set
            {
                DateTime dateTime;
                if (String.IsNullOrEmpty(value) || !DateTime.TryParse(value, out dateTime))
                    this.PublishedDateTime = new DateTime();
                else
                    this.PublishedDateTime = dateTime;
            }
        }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate, PropertyName = "topic")]
        public string TopicId { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate, PropertyName = "group")]
        public string GroupId { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate, PropertyName = "level")]
        public int Level { get; set; }

        [NotMapped]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate, PropertyName = "clientid")]
        public Guid ClientId { get; set; }

        [NotMapped]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate, PropertyName = "subscriptionid")]
        public Guid SubscriptionId { get; set; }

        [NotMapped]
        [JsonProperty(NullValueHandling=NullValueHandling.Ignore, DefaultValueHandling=DefaultValueHandling.Ignore, PropertyName="content")]
        public dynamic Content
        {
            get
            {
                return this._content ?? this._jsonContent;
            }

            set
            {
                this._content = value;

                if (this._content != null)
                    this._jsonContent = JToken.FromObject(this._content);
            }
        }

        [Column("Content")]
        [JsonIgnore]
        public string MappedContent
        {
            get
            {
                if (this._jsonContent != null)
                    return this._jsonContent.ToString();

                return null;
            }

            set
            {
                if (String.IsNullOrEmpty(value))
                    this._jsonContent = null;
                else
                {
                    try
                    {
                        this._jsonContent = JToken.Parse(value);
                    }
                    catch (JsonReaderException)
                    {
                        this._jsonContent = new JValue(value);
                    }
                }                
            }
        }

        #endregion
    }
}
