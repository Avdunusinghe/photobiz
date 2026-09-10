namespace Photobiz.Application.Common.Models
{
    public record PagedResult<T>(
        IReadOnlyList<T> Items,
        int TotalCount,
        int PageNumber,
        int PageSize)
    {
        public int TotalPages => PageSize <= 0
            ? 0
            : (int)Math.Ceiling(TotalCount / (double)PageSize);

        public bool HasPreviousPage => PageNumber > 1;

        public bool HasNextPage => PageNumber < TotalPages;

        public static PagedResult<T> Empty(int pageNumber, int pageSize) =>
            new([], 0, pageNumber, pageSize);
    }
}
