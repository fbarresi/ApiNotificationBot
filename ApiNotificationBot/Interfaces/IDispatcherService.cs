using System.Collections.Generic;
using System.Threading.Tasks;
using ApiNotificationBot.Models;

namespace ApiNotificationBot.Interfaces
{
	public interface IDispatcherService
	{
		Task<bool> BroadcastMessage(string topic, string message);
		void AddTopic(string topic, TopicSetting setting);
		void RemoveTopic(string topic);
		IEnumerable<string> GetTopics();
		void AddSubscriber(string chatId, string topic);
		void RemoveSubscriber(string chatId, string topic);
	}
}