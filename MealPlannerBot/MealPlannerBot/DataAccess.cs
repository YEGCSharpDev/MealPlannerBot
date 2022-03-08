using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Program;
using static MealPlannerBot.Recipe;

namespace MealPlannerBot
{
    public class DataAccess
    {


        const string insertIngredientSql = "INSERT INTO INGREDIENT (INGREDIENT_NAME,CREATED_ON,UPDATED_ON) values" +
            "(@INGREDIENT_NAME,@CREATED_ON,@UPDATED_ON);";

        const string insertRecipeSql = "INSERT INTO RECIPE (RECIPE_ID,RECIPE_NAME,INGREDIENT_ID,CREATED_ON,UPDATED_ON) values" +
            "(@RECIPE_ID,@RECIPE_NAME,@INGREDIENT_ID,@CREATED_ON,@UPDATED_ON);";

        const string getIngredientbyID = "SELECT INGREDIENT_ID FROM INGREDIENT WHERE INGREDIENT_NAME = @INGREDIENT_NAME";

        const string getRecipebyID = "SELECT DISTINCT RECIPE_MASTER_ID FROM RECIPE WHERE RECIPE_NAME = @RECIPE_NAME LIMIT 1";

        const string getMaxRecipeID = "SELECT COALESCE(MAX(RECIPE_ID),0) FROM RECIPE;";

        const string getAllRecipes = "SELECT DISTINCT RECIPE_ID, RECIPE_NAME FROM RECIPE";

        const string getAllIngredientsForRecipe = "SELECT A.INGREDIENT_ID, B.INGREDIENT_NAME FROM RECIPE A " +
            "INNER JOIN INGREDIENT B ON A.INGREDIENT_ID = B.INGREDIENT_ID WHERE A.RECIPE_ID= @RECIPE_ID";
        public void InsertIngredients(List<string> ingredients)
        {
            try
            {
                foreach (var ingredient in ingredients)
                {
                    var ingredientID = GetIngredientID(FormatIngredientString(ingredient));

                    if (ingredientID == 0)
                    {
                        using (var con = new SQLiteConnection("Data Source=MealPlannerino.db"))
                        {
                            using (var cmd = new SQLiteCommand(insertIngredientSql, con))
                            {
                                cmd.Parameters.AddWithValue("@INGREDIENT_NAME", FormatIngredientString(ingredient));
                                cmd.Parameters.AddWithValue("@CREATED_ON", DateTime.Now.ToString());
                                cmd.Parameters.AddWithValue("@UPDATED_ON", DateTime.Now.ToString());
                                con.Open();
                                cmd.ExecuteNonQuery();

                            }
                        }
                    }
                }

            }
            catch (Exception ex)
            {

                throw;
                //do nothing, we will silence the duplicate inserts as there is no
            }

        }

        public void InsertRecipe(string recipeName, List<string> ingredientList)
        {
            try
            {
                var maxRecipeId = GetMaxRecipeID();

                int recipeid = maxRecipeId + 1;
                foreach (var ingredient in ingredientList)
                {
                    var ingredientID = GetIngredientID(FormatIngredientString(ingredient));

                    if (ingredientID != 0)
                    {
                        using (var con = new SQLiteConnection("Data Source=MealPlannerino.db"))
                        {
                            con.Open();
                            using (var cmd = new SQLiteCommand(insertRecipeSql, con))
                            {

                                cmd.Parameters.AddWithValue("@RECIPE_ID", recipeid);
                                cmd.Parameters.AddWithValue("@RECIPE_NAME", FormatIngredientString(recipeName));
                                cmd.Parameters.AddWithValue("@INGREDIENT_ID", ingredientID);
                                cmd.Parameters.AddWithValue("@CREATED_ON", DateTime.Now.ToString());
                                cmd.Parameters.AddWithValue("@UPDATED_ON", DateTime.Now.ToString());
                                cmd.ExecuteNonQuery();


                            }
                        }
                    }
                }


            }
            catch (Exception)
            {
                throw;
            }

        }

        public int GetMaxRecipeID()
        {
            int maxRecipeID = 0;

            using (var con = new SQLiteConnection("Data Source=MealPlannerino.db"))
            {
                using (var cmd = new SQLiteCommand(getMaxRecipeID, con))
                {
                    con.Open();

                    SQLiteDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        maxRecipeID = reader.GetInt32(0);
                    }

                }

            }

            return maxRecipeID;
        }
        public int GetIngredientID(string ingredient)
        {
            int id = 0;

            using (var con = new SQLiteConnection("Data Source=MealPlannerino.db"))
            {
                using (var cmd = new SQLiteCommand(getIngredientbyID, con))
                {
                    con.Open();
                    cmd.Parameters.AddWithValue("@INGREDIENT_NAME", FormatIngredientString(ingredient));

                    SQLiteDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        id = reader.GetInt32(0);
                    }

                }

            }
            return id;
        }

        public int GetRecipeID(string recipe)
        {
            int id = 0;

            using (var con = new SQLiteConnection("Data Source=MealPlannerino.db"))
            {
                using (var cmd = new SQLiteCommand(getRecipebyID, con))
                {
                    con.Open();
                    cmd.Parameters.AddWithValue("@RECIPE_NAME", FormatIngredientString(recipe));

                    SQLiteDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        id = reader.GetInt32(0);
                    }

                }

            }
            return id;
        }



        public string FormatIngredientString(string ingredient)
        {
            return ingredient.Trim().ToUpperInvariant();

        }

        public List<Recipe> GetallRecipes()
        {
            var recipeList = new List<Recipe>();

            using (var con = new SQLiteConnection("Data Source=MealPlannerino.db"))
            {
                using (var cmd = new SQLiteCommand(getAllRecipes, con))
                {
                    con.Open();
                    
                    SQLiteDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {

                        var tempRecipe = new Recipe();
                        tempRecipe.RecipeID = reader.GetInt32(0);
                        tempRecipe.RecipeName = reader.GetString(1);
                        recipeList.Add(tempRecipe);
                    }

                }

            }
            GetAllIngredientsforRecipe(recipeList);

            return recipeList;

        }

        public List<Recipe> GetAllIngredientsforRecipe (List<Recipe> recipelist)
        {
            foreach (var recipe in recipelist)
            {
                var recipeid = recipe.RecipeID;

                recipe.Ingredients = new List<Ingredient>();

                using (var con = new SQLiteConnection("Data Source=MealPlannerino.db"))
                {
                    using (var cmd = new SQLiteCommand(getAllIngredientsForRecipe, con))
                    {
                        con.Open();
                        cmd.Parameters.AddWithValue("@RECIPE_ID", recipe.RecipeID);

                        SQLiteDataReader reader = cmd.ExecuteReader();
                        while (reader.Read())
                        {
                            var tempingredient = new Ingredient();

                            tempingredient.IngredientID = reader.GetInt32(0);
                            tempingredient.IngredientName = reader.GetString (1);
                            recipe.Ingredients.Add(tempingredient);
                            //reader.NextResult();
                            
                        }

                    }

                }
            }

            return recipelist;
        }


    }
}
