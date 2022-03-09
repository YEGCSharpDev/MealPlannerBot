using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MealPlannerBot.Recipe;
namespace MealPlannerBot
{
    public class Recipe
    {
        public int? RecipeID { get; set; }  
        public string? RecipeName { get; set; }

        public List<Ingredient>? Ingredients { get; set; }

        public class Ingredient
        {
            public int IngredientID { get; set; }
            public string IngredientName { get; set; }
        }
    }
}
