using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MonthlyClaimSystem.Data;
using MonthlyClaimSystem.Models;
using MonthlyClaimSystem.Models.MonthlyClaimSystem.Models;

namespace MonthlyClaimSystem.Services
{
    public class ClaimService : IClaimService
    {
        private readonly ApplicationDbContext _db;

        public ClaimService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<List<Claim>> GetAllAsync()
        {
            return await _db.Claims.Include(c => c.Employee)
                                   .Include(c => c.Documents)
                                   .OrderByDescending(c => c.ClaimDate)
                                   .ToListAsync();
        }

        public async Task<Claim?> GetByIdAsync(int id)
        {
            return await _db.Claims.Include(c => c.Employee)
                                   .Include(c => c.Documents)
                                   .FirstOrDefaultAsync(c => c.ClaimId == id);
        }

        public async Task SaveClaimAsync(Claim claim)
        {
            claim.TotalPayment = claim.HoursWorked * claim.HourlyRate;
            claim.DateSubmitted = DateTime.UtcNow;

            _db.Claims.Add(claim);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateAsync(Claim updated)
        {
            var existing = await _db.Claims.FindAsync(updated.ClaimId);
            if (existing != null)
            {
                existing.LecturerName = updated.LecturerName;
                existing.EmployeeID = updated.EmployeeID;
                existing.HoursWorked = updated.HoursWorked;
                existing.HourlyRate = updated.HourlyRate;
                existing.TotalPayment = updated.HoursWorked * updated.HourlyRate;
                existing.Notes = updated.Notes;
                existing.VerifyStatus = updated.VerifyStatus;
                existing.ApproveStatus = updated.ApproveStatus;
                existing.ApprovedByUserId = updated.ApprovedByUserId;
                existing.VerifiedByUserId = updated.VerifiedByUserId;

                await _db.SaveChangesAsync();
            }
        }

        public async Task DeleteAsync(int id)
        {
            var claim = await _db.Claims.FindAsync(id);
            if (claim != null)
            {
                _db.Claims.Remove(claim);
                await _db.SaveChangesAsync();
            }
        }

        public bool IsClaimValid(Claim claim)
        {
            return claim.HoursWorked > 0 &&
                   claim.HourlyRate >= 100 &&
                   claim.HourlyRate <= 500;
        }

        //Workflow helpers
        public async Task VerifyAsync(int id, string verifierUserId)
        {
            var claim = await _db.Claims.FindAsync(id);
            if (claim != null)
            {
                claim.VerifyStatus = ClaimVerifyStatus.Verified;
                claim.VerifiedByUserId = verifierUserId;
                claim.VerifiedOn = DateTime.UtcNow;
                await _db.SaveChangesAsync();
            }
        }

        public async Task RejectAsync(int id, string verifierUserId, string reason)
        {
            var claim = await _db.Claims.FindAsync(id);
            if (claim != null)
            {
                claim.VerifyStatus = ClaimVerifyStatus.Rejected;
                claim.VerifiedByUserId = verifierUserId;
                claim.VerifiedOn = DateTime.UtcNow;
                claim.Notes = string.IsNullOrWhiteSpace(claim.Notes)
                    ? $"Coordinator rejected: {reason}"
                    : $"{claim.Notes}\nCoordinator rejected: {reason}";
                await _db.SaveChangesAsync();
            }
        }

        public async Task ApproveAsync(int id, string approverUserId)
        {
            var claim = await _db.Claims.FindAsync(id);
            if (claim != null && claim.VerifyStatus == ClaimVerifyStatus.Verified)
            {
                claim.ApproveStatus = ClaimApproveStatus.Approved;
                claim.ApprovedByUserId = approverUserId;
                claim.ApprovedOn = DateTime.UtcNow;
                await _db.SaveChangesAsync();
            }
        }

        public async Task ManagerRejectAsync(int id, string approverUserId, string reason)
        {
            var claim = await _db.Claims.FindAsync(id);
            if (claim != null)
            {
                claim.ApproveStatus = ClaimApproveStatus.Rejected;
                claim.ApprovedByUserId = approverUserId;
                claim.ApprovedOn = DateTime.UtcNow;
                claim.Notes = string.IsNullOrWhiteSpace(claim.Notes)
                    ? $"Manager rejected: {reason}"
                    : $"{claim.Notes}\nManager rejected: {reason}";
                await _db.SaveChangesAsync();
            }
        }

        //Interface methods
        public async Task<List<Claim>> GetPendingForCoordinatorAsync()
        {
            return await _db.Claims
                .Include(c => c.Employee)
                .Where(c => c.VerifyStatus == ClaimVerifyStatus.Pending)
                .OrderBy(c => c.DateSubmitted)
                .ToListAsync();
        }

        public async Task<List<Claim>> GetVerifiedForManagerAsync()
        {
            return await _db.Claims
                .Include(c => c.Employee)
                .Where(c => c.VerifyStatus == ClaimVerifyStatus.Verified &&
                            c.ApproveStatus == ClaimApproveStatus.Pending)
                .OrderBy(c => c.VerifiedOn)
                .ToListAsync();
        }

        public async Task<List<Claim>> GetApprovedByManagerAsync(string managerUserId)
        {
            return await _db.Claims
                .Include(c => c.Employee)
                .Where(c => c.ApproveStatus == ClaimApproveStatus.Approved &&
                            c.ApprovedByUserId == managerUserId)
                .OrderByDescending(c => c.ApprovedOn)
                .ToListAsync();
        }

        public async Task<List<Claim>> GetVerifiedByCoordinatorAsync(string coordinatorUserId)
        {
            return await _db.Claims
                .Include(c => c.Employee)
                .Where(c => c.VerifyStatus == ClaimVerifyStatus.Verified &&
                            c.VerifiedByUserId == coordinatorUserId)
                .OrderByDescending(c => c.VerifiedOn)
                .ToListAsync();
        }
    }
}