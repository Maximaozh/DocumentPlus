using DocumentPlus.Server.Services;
using DocumentPlus.Shared.Dto.Access;
using Microsoft.AspNetCore.Mvc;

namespace DocumentPlus.Server.Endpoints
{
    public static class AccessEndpoints
    {
        public static IEndpointRouteBuilder MapAccessEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapPost("api/access/getDocs", GetDocks).RequireAuthorization(policy => policy.RequireRole("Администратор"));
            app.MapPost("api/access/getNoRightsGroups", GetNoRights).RequireAuthorization(policy => policy.RequireRole("Администратор"));
            app.MapPost("api/access/getReadGroups", GetRead).RequireAuthorization(policy => policy.RequireRole("Администратор"));
            app.MapPost("api/access/getEditGroups", GetEdit).RequireAuthorization(policy => policy.RequireRole("Администратор"));
            app.MapPost("api/access/changeRights", ChangeRights).RequireAuthorization(policy => policy.RequireRole("Администратор"));
            return app;
        }

        private static async Task<IResult> GetDocks(AccessService accessService, [FromBody] DocumentsBySearch search)
        {
            List<DocumentsNamed> data = await accessService.GetDocks(search);
            return data is null ? Results.BadRequest() : Results.Json(data);
        }

        private static async Task<IResult> GetNoRights(AccessService accessService, [FromBody] DocumentsNamed doc)
        {
            List<GroupAccess>? data = await accessService.GetNoRights(doc);
            return data is null ? Results.BadRequest() : Results.Json(data);
        }
        private static async Task<IResult> GetRead(AccessService accessService, [FromBody] DocumentsNamed doc)
        {
            List<GroupAccess>? data = await accessService.GetRead(doc);
            return data is null ? Results.BadRequest() : Results.Json(data);
        }
        private static async Task<IResult> GetEdit(AccessService accessService, [FromBody] DocumentsNamed doc)
        {
            List<GroupAccess>? data = await accessService.GetEdit(doc);
            return data is null ? Results.BadRequest() : Results.Json(data);
        }

        private static async Task<IResult> ChangeRights(AccessService accessService, [FromBody] SetRights doc)
        {
            int? data = await accessService.SetRights(doc);
            return data is null ? Results.BadRequest() : Results.Ok(data);
        }

    }
}
