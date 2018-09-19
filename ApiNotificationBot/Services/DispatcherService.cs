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
							AddSubscriber(chatId.ToString(), parameter);
							reply = $"Subscribed chat to {parameter}";
							break;
						case "/unsubscribe":
							RemoveSubscriber(chatId.ToString(), parameter);
							reply = $"Unsubscribed chat from {parameter}";
							break;
						default:
							break;
					}
					await botService.SendMessage(chatId.ToString(), reply);
				}
			}
			return Unit.Default;
		}

		private async Task<Unit> BroadcastToSubscibers(string topic, string message)
		{
			foreach (var chatId in topicSubscibers[topic])
			{
				await botService.SendMessage(chatId, $"{topic}: {message}");
			}
			return Unit.Default;
		}

		public void AddTopic(string topic, TopicSetting setting)
		{
			if(topicSubscriptions.ContainsKey(topic)) topicSubscriptions[topic].Dispose();
			List<string> subscribers;
			if (topicSubscibers.ContainsKey(topic)) topicSubscibers.TryRemove(topic, out subscribers);

			var apiSubscription = apiObserverService.ObserveApi(setting.ApiAddress, setting.Controller, setting.Period, setting.Member);
			var subscription = apiSubscription
				.SelectMany(message => BroadcastToSubscibers(topic, message))
				.Retry()
				.Subscribe();

			topicSubscriptions[topic] = subscription;
			topicSubscibers[topic] = new List<string>();
		}

		public void RemoveTopic(string topic)
		{
			IDisposable disposabe;
			List<string> subscribers;

			if (topicSubscriptions.ContainsKey(topic))
			{
				topicSubscriptions[topic].Dispose();
				topicSubscriptions.TryRemove(topic, out disposabe);
			}

			if (topicSubscibers.ContainsKey(topic)) topicSubscibers.TryRemove(topic, out subscribers);

		}

		public IEnumerable<string> GetTopics()
		{
			return topicSubscriptions.Keys;
		}

		public void AddSubscriber(string chatId, string topic)
		{
			if (topicSubscibers.ContainsKey(topic))
			{
				topicSubscibers[topic].Add(chatId);
				topicSubscibers[topic] = topicSubscibers[topic].Distinct().ToList();
			}
		}

		public void RemoveSubscriber(string chatId, string topic)
		{
			if (topicSubscibers.ContainsKey(topic))
			{
				topicSubscibers[topic].RemoveAll(s => s.Equals(chatId));
			}
		}

		public void Dispose()
		{
			disposables.Dispose();
			foreach (var topic in topicSubscriptions.Keys)
			{
				topicSubscriptions[topic].Dispose();
			}
		}
	}
}