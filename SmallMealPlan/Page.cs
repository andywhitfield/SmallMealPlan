namespace SmallMealPlan;

public class Page(int pageNumber, bool isSelected = false, bool isNextPageSkipped = false)
{
    public int PageNumber { get; } = pageNumber;
    public bool IsSelected { get; } = isSelected;
    public bool IsNextPageSkipped { get; } = isNextPageSkipped;

    public override string ToString() => $"{PageNumber}{(IsSelected ? " (selected)" : "")}";
    public override int GetHashCode() => PageNumber;
    public override bool Equals(object? obj)
    {
        if (obj is Page other)
            return PageNumber == other.PageNumber && IsSelected == other.IsSelected;
        return false;
    }
}