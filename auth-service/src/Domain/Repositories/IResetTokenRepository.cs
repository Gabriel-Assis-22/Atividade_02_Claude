using Domain.Entities;

namespace Domain.Repositories;

public interface IResetTokenRepository
{
    Task CreateAsync(ResetToken token);
    Task<ResetToken?> GetByTokenAsync(string token);
    Task MarkAsUsedAsync(string token);
}
