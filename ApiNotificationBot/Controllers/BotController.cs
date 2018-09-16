using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ApiNotificationBot.Interfaces;

namespace ApiNotificationBot.Controllers
{
	/// <inheritdoc />
	[Route("api/[controller]")]
	public class BotController : Controller
    {
		private readonly IBotService botService;
	    /// <inheritdoc />
	    public BotController(IBotService botService)
	    {
		    this.botService = botService;
	    }
		/// <summary>
		/// Get the bot status
		/// </summary>
		/// <returns></returns>
        [HttpGet]
        public Task<bool> Get()
        {
	        return botService.GetStatus();
        }
		
		/// <summary>
		/// Start a new bot with a token
		/// </summary>
        [HttpPost]
        public Task<bool> Start(string token)
        {
	        return botService.Start(token);
        }
		
		/// <summary>
		/// Stop the bot if running
		/// </summary>
        [HttpDelete("{id}")]
        public Task Stop(int id)
        {
	        return botService.Stop();
        }
    }
}
