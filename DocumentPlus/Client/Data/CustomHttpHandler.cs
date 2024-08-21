
using Blazored.LocalStorage;

namespace DocumentPlus.Client.Data
{
    // Этот класс необходим для того
    // Чтобы твои запросы на сервак имели в заголовки токен авторизации
    // Если не использовать httpfactory то ты не сможешь зайти
    public class CustomHttpHandler : DelegatingHandler
    {
        private readonly ILocalStorageService localStorage;
        public CustomHttpHandler(ILocalStorageService localStorage)
        {
            this.localStorage = localStorage;
        }

        // Здесь идёт попытка прогрузки токена из хранилища. Если токен не был обнаружен то ничего не будет добавлено.
        // Вызывается каждый раз когда сгенерированный при помощи HttpFactory посылает запросы на сервер.
        // Для получения последующей информации можно использовать токен, где на стороне сервере он расшифровывается
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request.RequestUri.AbsolutePath.ToLower().Contains("login"))
            {
                return await base.SendAsync(request, cancellationToken);
            }

            string? token = await localStorage.GetItemAsync<string>("jwt-access-token");

            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Add("Authorization", $"Bearer {token}");
            }

            return await base.SendAsync(request, cancellationToken);

        }
    }
}
