namespace SmallMealPlan.Web.Model.ShoppingList;

public class IndexViewModel : BaseViewModel
{
    public IndexViewModel(HttpContext context) : base(context) => SelectedArea = SmpArea.ShoppingList;

    public IEnumerable<ShoppingListItemModel> MyList { get; set; } = [];
    public IEnumerable<MealItemModel> MealFromPlannerList { get; set; } = [];
    public IEnumerable<ShoppingListItemModel> BoughtList { get; set; } = [];
    public string? RegularOrBought { get; set; }
    public Pagination? BoughtListPagination { get; set; }
    public bool RegularOrBoughtShowAll { get; set; }
    public bool HasRtmToken { get; set; }
    public bool HasSmallListerToken { get; set; }
    public bool HasSmallListerSyncList => !string.IsNullOrEmpty(SmallListerSyncListName);
    public string? SmallListerSyncListName { get; set; }
}