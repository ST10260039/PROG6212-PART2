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

        public static void SaveClaim(Claim claim)
        {
            if (claim.ClaimId == 0)
                claim.ClaimId = _claims.Count > 0 ? _claims.Max(c => c.ClaimId) + 1 : 1;

            claim.TotalPayment = claim.HoursWorked * claim.HourlyRate;
            _claims.Add(claim);
        }

        public static void Update(Claim updated)
        {
            var index = _claims.FindIndex(c => c.ClaimId == updated.ClaimId);
            if (index >= 0)
            {
                updated.TotalPayment = updated.HoursWorked * updated.HourlyRate;
                _claims[index] = updated;
            }
        }

        public static void Delete(int id)
        {
            var claim = GetById(id);
            if (claim != null)
                _claims.Remove(claim);
        }

        public static bool IsClaimValid(Claim claim)
        {
            return claim.HoursWorked > 0 && claim.HourlyRate >= 100 && claim.HourlyRate <= 500;
        }
    }
}