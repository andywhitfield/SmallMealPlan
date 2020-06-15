using System.Collections.Generic;

namespace SmallMealPlan.RememberTheMilk.Contracts
{
    public class RtmTasks
    {
        public IEnumerable<RtmTasksList> List { get; set; } = new List<RtmTasksList>();
    }
}