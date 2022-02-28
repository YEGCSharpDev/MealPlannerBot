﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using System.Configuration;

var botClient = new TelegramBotClient(token : ConfigurationManager.AppSettings["BotToken"]);    
using var cts = new CancellationTokenSource();


var receiverOptions = new ReceiverOptions
{
    AllowedUpdates = { } 
};

botClient.StartReceiving(
    HandleUpdateAsync,
    HandleErrorAsync,
    receiverOptions,
    cancellationToken: cts.Token);

var mealBot = await botClient.GetMeAsync();

Console.WriteLine($"Start listening for @{mealBot.FirstName}");
Console.ReadLine();

// Send cancellation request to stop bot
cts.Cancel();

async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
{
    // Only process Message updates: https://core.telegram.org/bots/api#message
    if (update.Type != UpdateType.Message)
        return;
    // Only process text messages
    if (update.Message!.Type != MessageType.Text)
        return;

    var chatId = update.Message.Chat.Id;
    var messageText = update.Message.Text;
    var username = update.Message.Chat.FirstName;

    if (messageText.StartsWith("/addrecipe"))
    {
        var recipeStringArray = messageText.Substring(10).Split('-');
        
        if (recipeStringArray.Length > 2 || messageText == "/addrecipe")
        {
            Message IncorrectAddRecipeResponse = await botClient.SendTextMessageAsync(
            chatId: chatId,
            text: "Incorrect format, please stick to RecipeName - Ingredient1, Ingredient2, Ingredient3 when using AddRecipe Command",
            cancellationToken: cancellationToken);
            return;
        }

        var recipeName = recipeStringArray[0];
        var ingredientList = recipeStringArray[1].Split(',').ToList();
        Message AddRecipeResponse = await botClient.SendTextMessageAsync(
        chatId: chatId,
        text: "Adding Recipe, please enter ingredients now.",
        cancellationToken: cancellationToken);
    }
    else
    {
        Console.WriteLine($"Received a '{messageText}' message in chat {chatId}. from user {username}");

        Message sentMessage = await botClient.SendTextMessageAsync(
        chatId: chatId,
        text: "You said:\n" + messageText,
        cancellationToken: cancellationToken);

    }



}

Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
{
    var ErrorMessage = exception switch
    {
        ApiRequestException apiRequestException
            => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
        _ => exception.ToString()
    };

    Console.WriteLine(ErrorMessage);
    return Task.CompletedTask;
}
