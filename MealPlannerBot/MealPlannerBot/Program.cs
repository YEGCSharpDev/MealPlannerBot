using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using System.Configuration;
using MealPlannerBot;
using System.Text;
using System.Globalization;

var botClient = new TelegramBotClient(token : ConfigurationManager.AppSettings["BotToken"]);    
using var cts = new CancellationTokenSource();

const string incorrectAddRecipeFormatMessage = "Incorrect format, please stick to RecipeName - Ingredient1, Ingredient2, Ingredient3 when using AddRecipe Command";
const string ingredientAdditionConfirmation = "Adding Ingredients.";
const string recipeAdditionConfirmation = "Adding Recipe";
const string recipeAdditionCompletion = "Recipe Added.";
const string recipeAlreadyAdded = "Recipe Already in collection";
const string mealPlanFormatIncorrect = "add yyyy-mm-dd date after generatemealplan keyword";
var dataAccessInstanceInstantiator = new MealPlannerBot.DataAccess();

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
        //yyyy-mm-dd
        if (messageText.Length > 17)
        {
            string? messageTextArray = messageText.Substring(17).Trim();

            DateTime planStartDate = GetNextSunday(messageTextArray);

            List<Recipe>? recipelist = GetAllRecipes();

            Random rand = new Random();

            var shuffledrecipelist = recipelist.OrderByDescending(recipe => rand.Next(recipelist.Count)).Take(15).ToList();

            var plannedmeallist = GenerateMealPlan (shuffledrecipelist,planStartDate);

            var sendingmessage = new StringBuilder();

            sendingmessage.AppendLine("Here is the Meal Plan : ");

            sendingmessage.AppendLine(Environment.NewLine);

            foreach (var meal in plannedmeallist)
            {
                DateOnly mealdate = DateOnly.FromDateTime(meal.mealDate);


                string? mealinformation = string.Concat(mealdate, "(", meal.mealDate.DayOfWeek.ToString().Substring(0,2),") ", " : ", meal.mealTiming, " - ", meal.recipe.RecipeName);

                sendingmessage.AppendLine(mealinformation);

            }

            await SendMessageAsync(sendingmessage.ToString(), chatId, cancellationToken);

            var ingredientlist = new List<String>();

            foreach (var meal in plannedmeallist)
            {
                foreach (var ingredient in meal.recipe.Ingredients)
                {
                    ingredientlist.Add(ingredient.IngredientName);
                }

            }

            ingredientlist = ingredientlist.Distinct().ToList();

            sendingmessage = new StringBuilder();

            sendingmessage.AppendLine("Here is the Ingredient List : ");

            sendingmessage.AppendLine(Environment.NewLine);

            foreach (var item in ingredientlist)
            {
                sendingmessage.AppendLine(item);

            }

            await SendMessageAsync(sendingmessage.ToString(), chatId, cancellationToken);

        }

        else
        {
            await SendMessageAsync(mealPlanFormatIncorrect, chatId, cancellationToken);
        }


    }
    else
    {
        var sendmessage = $"Received a '{messageText}' message in chat {chatId}. from user {username}";

        await SendMessageAsync(sendmessage, chatId, cancellationToken);
    }



}

List <MealPlan> GenerateMealPlan(List<Recipe> recipelist, DateTime planstartdate)
{
    var mealplanlist = new List<MealPlan>();


    for (int i = 0; i < recipelist.Count; i++)
    {
        if ((i == 6) || (i == 13))
        {
            mealplanlist.Add(new MealPlan
            {
                recipe = new Recipe
                {
                    RecipeName = "Pongal",
                    Ingredients = new List<Recipe.Ingredient>()
                    {
                        new Recipe.Ingredient
                        {
                            IngredientID = 1,
                            IngredientName = "Cumin"
                        }
                    }
                },
                mealDate = planstartdate.AddDays(i),
                mealTiming = "Lunch"

            }) ;
        }

        else if ((i != 5) && (i != 12))
        {
            
            mealplanlist.Add(new MealPlan
            {
                recipe = recipelist[i],
                mealDate = planstartdate.AddDays(i),
                mealTiming = "Dinner"
            });

            if (i != 14)
            mealplanlist.Add(new MealPlan
            {
                recipe = recipelist[i],
                mealDate = planstartdate.AddDays(i+1),
                mealTiming = "Lunch"
            });
        }


    }

    return mealplanlist;
}

List<Recipe> GetAllRecipes()
{
    var recipeList = dataAccessInstanceInstantiator.GetallRecipes();

    return recipeList;
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

DateTime GetNextSunday(string? messageText)
{
    var startdate = DateTime.Now;

    if (messageText[1].ToString() != String.Empty)
    {
        startdate = DateTime.Parse(messageText.ToString().Trim());
    }
    
    var integerday = (int)startdate.DayOfWeek;

    var nearestSunday = integerday == 0 ? startdate : startdate.AddDays(7 - integerday);

    return nearestSunday;
}