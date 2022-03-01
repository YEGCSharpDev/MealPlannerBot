using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealPlannerBot
{
    public class DataAccess
    {
        const string insertIngredientSql = "INSERT INTO INGREDIENT (INGREDIENT_NAME,CREATED_ON,UPDATED_ON) values" +
            "(@INGREDIENT_NAME,@CREATED_ON,@UPDATED_ON);";

        const string insertRecipeSql = "INSERT INTO RECIPE (RECIPE_ID,RECIPE_NAME,INGREDIENT_ID,CREATED_ON,UPDATED_ON) values" +
            "(@RECIPE_ID,@RECIPE_NAME,@INGREDIENT_ID,@CREATED_ON,@UPDATED_ON);";

        const string getIngredientbyID = "SELECT INGREDIENT_ID FROM INGREDIENT WHERE INGREDIENT_NAME = '@INGREDIENT_NAME'";

        public void InsertIngredients(List<string> ingredients)
        {
            try
            {
                using(var con = new SQLiteConnection("Data Source=MealPlannerino.db"))
                {
                    con.Open();

                    using (var cmd = new SQLiteCommand(insertIngredientSql, con))
                    {

                        foreach (var ingredient in ingredients)
                        {
                            if (GetIngredientID(FormatIngredientString(ingredient)) == 0)
                            {
                                cmd.Parameters.AddWithValue("@INGREDIENT_NAME", FormatIngredientString(ingredient));
                                cmd.Parameters.AddWithValue("@CREATED_ON", @"datetime('now')");
                                cmd.Parameters.AddWithValue("@UPDATED_ON", @"datetime('now')");
                                cmd.Prepare();
                                cmd.ExecuteNonQuery();
                            }

                        }
                    }
                }

            }
            catch (Exception)
            {

                //do nothing, we will silence the duplicate inserts as there is no
            }

        }

        public void InsertRecipe(string recipeName, List<string> ingredientList)
        {
            try
            {
                using (var con = new SQLiteConnection("Data Source=MealPlannerino.db"))
                {
                    con.Open();

                    using (var cmd = new SQLiteCommand(insertRecipeSql, con))
                    {
                        var recipeid = new Guid();

                        foreach (var ingredient in ingredientList)
                        {
                            if (GetIngredientID(FormatIngredientString(ingredient)) != 0)
                            {
                                cmd.Parameters.AddWithValue("@RECIPE_ID", recipeid);
                                cmd.Parameters.AddWithValue("@RECIPE_NAME", FormatIngredientString(recipeName));
                                cmd.Parameters.AddWithValue("@INGREDIENT_ID", GetIngredientID(FormatIngredientString(ingredient)));
                                cmd.Parameters.AddWithValue("@CREATED_ON", @"datetime('now')");
                                cmd.Parameters.AddWithValue("@UPDATED_ON", @"datetime('now')");
                                cmd.Prepare();
                                cmd.ExecuteNonQuery();
                            }

                        }
                    }
                }

            }
            catch (Exception)
            {

                //do nothing, we will silence the duplicate inserts as there is no
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
                        return id;

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
