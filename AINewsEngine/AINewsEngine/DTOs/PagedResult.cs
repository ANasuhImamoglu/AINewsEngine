namespace AINewsEngine.DTOs
{
    // Sayfa bilgilerini tutan sınıf
    public class PaginationInfo
    {
        public int TotalItems { get; set; }
        public int PageSize { get; set; }
        public int PageNumber { get; set; }
        public int TotalPages { get; set; }
        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;
    }

    // Hem sayfa verisini hem de sayfa bilgilerini içeren genel (generic) sınıf
    public class PagedResult<T>
    {
        public List<T> Items { get; set; } = new List<T>();
        public PaginationInfo Pagination { get; set; } = new PaginationInfo();
    }
}