using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealPlannerBot
{
    public class MealPlan : Recipe
    {
        public Recipe recipe { get; set; }

        public DateTime mealDate { get; set; }

        public string mealTiming { get; set; }

        public int? orderOfMeal { get; set; }


    }
}
