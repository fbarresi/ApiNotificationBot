using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using ApiNotificationBot.Interfaces;
using ApiNotificationBot.Models;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace ApiNotificationBot.Services
{
	public class DispatcherService : IDispatcherService, IDisposable
	{
		private readonly IBotService botService;
		private readonly IApiObserverService apiObserverService;

		private readonly ConcurrentDictionary<string, IDisposable> topicSubscriptions = new ConcurrentDictionary<string, IDisposable>();
		private readonly ConcurrentDictionary<string, List<string>> topicSubscibers = new ConcurrentDictionary<string, List<string>>();
		private readonly CompositeDisposable disposables = new CompositeDisposable();

		public DispatcherService(IBotService botService, IApiObserverService apiObserverService)
		{
			this.botService = botService;
			this.apiObserverService = apiObserverService;

			var messageSubcription = botService.Messages
				.SelectMany(DispatchMessage)
				.Retry()
				.Subscribe();
			disposables.Add(messageSubcription);
		}

        private bool CheckMatching(string text, string matchingText)
        {
            if (string.IsNullOrEmpty(matchingText)) return true;
            if (text.ToLowerInvariant().Contains(matchingText.ToLowerInvariant()))
                return true;
            return false;
        }

		private async Task<Unit> DispatchMessage(Message message)
		{
			var chatId = message.Chat.Id;
			if (message.Type == MessageType.Text)
			{
				if (message.Text.StartsWith("/"))
				{
					var inputs = message.Text.ToLowerInvariant().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
					var command = inputs.FirstOrDefault();
					var parameter = inputs.LastOrDefault();
					var reply = "Unsupported command...";
					switch (command)
					{
						case "/topics":
							reply = string.Join("\n", GetTopics());
							break;
						case "/subscribe":
							if(AddSubscriber(chatId.ToString(), parameter))
								reply = $"Subscribed chat to {parameter}";
							else
                                reply = $"Topic '{parameter}' not found...";
							break;
						case "/unsubscribe":
							if(RemoveSubscriber(chatId.ToString(), parameter))
								reply = $"Unsubscribed chat from {parameter}";
                            else
                                reply = $"Topic '{parameter}' not found...";
							break;
						default:
							break;
					}
					await botService.SendMessage(chatId.ToString(), reply);
				}
			}
			return Unit.Default;
		}

		private async Task<Unit> BroadcastToSubscibers(string message, params string[] topics)
		{
			foreach(var topic in topics)
			{
				foreach (var chatId in topicSubscibers[topic])
				{
					await botService.SendMessage(chatId, $"{topic}: {message}");
				}
			}
			return Unit.Default;
		}

		public void AddTopic(string topic, TopicSetting setting)
		{
			var topics = new Dictionary<string, string>();
			topics[topic] = string.Empty;
			if(setting.Subtopics != null)
				foreach(var subtopic in setting.Subtopics)
					topics[$"{topic}.{subtopic.Key}"] = subtopic.Value;
					
			RemoveTopics(topics.Keys.ToArray());

			var apiSubscription = apiObserverService.ObserveApi(setting.ApiAddress, setting.Controller, setting.Period, setting.Member);

            foreach (var t in topics)
                AddTopicSubscription(apiSubscription, t.Key, t.Value);
		}

		private void AddTopicSubscription(IObservable<string> messages, string topic, string matchingText)
		{
            var subscription = messages
				.Where(message => CheckMatching(message, matchingText))
                .SelectMany(message => BroadcastToSubscibers(message, topic))
                .Retry()
                .Subscribe();

            topicSubscriptions[topic] = subscription;
            topicSubscibers[topic] = new List<string>();
		}

		private void RemoveTopics(params string[] topics)
		{
			foreach(var topic in topics)
			{
				IDisposable disposable;
				List<string> subscribers;

				if (topicSubscriptions.ContainsKey(topic))
				{
					topicSubscriptions[topic].Dispose();
					topicSubscriptions.TryRemove(topic, out disposable);
				}

				if (topicSubscibers.ContainsKey(topic)) topicSubscibers.TryRemove(topic, out subscribers);
			}
		}

		public void RemoveTopic(string topic)
		{
			RemoveTopics(topic);
		}

		public IEnumerable<string> GetTopics()
		{
			return topicSubscriptions.Keys;
		}

		private bool AddSubscriber(string chatId, string topic)
		{
			if (topicSubscibers.ContainsKey(topic))
			{
				topicSubscibers[topic].Add(chatId);
				topicSubscibers[topic] = topicSubscibers[topic].Distinct().ToList();
				return true;
			}
			return false;
		}

		private bool RemoveSubscriber(string chatId, string topic)
		{
			if (topicSubscibers.ContainsKey(topic))
			{
				topicSubscibers[topic].RemoveAll(s => s.Equals(chatId));
                return true;
			}
			return false;
		}

		public void Dispose()
		{
			disposables.Dispose();
			foreach (var topic in topicSubscriptions)
			{
                topic.Value.Dispose();
			}
		}
	}
}