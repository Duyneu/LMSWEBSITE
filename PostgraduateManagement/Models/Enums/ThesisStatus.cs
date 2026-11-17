// File: Models/Enums/ThesisStatus.cs
namespace PostgraduateManagement.Models.Enums
{
    /// <summary>
    /// Trạng thái vòng đời của đề tài sau đại học (Luận văn, Luận án, Đề án).
    /// </summary>
    public enum ThesisStatus
    {
        /// <summary>
        /// Dự thảo / Bản nháp đề cương đề tài
        /// </summary>
        Draft = 0,

        /// <summary>
        /// Đã nộp đăng ký đề tài lên phòng Sau đại học
        /// </summary>
        Registered = 1,

        /// <summary>
        /// Đã phê duyệt đề tài và chỉ định người hướng dẫn
        /// </summary>
        Approved = 2,

        /// <summary>
        /// Đang trong quá trình nghiên cứu và thực hiện đề tài
        /// </summary>
        InProgress = 3,

        /// <summary>
        /// Đã nộp toàn văn và hồ sơ xin phản biện & bảo vệ
        /// </summary>
        SubmittedForReview = 4,

        /// <summary>
        /// Đã hoàn thành phiên bảo vệ trước hội đồng
        /// </summary>
        Defended = 5,

        /// <summary>
        /// Đã hoàn tất chỉnh sửa sau bảo vệ và nộp lưu chiểu đầy đủ
        /// </summary>
        Completed = 6,

        /// <summary>
        /// Đã lưu trữ hồ sơ: bước cuối của quy trình, đề tài chỉ còn xem, không chỉnh sửa
        /// </summary>
        Archived = 7
    }
}
