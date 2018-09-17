using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApiNotificationBot.Interfaces;
using ApiNotificationBot.Models;
using Microsoft.AspNetCore.Mvc;

namespace ApiNotificationBot.Controllers
{
	[Route("api/[controller]")]
	public class TopicController : Controller
	{
		private readonly IDispatcherService dispatcherService;

		/// <inheritdoc />
		public TopicController(IDispatcherService dispatcherService)
		{
			this.dispatcherService = dispatcherService;
		}
		/// <summary>
		/// Get all topics
		/// </summary>
		/// <returns></returns>
		[HttpGet]
		public IEnumerable<string> GetAll()
		{
			return dispatcherService.GetTopics();
		}

		/// <summary>
		/// Add a new topic
		/// </summary>
		[HttpPost]
		public void Start(string topic, [FromBody]TopicSetting topicSetting)
		{
			dispatcherService.AddTopic(topic, topicSetting);
		}

		/// <summary>
		/// Delete a topic
		/// </summary>
		[HttpDelete("{topic}")]
		public void Delete(string topic)
		{
			dispatcherService.RemoveTopic(topic);
		}
	}
}