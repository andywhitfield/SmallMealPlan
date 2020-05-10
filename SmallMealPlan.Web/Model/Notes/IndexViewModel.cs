using Microsoft.AspNetCore.Http;

namespace SmallMealPlan.Web.Model.Notes
{
    public class IndexViewModel : BaseViewModel
    {
        public IndexViewModel(HttpContext context) : base(context)
        {
            SelectedArea = SmpArea.Notes;
        }
        
        public string Notes { get; set; }
    }
}