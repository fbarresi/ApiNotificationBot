using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using ApiNotificationBot.Interfaces;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace ApiNotificationBot.Services{
    public class BotService : IBotService, IDisposable
    {
        private TelegramBotClient botClient;
        private readonly SerialDisposable serialDisposable = new SerialDisposable();
        private readonly Subject<Message> messagesSubject = new Subject<Message>();
        public IObservable<Message> Messages => messagesSubject.AsObservable();

        public async Task<bool> Start(string token)
        {
            botClient = new TelegramBotClient(token);
            var user = await botClient.GetMeAsync();
            var subscription = Observable.FromEventPattern<MessageEventArgs>(h=>botClient.OnMessage+=h, h=>botClient.OnMessage-=h)
                                        .Select(me=> me.EventArgs)
                                        .Select(ea=>ea.Message)
                                        .Retry()
                                        .Subscribe(messagesSubject.OnNext);
            serialDisposable.Disposable = subscription;
            botClient.StartReceiving();
            return user.Id != 0;
        }

        private async Task<Unit> DispatchMessage(Message message)
        {
            var chatId = message.Chat.Id;
            if(message.Type == MessageType.Text)
            {
                if(message.Text.StartsWith("//"))
                {
                    var inputs = message.Text.ToLowerInvariant().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    var command = inputs.FirstOrDefault();
                    var parameter = inputs.LastOrDefault();
                    var reply = "Unsupported command...";
                    switch(command)
                    {
                        case "//topics":
                            break;
                        default:
                            break;
                    }
                    await SendMessage(chatId.ToString(), reply);
                }
            }
            return Unit.Default;
        }

        public Task<bool> Stop()
        {
            botClient?.StopReceiving();
            botClient = null;
            return Task.FromResult(true);
        }

        public Task<bool> GetStatus()
        {
            return botClient.TestApiAsync();
        }

        public async Task<bool> SendMessage(string chatId, string message)
        {
            await botClient.SendTextMessageAsync(new ChatId(chatId), message);
            return true;
        }

        public void Dispose()
        {
            serialDisposable.Disposable.Dispose();
        }
    }
}