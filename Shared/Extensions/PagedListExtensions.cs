using Shared.Results;

namespace Shared.Extensions
{
    public static class PagedListExtensions
    {
        public static object CreatePaginationMetadata<TSource>(this PagedList<TSource> source)
        {
            var paginationMetadata = new
            {
                pageSize = source.PageSize,
                currentPage = source.CurrentPage,
                hasNext = source.HasNext,
                hasPrevious = source.HasPrevious,
                totalPages = source.TotalPages,
                totalCount = source.TotalCount
            };

            return paginationMetadata;
        }
    }
}
