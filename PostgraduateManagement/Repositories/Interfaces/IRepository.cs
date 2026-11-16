// File: Repositories/Interfaces/IRepository.cs
using System.Linq.Expressions;

namespace PostgraduateManagement.Repositories.Interfaces
{
    /// <summary>
    /// Giao diện kho lưu trữ dữ liệu tổng quát (Generic Repository) cho các thực thể trong hệ thống.
    /// Định nghĩa các thao tác CRUD cơ bản và tìm kiếm linh hoạt qua biểu thức Lambda.
    /// </summary>
    /// <typeparam name="T">Loại thực thể dữ liệu</typeparam>
    public interface IRepository<T> where T : class
    {
        /// <summary>
        /// Lấy toàn bộ danh sách thực thể trong cơ sở dữ liệu
        /// </summary>
        Task<IEnumerable<T>> GetAllAsync();

        /// <summary>
        /// Tìm thực thể theo khóa chính định danh (Id)
        /// </summary>
        /// <param name="id">Mã định danh khóa chính</param>
        Task<T?> GetByIdAsync(int id);

        /// <summary>
        /// Tìm kiếm danh sách thực thể thỏa mãn biểu thức điều kiện
        /// </summary>
        /// <param name="predicate">Biểu thức điều kiện lọc dữ liệu</param>
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// Thêm mới một thực thể vào ngữ cảnh cơ sở dữ liệu
        /// </summary>
        /// <param name="entity">Đối tượng thực thể cần thêm mới</param>
        Task AddAsync(T entity);

        /// <summary>
        /// Thêm danh sách nhiều thực thể vào ngữ cảnh cơ sở dữ liệu
        /// </summary>
        /// <param name="entities">Tập hợp các thực thể cần thêm mới</param>
        Task AddRangeAsync(IEnumerable<T> entities);

        /// <summary>
        /// Đánh dấu cập nhật thông tin thực thể
        /// </summary>
        /// <param name="entity">Đối tượng thực thể cần cập nhật</param>
        void Update(T entity);

        /// <summary>
        /// Đánh dấu xóa thực thể khỏi cơ sở dữ liệu
        /// </summary>
        /// <param name="entity">Đối tượng thực thể cần xóa</param>
        void Remove(T entity);
    }
}
