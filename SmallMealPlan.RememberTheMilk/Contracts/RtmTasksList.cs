using System.Collections.Generic;

namespace SmallMealPlan.RememberTheMilk.Contracts;

public class RtmTasksList
{
    public string? Id { get; set; }
    public IEnumerable<RtmTaskSeries> TaskSeries { get; set; } = new List<RtmTaskSeries>();
}