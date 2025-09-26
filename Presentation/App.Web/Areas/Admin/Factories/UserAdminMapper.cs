using App.Core.Domain.Users;
using App.Services.Security;
using App.Web.Areas.Admin.Models;

namespace App.Web.Areas.Admin.Factories
{
    public static class UserAdminMapper
    {
        public static UserModel ToModel(this AppUser entity)
        {
            if (entity == null) return null;

            return new UserModel
            {
                Id = entity.Id,
                UserName = entity.Username,
                Email = entity.Email,
                Active = entity.IsActive,
                IsLockedOut = entity.IsLockedOut,
                FailedLoginAttempts = entity.FailedLoginAttempts,
                LastLoginDateUtc = entity.LastLoginDateUtc,
                CreatedOnUtc = entity.CreatedOnUtc,
                UpdatedOnUtc = entity.UpdatedOnUtc
            };
        }

        public static AppUser ToEntity(this UserModel model, IEncryptionService encryptionService, AppUser entity = null)
        {
            if (model == null) return entity;

            if (entity == null)
                entity = new AppUser { CreatedOnUtc = DateTime.UtcNow };

            entity.Username = model.UserName;
            entity.Email = model.Email;
            entity.IsActive = model.Active;
            entity.IsLockedOut = model.IsLockedOut;
            entity.FailedLoginAttempts = model.FailedLoginAttempts;
            entity.UpdatedOnUtc = DateTime.UtcNow;

            if (!string.IsNullOrEmpty(model.Password))
            {
                var saltKey = encryptionService.CreateSaltKey(16);
                entity.PasswordSalt = saltKey;
                entity.PasswordHash = encryptionService.CreatePasswordHash(model.Password, saltKey);
            }

            return entity;
        }
    }
}
