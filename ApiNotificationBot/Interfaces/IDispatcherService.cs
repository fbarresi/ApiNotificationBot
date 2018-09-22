using System.Collections.Generic;
using System.Threading.Tasks;
using ApiNotificationBot.Models;

namespace ApiNotificationBot.Interfaces
{
	public interface IDispatcherService
	{
		void AddTopic(string topic, TopicSetting setting);
		void RemoveTopic(string topic);
		IEnumerable<string> GetTopics();
	}
}