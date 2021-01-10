using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace SmallMealPlan.Web.Model.ShoppingList
{
    public class IndexViewModel : BaseViewModel
    {
        public IndexViewModel(HttpContext context) : base(context)
        {
            SelectedArea = SmpArea.ShoppingList;
        }

        public IEnumerable<ShoppingListItemModel> MyList { get; set; } = new List<ShoppingListItemModel>();
        public IEnumerable<MealItemModel> MealFromPlannerList { get; set; } = new List<MealItemModel>();
        public IEnumerable<ShoppingListItemModel> BoughtList { get; set; } = new List<ShoppingListItemModel>();
        public Pagination BoughtListPagination { get; set; }
        public bool HasRtmToken { get; set; }
        public bool HasSmallListerToken { get; set; }
    }
}