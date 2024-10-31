namespace WebProject.Server.Models
{
    public class UserRegisterDto
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class UserLoginDto
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class UserRequestDto
    {
        public string Email { get; set; }
        public string Username { get; set; }
    }

    public class TokenResponseDto
    {
        public string AccessToken { get; set; }
        public DateTime Expiration { get; set; }
    }
}
