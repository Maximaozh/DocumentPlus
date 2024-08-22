using DocumentPlus.Server.Services;
using DocumentPlus.Shared.Dto.Groups;
using Microsoft.AspNetCore.Mvc;

namespace DocumentPlus.Server.Endpoints
{
    public static class GroupEndpointsClass
    {
        public static IEndpointRouteBuilder MapGroupEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapGet("api/group/get", GetGroups).RequireAuthorization(policy => policy.RequireRole("Администратор"));
            app.MapPost("api/group/addUsers", AddUsers).RequireAuthorization(policy => policy.RequireRole("Администратор"));
            app.MapPost("api/group/deleteUsers", DeleteUsers).RequireAuthorization(policy => policy.RequireRole("Администратор"));
            app.MapPost("api/group/create", Create).RequireAuthorization(policy => policy.RequireRole("Администратор"));
            app.MapPost("api/group/edit", Edit).RequireAuthorization(policy => policy.RequireRole("Администратор"));
            app.MapPost("api/group/delete", Delete).RequireAuthorization(policy => policy.RequireRole("Администратор"));
            return app;
        }

        private static async Task<IResult> GetGroups(GroupService GroupService)
        {
            List<GroupInfo> data = await GroupService.GetGroups();
            return data is null ? Results.BadRequest() : Results.Json(data);
        }

        private static async Task<IResult> AddUsers(GroupService GroupService, [FromBody] GroupWithUsers group)
        {
            int data = await GroupService.Add(group);
            return data == 0 ? Results.Ok(data) : Results.BadRequest(data);
        }
        private static async Task<IResult> DeleteUsers(GroupService GroupService, [FromBody] DeleteUserGroupsRequest group)
        {
            int data = await GroupService.Kick(group);
            return data == 0 ? Results.Ok(data) : Results.BadRequest(data);
        }
        private static async Task<IResult> Create(GroupService GroupService, [FromBody] GroupInfo group)
        {
            int data = await GroupService.Create(group);
            return data == 0 ? Results.Ok(data) : Results.BadRequest(data);
        }
        private static async Task<IResult> Edit(GroupService GroupService, [FromBody] GroupEdit group)
        {
            int data = await GroupService.Edit(group);
            return data == 0 ? Results.Ok(data) : Results.BadRequest(data);
        }
        private static async Task<IResult> Delete(GroupService GroupService, [FromBody] GroupInfo group)
        {
            int data = await GroupService.Delete(group);
            return data == 0 ? Results.Ok(data) : Results.BadRequest(data);
        }
    }
}
