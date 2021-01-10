using System.Collections.Generic;

namespace SmallMealPlan.SmallLister
{
    public class SmallListerListsResponse
    {
        public IEnumerable<SmallListerList> Lists { get; set; }
    }
}