namespace DocumentPlus.Shared.Dto.Users
{
	// Строка, представляющая собой JWT токен
	public class LoginResponse
	{
		public string? Token { get; set; }
		public UserInfo? User { get; set; }
	}
}
