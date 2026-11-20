using WebEventCompany.Models;

namespace WebEventCompany.Services;

public interface ILuckyDrawUserService
{
    Task<IReadOnlyList<LuckyDrawParticipant>> GetParticipantsAsync(CancellationToken cancellationToken = default);
}
