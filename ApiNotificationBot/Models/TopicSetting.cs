using System;
using System.Collections.Generic;
using ApiNotificationBot.Enums;

namespace ApiNotificationBot.Models
{
    public class TopicSetting
    {
        public string Member { get; set; }
        public string ApiAddress { get; set; }
        public string Controller { get; set; }
        public TimeSpan Period { get; set; }
        public Dictionary<string, string> Subtopics { get; set; }
    }
}