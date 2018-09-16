using System.Reactive.Disposables;
using System.Threading.Tasks;

namespace ApiNotificationBot.Interfaces
{
	public interface IBotService
	{
		Task<bool> Start(string token);
        Task<bool> Stop();
        Task<bool> GetStatus();
		Task<bool> SendMessage(string chatId, string message);
	}
}