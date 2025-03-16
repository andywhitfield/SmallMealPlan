namespace SmallMealPlan.Web.Model.ShoppingList;

public class IndexViewModel : BaseViewModel
{
    public IndexViewModel(HttpContext context) : base(context) => SelectedArea = SmpArea.ShoppingList;

    public IEnumerable<ShoppingListItemModel> MyList { get; set; } = new List<ShoppingListItemModel>();
    public IEnumerable<MealItemModel> MealFromPlannerList { get; set; } = new List<MealItemModel>();
    public IEnumerable<ShoppingListItemModel> BoughtList { get; set; } = new List<ShoppingListItemModel>();
    public string? RegularOrBought { get; set; }
    public Pagination? BoughtListPagination { get; set; }
    public bool HasRtmToken { get; set; }
    public bool HasSmallListerToken { get; set; }
    public bool HasSmallListerSyncList => !string.IsNullOrEmpty(SmallListerSyncListName);
    public string? SmallListerSyncListName { get; set; }
}