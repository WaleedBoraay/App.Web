namespace App.Web.Api.DTOs
{
    public class SectorsModel
    {
        public int Id { get; set; }
        public string? SectorName { get; set; }
        public string? SectorDescription { get; set; }

		//Department Info
		public int DepartmentId { get; set; }
        public string? DepartmentName { get; set; }
        public string? DepartmentDescription { get; set; }


		//User Info
		public int UserId { get; set; }
        public string? UserName { get; set; }

		//Role of user Info
		public int RoleId { get; set; }
        public string? RoleName { get; set; }
	}
}
