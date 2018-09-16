using System;
using System.Threading.Tasks;
using ApiNotificationBot.Enums;

namespace ApiNotificationBot.Interfaces
{
	public interface IApiObserverService
	{
		Task<IObservable<string>> Start(string apiAddress, ApiMethods method, TimeSpan interval, string member);
	}
}