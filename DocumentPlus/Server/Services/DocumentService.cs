using DocumentPlus.Data.AppContextDB;
using DocumentPlus.Data.Models;
using DocumentPlus.Shared.Dto.Docs;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;

namespace DocumentPlus.Server.Services;


public class DocumentService
{
    private readonly AppContextDB DbContext;
    public DocumentService(AppContextDB DbContext)
    {
        this.DbContext = DbContext;
    }
    public async Task<int> Create(DocInfo documentInfo, HttpContext httpContext)
    {
        if (documentInfo is null) return -1;
       
        // Получаем Id пользователя
        string? token = await httpContext.GetTokenAsync("access_token");
        JwtSecurityToken cler = new JwtSecurityTokenHandler().ReadJwtToken(token);
        var claims = cler.Claims;
        var UserId = claims.FirstOrDefault(c => c.Type == "UserId").Value;

        if (UserId == null) return -2;

        int userId = Convert.ToInt32(UserId);
        User user = await DbContext.Users.FirstOrDefaultAsync(u => u.Id == userId)
            ?? throw new Exception();

        Document document = new Document()
        {
            Name = documentInfo.Name,
            Path = documentInfo.Path,
            Created = documentInfo.CreatedDate,
            ExpireDate = documentInfo.ExpireDate,
            Desc = documentInfo.Description,
            User = user,
        };

        DocumentAccess access = new DocumentAccess()
        {
            Access_level = 0,
            Document = document,
            User = user,
        };

        await DbContext.AddAsync(access);
        await DbContext.AddAsync(document);
        await DbContext.SaveChangesAsync();
        return 0;
    }

    // Данил, тут пока возвращаются все доки, доделай чтобы было по ID
    public async Task<List<DocInfoGet>> Get()
    {
        List<DocInfoGet> docs = await DbContext
        .Documents
        .AsNoTracking()
        .Select(d => new DocInfoGet()
        {
            Id = d.Id,
            Name = d.Name,
            Description = d.Desc,
            CreatedDate = d.Created,
            ExpireDate = d.ExpireDate,
            Path = d.Path,
            UserId = d.User.Id
        })
        .ToListAsync();

        return docs;
    }

    public async Task<List<DocInfoGet>> GetByUser(HttpContext httpContext)
    {
        // Получаем Id пользователя
        string? token = await httpContext.GetTokenAsync("access_token");
        JwtSecurityToken cler = new JwtSecurityTokenHandler().ReadJwtToken(token);
        var claims = cler.Claims;
        var UserId = claims.FirstOrDefault(c => c.Type == "UserId").Value;

        if (UserId == null) return null;
        int userId = Convert.ToInt32(UserId);

        return await DbContext.DocumentAccesses
            .AsNoTracking()
            .Where(da => da.User.Id == userId)
            .Include(da => da.Document) // Забираем связанные документы
            .Select(da => new DocInfoGet()
            {
                Id = da.Document.Id,
                Name = da.Document.Name,
                Description = da.Document.Desc,
                CreatedDate = da.Document.Created,
                ExpireDate = da.Document.ExpireDate,
                Path = da.Document.Path,
                UserId = da.Document.User.Id
            })
            .ToListAsync();
    }

    public async Task<DocInfoGetId?> GetById(int id)
    {
        return await DbContext.Documents
            .Include(d => d.User) // Забираем связанные документы
            .Select(d => new DocInfoGetId()
            {
                Id = d.Id,
                Name = d.Name,
                Description = d.Desc,
                CreatedDate = d.Created,
                ExpireDate = d.ExpireDate,
                Path = d.Path,
                AuthorName = d.User.Name,
                AuthorSurname = d.User.Surname,
            }).FirstOrDefaultAsync(d => d.Id == id);

        //Document? document = await DbContext
        //    .Documents
        //    .Include(d => d.User)
        //    .SingleOrDefaultAsync(d => d.Id == id);

        //if (document == null) return null;

        //User? user = await DbContext.Users.FindAsync(document.User.Id);

        //if (user == null) return null;
        
        //return new DocInfoGetId()
        //{
        //    Id = document.Id,
        //    Name = document.Name,
        //    Description = document.Desc,
        //    CreatedDate = document.Created,
        //    ExpireDate = document.ExpireDate,
        //    Path = document.Path,
        //    AuthorName = user.Name,
        //    AuthorSurname = user.Surname,
        //};
    }

    //Получение одного документа с правами автора
    public async Task<DocInfoGetId?> GetByIdAndUser(int id, HttpContext httpContext)
    {
        // Получаем Id пользователя
        string? token = await httpContext.GetTokenAsync("access_token");
        JwtSecurityToken cler = new JwtSecurityTokenHandler().ReadJwtToken(token);
        var claims = cler.Claims;
        string UserId = claims.FirstOrDefault(c => c.Type == "UserId").Value;

        if (UserId == null) return null;

        int userId = Convert.ToInt32(UserId);

        return await DbContext.DocumentAccesses
            .Where(da => da.User.Id == userId)
            .Include(da => da.Document) // Забираем связанные документы
            .Select(da => new DocInfoGetId()
            {
                Id = da.Document.Id,
                Name = da.Document.Name,
                Description = da.Document.Desc,
                CreatedDate = da.Document.Created,
                ExpireDate = da.Document.ExpireDate,
                Path = da.Document.Path,
                AuthorName = da.User.Name,
                AuthorSurname = da.User.Surname,
            }).FirstOrDefaultAsync(d => d.Id == id);
    }

    public async Task Delete(int id)
    {
        Document? document = await DbContext.Documents.FindAsync(id);

        if (document == null) return;

        DbContext.Documents.Remove(document);
        await DbContext.SaveChangesAsync();
    }

    public async Task UpdateDocumentAsync(DocInfoGetId updatedDocument)
    {
        Document? document = await DbContext.Documents.FindAsync(updatedDocument.Id);

        if (document == null) return;

        document.Name = updatedDocument.Name;
        document.Desc = updatedDocument.Description;
        document.ExpireDate = updatedDocument.ExpireDate;
        document.Path = updatedDocument.Path;

        await DbContext.SaveChangesAsync();
    }

    public List<Folder> ParseFolders(List<DocInfoGet> documents)
    {
        var folderDictionary = new Dictionary<string, Folder>();

        // Заполняем словарь папками и документами
        foreach (var document in documents)
        {
            var parts = document.Path.Split('\\');
            var currentFolder = string.Empty;

            for (int i = 0; i < parts.Length; i++)
            {
                currentFolder = string.Join("\\", parts.Take(i + 1));

                if (!folderDictionary.ContainsKey(currentFolder))
                {
                    folderDictionary[currentFolder] = new Folder { Name = parts[i], SubFolders = new List<Folder>(), Documents = new List<DocInfoGet>() };
                }

                if (i == parts.Length - 1) // последний элемент - это документ
                {

                    folderDictionary[currentFolder].Documents.Add(document);

                    if (i > 0 && !folderDictionary[string.Join("\\", parts.Take(i))].SubFolders.Contains(folderDictionary[currentFolder])) // добавляем в родительскую папку
                    {
                        var parentFolder = string.Join("\\", parts.Take(i));
                        folderDictionary[parentFolder].SubFolders.Add(folderDictionary[currentFolder]);
                    }
                }

                else // остальные элементы - это папки
                {
                    if (i > 0 && !folderDictionary[string.Join("\\", parts.Take(i))].SubFolders.Contains(folderDictionary[currentFolder])) 
                    {
                        var parentFolder = string.Join("\\", parts.Take(i));
                        folderDictionary[parentFolder].SubFolders.Add(folderDictionary[currentFolder]);
                    }
                }
            }
        }

        List<Folder> folderTree = folderDictionary.Values.ToList().Take(1).ToList();

        return folderTree;
    }
}
