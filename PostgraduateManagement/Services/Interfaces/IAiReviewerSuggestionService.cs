// File: Services/Interfaces/IAiReviewerSuggestionService.cs
using PostgraduateManagement.Services.DTOs;

namespace PostgraduateManagement.Services.Interfaces
{
    /// <summary>
    /// Giao diện dịch vụ gợi ý phản biện AI độc lập cho đề tài sau đại học.
    /// Áp dụng thuật toán heuristic tính điểm tương đồng lĩnh vực nghiên cứu và học hàm/học vị.
    /// Kết quả chỉ mang tính chất tham khảo (Advisory Only), không tự động phân công vào cơ sở dữ liệu.
    /// </summary>
    public interface IAiReviewerSuggestionService
    {
        /// <summary>
        /// Gợi ý danh sách các chuyên gia phản biện phù hợp nhất cho một đề tài sau đại học.
        /// Tính điểm dựa trên:
        /// 1. Mức độ trùng khớp từ khóa lĩnh vực nghiên cứu (0 - 50 điểm)
        /// 2. Trọng số học hàm, học vị: GS (+30đ), PGS (+20đ), TS (+10đ)
        /// 3. Điểm cộng chuyên gia ngoài trường (+10đ)
        /// Sắp xếp giảm dần theo tổng điểm và sinh giải trình lý do khuyến nghị rõ ràng.
        /// </summary>
        /// <param name="thesisId">Mã định danh đề tài cần gợi ý phản biện</param>
        /// <returns>Danh sách khuyến nghị chuyên gia phản biện</returns>
        Task<IEnumerable<ReviewerSuggestionDto>> SuggestReviewersAsync(int thesisId);
    }
}
