using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using ApiNotificationBot.Interfaces;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace ApiNotificationBot.Services
{
	public class ApiObserverService : IApiObserverService
	{
		public IObservable<string> ObserveApi(string apiAddress, string controller, TimeSpan period, string member)
		{
			return Observable.Interval(period)
					.SelectMany(_ => CallApi(apiAddress, controller, period))
					.Select(result => SelectMemberFromResult(result, member))
					.DistinctUntilChanged()
					.Retry()
					.Publish()
					.RefCount()
				;
		}

        private async Task<JObject> CallApi(string apiAddress, string controller, TimeSpan period)
		{
			var client = new RestClient(apiAddress);

			var request = new RestRequest(controller, Method.GET);

			using (var cancellationTokenSource = new CancellationTokenSource(period))
			{
				var response = await client.ExecuteGetTaskAsync(request, cancellationTokenSource.Token);
				var jsonObject = JObject.Parse(response.Content);
				return jsonObject;
			}
		}

		private string SelectMemberFromResult(JObject result, string member)
		{
			var value = result.SelectToken(member);
			return value.ToString();
		}
	}
}