using System.Collections.Generic;
using System.Linq;

namespace SmallMealPlan
{
    public class Pagination
    {
        public const string SortByName = "name";
        public const string SortByRecentlyUsed = "recent";

        public Pagination(int pageNumber, int pageCount, string sort, string filter)
        {
            PageNumber = pageNumber;
            PageCount = pageCount;
            Sort = sort;
            Filter = filter;

            if (PageCount <= 10)
            {
                Pages = Enumerable.Range(1, PageCount).Select(p => new Page(p, pageNumber == p));
            }
            else
            {
                var pages = new List<int>();
                pages.AddRange(Enumerable.Range(pageNumber - 2, 6));
                Normalise(pages);

                if (pageNumber - 4 <= 1)
                {
                    pages.InsertRange(0, Enumerable.Range(pages[0] - 2, 2));
                    Normalise(pages);
                    pages.Add(PageCount);
                }
                else if (pages[pages.Count - 1] + 2 >= PageCount)
                {
                    pages.AddRange(Enumerable.Range(pages[pages.Count - 1] + 1, 2));
                    Normalise(pages);
                    pages.Insert(0, 1);
                }
                else
                {
                    pages.Insert(0, 1);
                    pages.Add(PageCount);
                }

                Pages = pages.Select((p, idx) => new Page(p, pageNumber == p, idx + 1 < pages.Count && pages[idx + 1] != p + 1));

                void Normalise(List<int> pgs)
                {
                    var adjust = pgs[0] < 1
                        ? ((pgs[0] * -1) + 1)
                        : (pgs[pgs.Count - 1] > PageCount
                            ? (PageCount - pgs[pgs.Count - 1])
                            : 0);

                    if (adjust != 0)
                        for (var i = 0; i < pgs.Count; i++)
                            pgs[i] += adjust;
                }
            }
        }

        public int PageNumber { get; }
        public int PageCount { get; }
        public string Sort { get; }
        public bool SortedByName => Sort == SortByName;
        public bool SortedByRecentlyUsed => !SortedByName;
        public string Filter { get; }
        public IEnumerable<Page> Pages { get; }
    }
}