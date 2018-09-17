using System;
using System.Threading.Tasks;
using ApiNotificationBot.Enums;

namespace ApiNotificationBot.Interfaces
{
	public interface IApiObserverService
	{
		IObservable<string> ObserveApi(string apiAddress, string controller, TimeSpan period, string member);
	}
}