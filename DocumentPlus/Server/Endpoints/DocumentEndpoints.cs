using DocumentPlus.Server.Services;
using DocumentPlus.Shared.Dto.Docs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DocumentPlus.Server.Endpoints
{
    public static class DocumentEndpoints
    {
        public static IEndpointRouteBuilder MapDocEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapPost("api/Document", Creation).RequireAuthorization();
            app.MapGet("api/documents/user", GettingByUser).RequireAuthorization();
            app.MapGet("api/documents/group", GettingByGroup).RequireAuthorization();
            app.MapGet("api/documents/tree/group", GettingTreeByUser).RequireAuthorization();
            app.MapGet("api/DocWork", Getting).RequireAuthorization(policy => policy.RequireRole("Администратор"));
            app.MapGet("api/documents/tree", GettingTree).RequireAuthorization(policy => policy.RequireRole("Администратор"));
            app.MapGet("/api/documents/{id}", async (int id, DocumentService documentService) =>
            {
                var document = await documentService.GetById(id);
                if (document == null) return Results.NotFound();
                return Results.Ok(document);
            }).RequireAuthorization(policy => policy.RequireRole("Администратор"));
            app.MapGet("/api/user/documents/{id}", async (int id, DocumentService documentService, HttpContext httpContext) =>
            {
                var document = await documentService.GetByIdAndUser(id, httpContext);
                if (document == null) return Results.NotFound();
                return Results.Ok(document);
            }).RequireAuthorization();
            app.MapGet("/api/group/documents/{id}", async (int id, DocumentService documentService, HttpContext httpContext) =>
            {
                var document = await documentService.GetByIdAndGroup(id, httpContext);
                if (document == null) return Results.NotFound();
                return Results.Ok(document);
            }).RequireAuthorization();
            app.MapDelete("/api/document/{id}", async (int id, DocumentService documentService) =>
            {
                await documentService.Delete(id);
                return Results.NoContent();
            }).RequireAuthorization();
            app.MapPut("/api/documents", async (DocInfoGetId document, DocumentService documentService) =>
            {
                try
                {
                    await documentService.UpdateDocumentAsync(document);
                    return Results.NoContent();
                }
                catch (ArgumentException)
                {
                    return Results.NotFound();
                }
            }).RequireAuthorization();

            return app;
        }

        private static async Task<IResult> Creation(DocumentService documentService, DocInfo document, HttpContext httpContext)
        {
            int response = await documentService.Create(document, httpContext);
            return Results.Ok(response);
        }

        private static async Task<IResult> Getting(string filter, DocumentService documentService)
        {
            List<DocInfoGet> response = await documentService.Get(filter);
            return Results.Json(response);
        }

        private static async Task<IResult> GettingTree(string filter, DocumentService documentService)
        {
            List<DocInfoGet> documents = await documentService.Get(filter);
            var tree = documentService.ParseFolders(documents);
            return Results.Json(tree);
        }

        private static async Task<IResult> GettingByUser(DocumentService documentService, HttpContext httpContext)
        {
            List<DocInfoGet> response = await documentService.GetByUser(httpContext);
            return Results.Json(response);
        }
        private static async Task<IResult> GettingTreeByUser(DocumentService documentService, HttpContext httpContext)
        {
            List<DocInfoGet> documents = await documentService.GetByUser(httpContext);
            var tree = documentService.ParseFolders(documents);
            return Results.Json(tree);
        }

        private static async Task<IResult> GettingByGroup(string filter, DocumentService documentService, HttpContext httpContext)
        {
            List<DocInfoGet> response = await documentService.GetByGroupAndSearch(httpContext, filter);
            return Results.Json(response);
        }
    }
}