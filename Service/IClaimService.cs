using System.Collections.Generic;
using System.Threading.Tasks;
using MonthlyClaimSystem.Models;

namespace MonthlyClaimSystem.Services
{
    public interface IClaimService
    {
        Task<List<Claim>> GetPendingForCoordinatorAsync();
        Task VerifyAsync(int claimId, string verifierUserId);
        Task RejectAsync(int claimId, string verifierUserId, string reason);

        Task<List<Claim>> GetVerifiedForManagerAsync();
        Task ApproveAsync(int claimId, string approverUserId);
        Task ManagerRejectAsync(int claimId, string approverUserId, string reason);

        Task<List<Claim>> GetApprovedByManagerAsync(string managerUserId);
        Task<List<Claim>> GetVerifiedByCoordinatorAsync(string coordinatorUserId);
    }
}