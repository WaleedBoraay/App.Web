using System;
using System.Collections.Generic;

namespace App.Core.Domain.Admin.Models.Users
{
    public class UsersListQuery
    {
        public string Search { get; set; }
        public int? InstitutionId { get; set; }
        public int? RoleId { get; set; }
        public bool? IsActive { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    public class UserListModel
    {
        public UsersListQuery Query { get; set; }
        public IList<UserSummaryModel> Items { get; set; } = new List<UserSummaryModel>();
        public int TotalCount { get; set; }
        public IList<KeyValuePair<int, string>> AvailableRoles { get; set; } = new List<KeyValuePair<int, string>>();
    }

    public class UserSummaryModel
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public bool IsActive { get; set; }
        public int InstitutionId { get; set; }
        public string InstitutionName { get; set; }
        public IList<string> Roles { get; set; } = new List<string>();
        public DateTime? LastLoginDateUtc { get; set; }
        public DateTime CreatedOnUtc { get; set; }
        public DateTime? LastLoginUtc { get; internal set; }
    }

    public class ResetPasswordModel
    {
        public int UserId { get; set; }
        public string NewPassword { get; set; }
        public bool ForceChangeOnNextLogin { get; set; }
    }

    public class AssignUserRolesModel
    {
        public int UserId { get; set; }
        public IList<int> RoleIds { get; set; } = new List<int>();
    }

    public class ExportUserRow
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Institution { get; set; }
        public string Roles { get; set; }
        public bool IsActive { get; set; }
        public DateTime? LastLoginDateUtc { get; set; }
        public DateTime CreatedOnUtc { get; set; }
    }
}
