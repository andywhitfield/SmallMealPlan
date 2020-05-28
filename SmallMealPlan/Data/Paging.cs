using System;

namespace SmallMealPlan.Data
{
    public static class Paging
    {
        public static (int PageIndex, int PageCount) GetPageInfo(int totalCount, int pageSize, int pageNumber)
        {
            var pageCount = totalCount / pageSize;
            if (totalCount % pageSize != 0) pageCount++;
            var pageIndex = Math.Min(pageCount - 1, Math.Max(0, pageNumber - 1));
            return (pageIndex, pageCount);
        }
    }
}