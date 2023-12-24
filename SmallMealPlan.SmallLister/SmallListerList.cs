using System.Collections.Generic;

namespace SmallMealPlan.SmallLister;

public class SmallListerList
{
    public string? ListId { get; set; }
    public string? Name { get; set; }
    public IEnumerable<SmallListerItem>? Items { get; set; }
}