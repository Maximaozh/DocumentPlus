using Microsoft.AspNetCore.Components.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;


namespace DocumentPlus.Client.Data
{
    // Работает с состоянием пользователя, по больешй части автоматика
    public class CustomAuthenticationStateProivder : AuthenticationStateProvider
    {
        private readonly CustomLocalStorage localStorage;

        public CustomAuthenticationStateProivder(CustomLocalStorage localStorage)
        {
            this.localStorage = localStorage;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            string jwtToken = await localStorage.GetValueAsync<string>("jwt-access-token");
            if (string.IsNullOrEmpty(jwtToken))
            {
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }
            AuthenticationState state = new AuthenticationState(new ClaimsPrincipal(
                new ClaimsIdentity(ParseClaimsFromJwt(jwtToken), "JwtAuth")));

            // Почему.
            NotifyAuthenticationStateChanged(Task.FromResult(state));

            return state;

        }
        private static IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
        {
            JwtSecurityToken token = new JwtSecurityTokenHandler().ReadJwtToken(jwt);
            List<Claim> claims = token.Claims.ToList();
            return claims;
        }

        // Мы объявляем о изменение состояние, где потом повторно объявляем о обновлении состояния.
        // Выглядит как неэффективный код, но он работает и будет время Я его исправлю
        // А может и не исправляю.
        // Почему.
        public void NotifyStateChange()
        {
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }
    }
}
