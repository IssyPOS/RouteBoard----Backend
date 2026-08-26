namespace POSShopTicketing.Shared.Wrappers;

public class PaginatedResponse<T> : ApiResponse<List<T>>
{
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;

    public static PaginatedResponse<T> Create(List<T> data, int pageNumber, int pageSize, int totalCount)
    {
        return new PaginatedResponse<T>
        {
            Succeeded = true,
            Data = data,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        };
    }
}
