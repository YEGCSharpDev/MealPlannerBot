using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Program;

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
                foreach (var ingredient in ingredientList)
                {
                    var ingredientID = GetIngredientID(FormatIngredientString(ingredient));

                    if(ingredientID != 0)
                    {
                        using (var con = new SQLiteConnection("Data Source=MealPlannerino.db"))
                        {
                            con.Open();
                            using (var cmd = new SQLiteCommand(insertRecipeSql, con))
                            {
                                int recipeid = 1;

                                cmd.Parameters.AddWithValue("@RECIPE_ID", recipeid);
                                cmd.Parameters.AddWithValue("@RECIPE_NAME", FormatIngredientString(recipeName));
                                cmd.Parameters.AddWithValue("@INGREDIENT_ID", ingredientID);
                                cmd.Parameters.AddWithValue("@CREATED_ON", DateTime.Now.ToString());
                                cmd.Parameters.AddWithValue("@UPDATED_ON", DateTime.Now.ToString());
                                cmd.ExecuteNonQuery();
                                recipeid++;

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

        public int GetIngredientID (string ingredient)
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


    }
}
