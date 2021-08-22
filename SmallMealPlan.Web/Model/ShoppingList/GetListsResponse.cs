using System.Collections.Generic;

namespace SmallMealPlan.Web.Model.ShoppingList
{
    public class GetListsResponse
    {
        public IEnumerable<ListItem> Options { get; set; } = new List<ListItem>();

        public class ListItem
        {
            public ListItem(string value, string text, bool isSelected)
            {
                Value = value;
                Text = text;
                IsSelected = isSelected;
            }
            public string Value { get; }
            public string Text { get; }
            public bool IsSelected { get; }
        }
    }
}