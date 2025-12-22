using Microsoft.EntityFrameworkCore;

namespace Shared.Results
{
    public class PagedList<T> : List<T>
    {
        public int PageSize { get; private set; }

        public int CurrentPage { get; private set; }

        public int TotalPages { get; private set; }

        public int TotalCount { get; private set; }

        public bool HasPrevious => CurrentPage > 1;

        public bool HasNext => CurrentPage < TotalPages;

        public PagedList(List<T> items, int count, int pageNumber, int pageSize)
        {
            TotalCount = count;
            PageSize = pageSize;
            TotalPages = (int)Math.Ceiling(count / (double)pageSize);
            CurrentPage = pageNumber;
            AddRange(items);
        }

        public static async Task<PagedList<T>> Create(IQueryable<T> source, int pageNumber, int pageSize)
        {
            var count = source.Count();
            var items = await source
                .Skip(pageSize * (pageNumber - 1))
                .Take(pageSize)
                .ToListAsync();
            return new PagedList<T>(items, count, pageNumber, pageSize);
        }
    }
}
