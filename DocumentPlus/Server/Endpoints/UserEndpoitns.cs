using DocumentPlus.Server.Services;
using DocumentPlus.Shared.Dto;
using DocumentPlus.Shared.Dto.Users;
using DocumentPlus.Shared.Dto.Users.Controls;
using Microsoft.AspNetCore.Mvc;

namespace DocumentPlus.Server.Endpoints
{
    public static class UserEndpoitns
    {
        public static IEndpointRouteBuilder MapUserEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapPost("api/user/login", Login);
            app.MapPost("api/user/registrate", Registrate).RequireAuthorization(policy => policy.RequireRole("Администратор"));
            app.MapPost("api/user/edit", Edit).RequireAuthorization(policy => policy.RequireRole("Администратор"));
            app.MapPost("api/user/delete", Delete).RequireAuthorization(policy => policy.RequireRole("Администратор"));
            app.MapPost("api/user/getUsers", GetUsers).RequireAuthorization(policy => policy.RequireRole("Администратор"));
            app.MapPost("api/user/getGroupsByUsers", GetGroupsByUsers).RequireAuthorization(policy => policy.RequireRole("Администратор"));
            app.MapGet("api/user/getCount", GetCount).RequireAuthorization(policy => policy.RequireRole("Администратор"));

            return app;
        }

        private static async Task<IResult> Login(HttpContext httpContext, UserService userService, [FromBody] UserLogin user)
        {
            LoginResponse data = await userService.AuthenticateUser(user);
            return data is null ? Results.BadRequest() : Results.Json(data);
        }

        private static async Task<IResult> Registrate(UserService userService, [FromBody] UserRegistrate user)
        {
            int status = await userService.Registrate(user);
            return status == 0 ? Results.Ok() : Results.BadRequest();
        }

        private static async Task<IResult> Edit(UserService userService, [FromBody] UserEdit user)
        {
            int status = await userService.Edit(user);
            return status == 0 ? Results.Ok() : Results.BadRequest();
        }
        private static async Task<IResult> Delete(UserService userService, [FromBody] UserInfo user)
        {
            int status = await userService.Delete(user);
            return status == 0 ? Results.Ok() : Results.BadRequest();
        }


        private static async Task<IResult> GetCount(UserService userService)
        {
            int count = await userService.GetCount();
            return Results.Ok(count);
        }

        private static async Task<IResult> GetUsers(UserService userService, [FromBody] UsersTableRequest filter)
        {
            UsersTableResponse response = await userService.GetUsers(filter);
            return Results.Json(response);
        }
        private static async Task<IResult> GetGroupsByUsers(UserService userService, [FromBody] GroupsByUsersRequest filter)
        {
            GroupsByUsersResponse response = await userService.GetGroupsByUsers(filter);
            return Results.Json(response);
        }

    }
}
