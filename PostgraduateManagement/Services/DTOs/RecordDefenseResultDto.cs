// File: Services/DTOs/RecordDefenseResultDto.cs
using System.ComponentModel.DataAnnotations;

namespace PostgraduateManagement.Services.DTOs
{
    /// <summary>
    /// Đối tượng truyền tải dữ liệu nhập kết quả đánh giá phiên bảo vệ đề tài.
    /// </summary>
    public class RecordDefenseResultDto
    {
        /// <summary>
        /// Mã định danh phiên bảo vệ
        /// </summary>
        [Required(ErrorMessage = "Mã phiên bảo vệ không được để trống")]
        public int SessionId { get; set; }

        /// <summary>
        /// Điểm số đánh giá trung bình của hội đồng (thang điểm 10.0)
        /// </summary>
        [Required(ErrorMessage = "Vui lòng nhập điểm đánh giá")]
        [Range(0.0, 10.0, ErrorMessage = "Điểm đánh giá phải từ 0.0 đến 10.0")]
        public decimal Score { get; set; }

        /// <summary>
        /// Kết luận: Đạt (true) hoặc Không đạt (false)
        /// </summary>
        public bool Pass { get; set; } = true;

        /// <summary>
        /// Nhận xét, đánh giá chi tiết và kết luận của hội đồng
        /// </summary>
        [Required(ErrorMessage = "Vui lòng nhập nhận xét của hội đồng")]
        [StringLength(2000, ErrorMessage = "Nhận xét tối đa 2000 ký tự")]
        public string Comments { get; set; } = string.Empty;
    }
}
