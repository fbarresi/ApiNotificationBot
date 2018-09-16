using System;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace ApiNotificationBot.Interfaces
{
	public interface IBotService
	{
		Task<bool> Start(string token);
        Task<bool> Stop();
        Task<bool> GetStatus();
		Task<bool> SendMessage(string chatId, string message);
		IObservable<Message> Messages {get;}
	}
}