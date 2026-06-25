using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMS.Domain
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; private set; }
        public string LastName { get; private set; }
        public bool IsActive { get; private set; } = true;

        public DateTime CreatedAt { get; private set; }
        public Guid? CreatedBy { get; private set; }
        public DateTime? UpdatedAt { get; private set; }
        public Guid? UpdatedBy { get; private set; }

        public bool IsDeleted { get; private set; } = false;
        public DateTime? DeletedAt { get; private set; }
        public Guid? DeletedBy { get; private set; }

        // Refresh Token
        public string? RefreshToken { get; private set; }
        public DateTime? RefreshTokenExpiryTime { get; private set; }


        // Parameterless constructor for EF Core
        private ApplicationUser() : base() { }

        // Domain Constructor
        public static ApplicationUser Register(string email, string firstName, string lastName)
        {
            if (string.IsNullOrWhiteSpace(firstName)) throw new ArgumentException("First name is required.");
            if (string.IsNullOrWhiteSpace(lastName)) throw new ArgumentException("Last name is required.");

            var user = new ApplicationUser
            {
                Email = email,
                UserName = email,
                FirstName = firstName,
                LastName = lastName,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            return user;
        }

        #region Domain Methods
        public void UpdateRefreshToken(string refreshToken, DateTime expiryTime)
        {
            RefreshToken = refreshToken;
            RefreshTokenExpiryTime = expiryTime;
        }

        public void UpdateName(string firstName, string lastName)
        {
            if (string.IsNullOrWhiteSpace(firstName)) throw new ArgumentException("First name is required.");
            if (string.IsNullOrWhiteSpace(lastName)) throw new ArgumentException("Last name is required.");

            FirstName = firstName;
            LastName = lastName;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Deactivate()
        {
            IsActive = false;
            UpdatedAt = DateTime.UtcNow;
        }

        public void SoftDelete(Guid deletedBy)
        {
            IsDeleted = true;
            DeletedAt = DateTime.UtcNow;
            DeletedBy = deletedBy;
        } 
        #endregion

    }
}
