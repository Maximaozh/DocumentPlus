namespace DocumentPlus.Shared.Dto.Users
{
	// Отправляемая инфомрация на сервер для проверки
	public class UserLogin
	{
		public required string Login { get; set; }
		public required string Password { get; set; }
	}
}
