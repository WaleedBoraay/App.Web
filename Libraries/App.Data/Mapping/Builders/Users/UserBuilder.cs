using FluentMigrator.Builders.Create.Table;
using App.Core.Builders;
using App.Core.Domain.Users;

namespace App.Data.Mapping.Builders.Users
{
    /// <summary>
    /// Represents the User entity builder
    /// </summary>
    public partial class UserBuilder : EntityBuilder<AppUser>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(AppUser.Id)).AsInt32().PrimaryKey().Identity()

                .WithColumn(nameof(AppUser.Username)).AsString(256).Nullable()
                .WithColumn(nameof(AppUser.Email)).AsString(256).Nullable()
                .WithColumn(nameof(AppUser.PasswordHash)).AsString(512).Nullable()
                .WithColumn(nameof(AppUser.PasswordSalt)).AsString(512).Nullable()
                .WithColumn(nameof(AppUser.PasswordFormatId)).AsInt32().Nullable()

                .WithColumn(nameof(AppUser.IsActive)).AsBoolean().Nullable()
                .WithColumn(nameof(AppUser.IsLockedOut)).AsBoolean().Nullable()
                .WithColumn(nameof(AppUser.FailedLoginAttempts)).AsInt32().Nullable()
                .WithColumn(nameof(AppUser.LockoutEndUtc)).AsDateTime().Nullable()

                .WithColumn(nameof(AppUser.CreatedOnUtc)).AsDateTime().Nullable()
                .WithColumn(nameof(AppUser.UpdatedOnUtc)).AsDateTime().Nullable()
                .WithColumn(nameof(AppUser.LastLoginDateUtc)).AsDateTime().Nullable()
                .WithColumn(nameof(AppUser.DeactivatedOnUtc)).AsDateTime().Nullable()

                .WithColumn(nameof(AppUser.InstitutionId)).AsInt32().Nullable()
                .WithColumn(nameof(AppUser.RegistrationId)).AsInt32().Nullable()

                // Organization hierarchy (new)
                .WithColumn(nameof(AppUser.DepartmentId)).AsInt32().Nullable()
                .WithColumn(nameof(AppUser.UnitId)).AsInt32().Nullable()
                .WithColumn(nameof(AppUser.SubUnitId)).AsInt32().Nullable()

                .WithColumn(nameof(AppUser.PasswordResetToken)).AsString(256).Nullable()
                .WithColumn(nameof(AppUser.PasswordResetTokenExpiry)).AsDateTime().Nullable()
                .WithColumn(nameof(AppUser.TwoFactorSecret)).AsString(256).Nullable()
                .WithColumn(nameof(AppUser.TwoFactorEnabled)).AsBoolean().Nullable();
        }
    }
}
