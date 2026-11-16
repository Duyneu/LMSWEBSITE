// File: Repositories/Implementations/Repository.cs
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using PostgraduateManagement.Data;
using PostgraduateManagement.Repositories.Interfaces;

namespace PostgraduateManagement.Repositories.Implementations
{
    /// <summary>
    /// Triển khai kho lưu trữ dữ liệu tổng quát (Generic Repository) sử dụng Entity Framework Core.
    /// Tối ưu hóa hiệu năng truy vấn chỉ đọc với AsNoTracking().
    /// </summary>
    /// <typeparam name="T">Loại thực thể dữ liệu</typeparam>
    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly AppDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public Repository(AppDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _dbSet = _context.Set<T>();
        }

        /// <summary>
        /// Lấy toàn bộ danh sách thực thể với chế độ AsNoTracking để tối ưu tài nguyên
        /// </summary>
        public virtual async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _dbSet.AsNoTracking().ToListAsync();
        }

        /// <summary>
        /// Tìm thực thể theo khóa chính Id
        /// </summary>
        public virtual async Task<T?> GetByIdAsync(int id)
        {
            return await _dbSet.FindAsync(id);
        }

        /// <summary>
        /// Lọc danh sách thực thể theo biểu thức điều kiện với AsNoTracking
        /// </summary>
        public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.Where(predicate).AsNoTracking().ToListAsync();
        }

        /// <summary>
        /// Thêm mới một bản ghi thực thể vào DbSet
        /// </summary>
        public virtual async Task AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
        }

        /// <summary>
        /// Thêm mới hàng loạt bản ghi thực thể vào DbSet
        /// </summary>
        public virtual async Task AddRangeAsync(IEnumerable<T> entities)
        {
            await _dbSet.AddRangeAsync(entities);
        }

        /// <summary>
        /// Đánh dấu thực thể đã được chỉnh sửa để lưu thay đổi
        /// </summary>
        public virtual void Update(T entity)
        {
            _dbSet.Update(entity);
        }

        /// <summary>
        /// Đánh dấu thực thể cần xóa khỏi DbSet
        /// </summary>
        public virtual void Remove(T entity)
        {
            _dbSet.Remove(entity);
        }
    }
}
