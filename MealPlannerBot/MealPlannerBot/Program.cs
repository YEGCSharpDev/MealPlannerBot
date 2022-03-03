using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using System.Configuration;
using static MealPlannerBot.DataAccess;

var botClient = new TelegramBotClient(token : ConfigurationManager.AppSettings["BotToken"]);    
using var cts = new CancellationTokenSource();

const string incorrectAddRecipeFormatMessage = "Incorrect format, please stick to RecipeName - Ingredient1, Ingredient2, Ingredient3 when using AddRecipe Command";
const string ingredientAdditionConfirmation = "Adding Ingredients.";
const string recipeAdditionConfirmation = "Adding Recipe";
const string recipeAdditionCompletion = "Recipe Added.";
const string recipeAlreadyAdded = "Recipe Already in collection";

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

        var ingredientInstanceInstantiator = new MealPlannerBot.DataAccess();

        ingredientInstanceInstantiator.GetIngredientID(" ");
        
        if (recipeStringArray.Length > 2 || messageText == "/addrecipe")
        {
            await SendMessageAsync(incorrectAddRecipeFormatMessage, chatId, cancellationToken);
        }

        var recipeName = recipeStringArray[0];
        var ingredientList = recipeStringArray[1].Split(',').ToList();
        
        if (ValidateRecipeName (recipeName) == 0)
        {
            await SendMessageAsync(ingredientAdditionConfirmation, chatId, cancellationToken);

            InsertIngredients(ingredientList);

            await SendMessageAsync(recipeAdditionConfirmation, chatId, cancellationToken);

            InsertRecipe(recipeName, ingredientList);

            await SendMessageAsync(recipeAdditionCompletion, chatId, cancellationToken);

        }
        else
        {
            await SendMessageAsync(recipeAlreadyAdded, chatId, cancellationToken);
            
        }

    }
    else if(messageText.StartsWith("/generatemealplan"))
    {
        

    }
    else
    {
        var sendmessage = $"Received a '{messageText}' message in chat {chatId}. from user {username}";

        await SendMessageAsync(sendmessage, chatId, cancellationToken);
    }



}

void InsertIngredients(List<string> ingredientList)
{
    var ingredientInstanceInstantiator = new MealPlannerBot.DataAccess();

    ingredientInstanceInstantiator.InsertIngredients(ingredientList);
}

void InsertRecipe(string recipeName, List<string> ingredientList)
{
    var recipeInstanceInstantiator = new MealPlannerBot.DataAccess();

    recipeInstanceInstantiator.InsertRecipe(recipeName, ingredientList);
}

int ValidateRecipeName(string recipeName)
{
    
    var recipevalidationInstantiator = new MealPlannerBot.DataAccess();

    return recipevalidationInstantiator.GetRecipeID(recipeName);

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

async Task SendMessageAsync (string message, ChatId chatId,CancellationToken cancellationToken )
{
        Message sentMessage = await botClient.SendTextMessageAsync(
        chatId: chatId,
        text: message,
        cancellationToken: cancellationToken);
}





