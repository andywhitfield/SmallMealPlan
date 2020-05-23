namespace SmallMealPlan
{
    public class Page
    {
        public Page(int pageNumber, bool isSelected = false, bool isNextPageSkipped = false)
        {
            PageNumber = pageNumber;
            IsSelected = isSelected;
            IsNextPageSkipped = isNextPageSkipped;
        }

        public int PageNumber { get; }
        public bool IsSelected { get; }
        public bool IsNextPageSkipped { get; }

        public override string ToString() => $"{PageNumber}{(IsSelected ? " (selected)" : "")}";
        public override int GetHashCode() => PageNumber;
        public override bool Equals(object obj)
        {
            if (obj is Page other)
                return PageNumber == other.PageNumber && IsSelected == other.IsSelected;
            return false;
        }
    }
}