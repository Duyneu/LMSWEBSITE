// File: Services/Implementations/AiReviewerSuggestionService.cs
using Microsoft.Extensions.Logging;
using PostgraduateManagement.Models.Entities;
using PostgraduateManagement.Models.Enums;
using PostgraduateManagement.Repositories.Interfaces;
using PostgraduateManagement.Services.DTOs;
using PostgraduateManagement.Services.Interfaces;

namespace PostgraduateManagement.Services.Implementations
{
    /// <summary>
    /// Lớp triển khai dịch vụ gợi ý chuyên gia phản biện bằng thuật toán AI Heuristic (AiReviewerSuggestionService).
    /// THUẬT TOÁN ĐỐI SOÁT CHUYÊN MÔN:
    /// 1. Điểm tương đồng từ khóa lĩnh vực nghiên cứu (0 - 50 điểm)
    /// 2. Trọng số học hàm, học vị: GS (+30đ), PGS (+20đ), TS (+10đ)
    /// 3. Điểm cộng chuyên gia ngoài trường (+10đ nếu IsExternal)
    /// NGUYÊN TẮC BẮT BUỘC (ADVISORY ONLY):
    /// Kết quả hoàn toàn mang tính chất tham khảo độc lập cho cán bộ quản lý, không tự động lưu vết phân công vào DB.
    /// Loại trừ người hướng dẫn của chính đề tài để tránh xung đột lợi ích (conflict of interest).
    /// </summary>
    public class AiReviewerSuggestionService : IAiReviewerSuggestionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<AiReviewerSuggestionService> _logger;

        private const string AdvisoryNoteText = "Kết quả gợi ý phản biện AI mang tính chất khuyến nghị tham khảo độc lập, hỗ trợ cán bộ quản lý ra quyết định. Không tự động thực hiện phân công trong hệ thống.";

        public AiReviewerSuggestionService(IUnitOfWork unitOfWork, ILogger<AiReviewerSuggestionService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        /// <summary>
        /// Gợi ý danh sách chuyên gia phản biện phù hợp nhất cho đề tài sau đại học.
        /// </summary>
        public async Task<IEnumerable<ReviewerSuggestionDto>> SuggestReviewersAsync(int thesisId)
        {
            var thesis = await _unitOfWork.Theses.GetThesisWithFullDetailsAsync(thesisId);
            if (thesis == null)
            {
                throw new KeyNotFoundException($"Không tìm thấy đề tài với mã định danh {thesisId}.");
            }

            var allPersons = (await _unitOfWork.AcademicPersons.GetAllAsync()).ToList();

            // Loại trừ những người đang làm người hướng dẫn của chính đề tài này (xung đột lợi ích)
            var supervisorPersonIds = thesis.Assignments?
                .Where(a => a.Role == AssignmentRole.Supervisor)
                .Select(a => a.PersonId)
                .ToHashSet() ?? new HashSet<int>();

            var suggestions = new List<ReviewerSuggestionDto>();

            foreach (var person in allPersons)
            {
                // Bỏ qua người hướng dẫn
                if (supervisorPersonIds.Contains(person.Id))
                {
                    continue;
                }

                // 1. Tính điểm tương đồng từ khóa lĩnh vực nghiên cứu (0 - 50 điểm)
                var (fieldScore, matchedKeywords) = CalculateFieldScore(thesis.Field, person.ResearchField);

                // 2. Trọng số học hàm, học vị: GS (+30đ), PGS (+20đ), TS (+10đ)
                var degreeScore = person.AcademicDegree switch
                {
                    AcademicDegree.GS => 30,
                    AcademicDegree.PGS => 20,
                    AcademicDegree.TS => 10,
                    _ => 0
                };

                // 3. Điểm cộng chuyên gia ngoài trường (+10đ nếu IsExternal)
                var externalScore = person.IsExternal ? 10 : 0;

                // Tổng điểm phù hợp (tối đa 100 điểm)
                var totalScore = Math.Min(100, fieldScore + degreeScore + externalScore);

                // Tạo giải trình tường minh lý do gợi ý
                var degreeLabel = person.AcademicDegree switch
                {
                    AcademicDegree.GS => "Học hàm GS",
                    AcademicDegree.PGS => "Học hàm PGS",
                    AcademicDegree.TS => "Học vị TS",
                    _ => "Học vị"
                };

                var externalText = person.IsExternal ? " + Chuyên gia ngoài trường (10đ)" : " + Cán bộ trong trường (0đ)";
                var matchedKeywordText = matchedKeywords.Count > 0 ? string.Join(", ", matchedKeywords) : thesis.Field;

                var reason = $"Trùng khớp lĩnh vực '{matchedKeywordText}' ({fieldScore}đ) + {degreeLabel} ({degreeScore}đ){externalText} => Điểm phù hợp: {totalScore}%";

                suggestions.Add(new ReviewerSuggestionDto
                {
                    PersonId = person.Id,
                    FullName = person.FullName,
                    SharedPersonCode = person.SharedPersonCode,
                    AcademicDegree = person.AcademicDegree,
                    ResearchField = person.ResearchField,
                    IsExternal = person.IsExternal,
                    Workplace = person.Workplace,
                    Email = person.Email,
                    MatchScore = totalScore,
                    FieldScore = fieldScore,
                    DegreeScore = degreeScore,
                    ExternalScore = externalScore,
                    RecommendationReason = reason,
                    IsAdvisoryOnly = true,
                    AdvisoryNote = AdvisoryNoteText
                });
            }

            // Ưu tiên hiển thị chuyên gia có điểm trùng khớp lĩnh vực hoặc tổng điểm cao
            var rankedList = suggestions
                .Where(s => s.FieldScore > 0 || s.MatchScore >= 20)
                .OrderByDescending(s => s.MatchScore)
                .ThenByDescending(s => s.DegreeScore)
                .ThenByDescending(s => s.ExternalScore)
                .ThenBy(s => s.FullName)
                .ToList();

            _logger.LogInformation("Gợi ý phản biện AI cho đề tài {Code}: tìm thấy {Count} chuyên gia phù hợp.",
                thesis.Code, rankedList.Count);

            return rankedList;
        }

        /// <summary>
        /// Tính điểm tương đồng từ khóa giữa lĩnh vực của đề tài và lĩnh vực của chuyên gia (0 - 50 điểm).
        /// </summary>
        private static (int Score, List<string> MatchedKeywords) CalculateFieldScore(string thesisField, string personField)
        {
            if (string.IsNullOrWhiteSpace(thesisField) || string.IsNullOrWhiteSpace(personField))
            {
                return (0, new List<string>());
            }

            var tNorm = thesisField.Trim().ToLowerInvariant();
            var pNorm = personField.Trim().ToLowerInvariant();

            // Trường hợp 1: Trùng khớp chính xác hoặc chứa toàn bộ chuỗi
            if (tNorm == pNorm)
            {
                return (50, new List<string> { thesisField.Trim() });
            }

            if (tNorm.Contains(pNorm))
            {
                return (45, new List<string> { personField.Trim() });
            }

            if (pNorm.Contains(tNorm))
            {
                return (45, new List<string> { thesisField.Trim() });
            }

            // Trường hợp 2: Giao thoa từ khóa có nghĩa
            var stopWords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "và", "của", "cho", "trong", "các", "những", "về", "nghiên", "cứu",
                "đề", "tài", "ứng", "dụng", "hệ", "thống", "phát", "triển",
                "and", "or", "of", "in", "the", "for", "on", "with", "system"
            };

            var delimiters = new[] { ' ', ',', ';', '/', '-', '&', '(', ')' };

            var tTokens = tNorm.Split(delimiters, StringSplitOptions.RemoveEmptyEntries)
                               .Where(w => w.Length > 2 && !stopWords.Contains(w))
                               .Distinct()
                               .ToList();

            var pTokens = pNorm.Split(delimiters, StringSplitOptions.RemoveEmptyEntries)
                               .Where(w => w.Length > 2 && !stopWords.Contains(w))
                               .Distinct()
                               .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var matched = tTokens.Where(t => pTokens.Contains(t)).ToList();

            if (matched.Count == 0)
            {
                return (0, new List<string>());
            }

            // Điểm số tăng dần theo số lượng từ khóa trùng khớp (tối đa 50đ)
            var score = matched.Count switch
            {
                1 => 25,
                2 => 35,
                3 => 45,
                _ => 50
            };

            return (score, matched);
        }
    }
}
