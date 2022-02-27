using System;
using System.Configuration;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;

namespace MealPlannerBot
{
    public static class Program
    {
        private static TelegramBotClient Bot = new TelegramBotClient(ConfigurationManager.AppSettings.Get("ApiKey"));

        public static async Task Main()
        {
            Bot = new TelegramBotClient(ConfigurationManager.AppSettings.Get("ApiKey"));

            User me = await Bot.GetMeAsync();
            Console.Title = me.Username ?? "My awesome Bot";

            using var cts = new CancellationTokenSource();

            // StartReceiving does not block the caller thread. Receiving is done on the ThreadPool.
            ReceiverOptions receiverOptions = new() { AllowedUpdates = { } };
            Bot.StartReceiving(Handler)
            //Bot.StartReceiving(Handlers.HandleUpdateAsync,
            //                   Handlers.HandleErrorAsync,
            //                   receiverOptions,
            //                   cts.Token);

            Console.WriteLine($"Start listening for @{me.Username}");
            Console.ReadLine();

            // Send cancellation request to stop bot
            cts.Cancel();
        }
    }
}

    