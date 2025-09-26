namespace App.Web.Api.DTOs
{
    public class LoginDto
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public bool IsActive { get; set; } = false;
    }
}
