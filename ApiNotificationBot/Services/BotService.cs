using System;
using System.Threading.Tasks;
using ApiNotificationBot.Interfaces;

namespace ApiNotificationBot.Services{
public class BotService : IBotService, IDisposable
{

    Task<bool> IBotService.Start(string token)
    {
        throw new NotImplementedException();
    }

    Task<bool> IBotService.Stop()
    {
        throw new NotImplementedException();
    }

    Task<bool> IBotService.GetStatus()
    {
        throw new NotImplementedException();
    }

    Task<bool> IBotService.SendMessage(string chatId, string message)
    {
        throw new NotImplementedException();
    }

    public void Dispose()
    {
        throw new NotImplementedException();
    }
}
}