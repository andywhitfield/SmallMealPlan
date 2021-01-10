using System;

namespace SmallMealPlan.SmallLister
{
    public class SmallListerItem
    {
        public string ItemId { get; set; }
        public string Description { get; set; }
        public DateTime? DueDate { get; set; }
        public string Notes { get; set; }
    }
}