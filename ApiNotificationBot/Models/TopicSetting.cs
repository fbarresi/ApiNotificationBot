using System;
using ApiNotificationBot.Enums;

namespace ApiNotificationBot.Models
{
    public class TopicSetting
    {
        public string Member { get; set; }
        public string ApiAddress { get; set; }
        public TimeSpan Interval { get; set; }
        public ApiMethods Method { get; set; }
    }
}