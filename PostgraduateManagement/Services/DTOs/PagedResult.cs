// File: Services/DTOs/PagedResult.cs
namespace PostgraduateManagement.Services.DTOs
{
    /// <summary>
    /// Đối tượng chứa kết quả phân trang dùng chung cho các truy vấn danh sách.
    /// Bao gồm tập dữ liệu và các siêu dữ liệu phân trang (tổng số, trang hiện tại, tổng trang).
    /// </summary>
    /// <typeparam name="T">Kiểu dữ liệu của các phần tử trong danh sách</typeparam>
    public class PagedResult<T>
    {
        /// <summary>
        /// Danh sách các phần tử dữ liệu thuộc trang hiện tại
        /// </summary>
        public IEnumerable<T> Items { get; set; } = new List<T>();

        /// <summary>
        /// Tổng số lượng bản ghi thỏa mãn điều kiện lọc
        /// </summary>
        public int TotalItems { get; set; }

        /// <summary>
        /// Số thứ tự trang hiện tại (bắt đầu từ 1)
        /// </summary>
        public int PageNumber { get; set; } = 1;

        /// <summary>
        /// Số lượng bản ghi tối đa trên một trang
        /// </summary>
        public int PageSize { get; set; } = 10;

        /// <summary>
        /// Tổng số trang được tính toán dựa trên tổng số bản ghi và kích thước trang
        /// </summary>
        public int TotalPages => PageSize > 0 ? (int)Math.Ceiling(TotalItems / (double)PageSize) : 0;

        /// <summary>
        /// Cờ kiểm tra có trang trước đó hay không
        /// </summary>
        public bool HasPreviousPage => PageNumber > 1;

        /// <summary>
        /// Cờ kiểm tra có trang tiếp theo hay không
        /// </summary>
        public bool HasNextPage => PageNumber < TotalPages;

        /// <summary>
        /// Khởi tạo mặc định
        /// </summary>
        public PagedResult()
        {
        }

        /// <summary>
        /// Khởi tạo với đầy đủ thông số phân trang
        /// </summary>
        public PagedResult(IEnumerable<T> items, int totalItems, int pageNumber, int pageSize)
        {
            Items = items;
            TotalItems = totalItems;
            PageNumber = pageNumber;
            PageSize = pageSize;
        }
    }
}
