using System.Collections.Generic;
using System.Linq;
using MonthlyClaimSystem.Models;

namespace MonthlyClaimSystem.Services
{
    public static class ClaimService
    {
        private static List<Claim> _claims = new();

        public static List<Claim> GetAll() => _claims;

        public static Claim? GetById(int id) => _claims.FirstOrDefault(c => c.ClaimId == id);

        public static void SaveClaim(Claim claim) => _claims.Add(claim);

        public static void Update(Claim updated)
        {
            var index = _claims.FindIndex(c => c.ClaimId == updated.ClaimId);
            if (index >= 0) _claims[index] = updated;
        }
    }
}