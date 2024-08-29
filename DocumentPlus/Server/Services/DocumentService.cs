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

    public async Task<int> GetUserId(HttpContext httpContext)
    {
        string? token = await httpContext.GetTokenAsync("access_token");
        JwtSecurityToken cler = new JwtSecurityTokenHandler().ReadJwtToken(token);
        IEnumerable<System.Security.Claims.Claim> claims = cler.Claims;
        string UserId = claims.FirstOrDefault(c => c.Type == "UserId").Value;

        return Convert.ToInt32(UserId);
    }

    public async Task<int> Create(DocInfo documentInfo, HttpContext httpContext)
    {
        if (documentInfo is null)
        {
            return -1;
        }

        int userId = await GetUserId(httpContext);
        User user = await DbContext.Users.FirstOrDefaultAsync(u => u.Id == userId)
            ?? throw new Exception();

        Document document = new()
        {
            Name = documentInfo.Name,
            Path = documentInfo.Path,
            Created = documentInfo.CreatedDate,
            ExpireDate = documentInfo.ExpireDate,
            Desc = documentInfo.Description,
            User = user,
        };

        DocumentAccess access = new()
        {
            Access_level = 0,
            Document = document,
            User = user,
        };

        await DbContext.AddAsync(document);
        await DbContext.AddAsync(access);
        await DbContext.SaveChangesAsync();
        return 0;
    }

    public async Task<List<DocInfoGet>> Get(string search)
    {
        if (string.IsNullOrWhiteSpace(search))
        {
            return await DbContext.Documents
            .AsNoTracking()
            .OrderBy(d => d.Id)
            .Select(d => new DocInfoGet()
            {
                Id = d.Id,
                Name = d.Name,
                Description = d.Desc,
                CreatedDate = d.Created,
                ExpireDate = d.ExpireDate,
                Path = d.Path,
                UserId = d.User.Id,
                AuthorName = d.User.Name,
                AuthorSurname = d.User.Surname,
            }).ToListAsync();
        }
        string filter = search.ToLower();

        return await DbContext.Documents
            .AsNoTracking()
            .Where(d => d.Name.ToLower().Contains(filter) ||
                                     d.User.Name.ToLower().Contains(filter) ||
                                     d.User.Surname.ToLower().Contains(filter))
            .Select(d => new DocInfoGet()
            {
                Id = d.Id,
                Name = d.Name,
                Description = d.Desc,
                CreatedDate = d.Created,
                ExpireDate = d.ExpireDate,
                Path = d.Path,
                UserId = d.User.Id,
                AuthorName = d.User.Name,
                AuthorSurname = d.User.Surname,
            }).ToListAsync();
    }

    public async Task<List<DocInfoGet>> GetByUser(HttpContext httpContext)
    {
        int userId = await GetUserId(httpContext);

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
                UserId = da.Document.User.Id,
            })
            .ToListAsync();
    }

    public async Task<List<DocInfoGet>> GetByGroupAndSearch(HttpContext httpContext, string search)
    {
        if (string.IsNullOrWhiteSpace(search))
        {
            return await GetByGroup(httpContext);
        }
        string filter = search.ToLower();

        int userId = await GetUserId(httpContext);

        List<int> groups = await DbContext.UserGroups
        .AsNoTracking()
        .Where(g => g.User.Id == userId)
        .Select(g => g.Group.Id)
        .ToListAsync();

        // Получаем документы, доступные автору через права автора
        var authorDocuments = await DbContext.DocumentAccesses
        .Where(r => r.User.Id == userId)
        .Where(r => r.Document.Name.ToLower().Contains(filter) ||
                                     r.Document.User.Name.ToLower().Contains(filter) ||
                                     r.Document.User.Surname.ToLower().Contains(filter))
        .Include(r => r.Document)
        .Include(r => r.Document.User)
        .Select(r => new { r.Document, r.Document.User, r.Access_level, r.Group })
        .ToListAsync();

        // Получаем документы, доступные через группы (если автор состоит в каких-либо группах)
        var groupDocuments = await DbContext.DocumentAccesses
            .Where(r => groups.Contains(r.Group.Id))
            .Where(r => r.Document.Name.ToLower().Contains(filter) ||
                                     r.Document.User.Name.ToLower().Contains(filter) ||
                                     r.Document.User.Surname.ToLower().Contains(filter) ||
                                     r.Group.Name.ToLower().Contains(filter))
            .Include(r => r.Document)
            .Select(r => new { r.Document, r.Document.User, r.Access_level, r.Group })
            .ToListAsync();

        // Объединяем все документы
        var allDocuments = authorDocuments.Concat(groupDocuments);

        // Создаем словарь для хранения максимального уровня доступа для каждого документа
        Dictionary<int, (DocInfoGet docs, int MinAccessLevel)> documentAccessLevels = new();

        foreach (var entry in allDocuments)
        {
            // Проверяем, содержится ли документ в словаре
            if (documentAccessLevels.TryGetValue(entry.Document.Id, out (DocInfoGet docs, int MinAccessLevel) existingEntry))
            {
                // Если документ уже существует, обновляем, если новый уровень доступа выше
                if (entry.Access_level < existingEntry.MinAccessLevel)
                {
                    documentAccessLevels[entry.Document.Id] = (new DocInfoGet
                    {
                        Id = entry.Document.Id,
                        Name = entry.Document.Name,
                        Description = entry.Document.Desc,
                        CreatedDate = entry.Document.Created,
                        ExpireDate = entry.Document.ExpireDate,
                        Path = entry.Document.Path,
                        AuthorName = entry.Document.User.Name,
                        AuthorSurname = entry.Document.User.Surname,
                        AccessLevel = entry.Access_level,
                        GroupName = entry.Group?.Name,
                    }, entry.Access_level);
                }
            }
            else
            {
                // Если документ не существует, добавляем его
                documentAccessLevels[entry.Document.Id] = (new DocInfoGet
                {
                    Id = entry.Document.Id,
                    Name = entry.Document.Name,
                    Description = entry.Document.Desc,
                    CreatedDate = entry.Document.Created,
                    ExpireDate = entry.Document.ExpireDate,
                    Path = entry.Document.Path,
                    AuthorName = entry.User.Name,
                    AuthorSurname = entry.Document.User.Surname,
                    AccessLevel = entry.Access_level,
                    GroupName = entry.Group?.Name,
                }, entry.Access_level);
            }
        }

        // Получаем уникальные документы с наивысшим уровнем доступа
        List<DocInfoGet> availableDocuments = documentAccessLevels.Values.Select(x => x.docs).ToList();

        return availableDocuments;
    }

    public async Task<List<DocInfoGet>> GetByGroup(HttpContext httpContext)
    {
        int userId = await GetUserId(httpContext);

        List<int> groups = await DbContext.UserGroups
        .AsNoTracking()
        .Where(g => g.User.Id == userId)
        .Select(g => g.Group.Id)
        .ToListAsync();

        // Получаем документы, доступные автору через права автора
        var authorDocuments = await DbContext.DocumentAccesses
        .Where(r => r.User.Id == userId)
        .Include(r => r.Document)
        .Include(r => r.Document.User)
        .Select(r => new { r.Document, r.Document.User, r.Access_level, r.Group })
        .ToListAsync();

        // Получаем документы, доступные через группы (если автор состоит в каких-либо группах)
        var groupDocuments = await DbContext.DocumentAccesses
            .Where(r => groups.Contains(r.Group.Id))
            .Include(r => r.Document)
            .Select(r => new { r.Document, r.Document.User, r.Access_level, r.Group })
            .ToListAsync();

        // Объединяем все документы
        var allDocuments = authorDocuments.Concat(groupDocuments);

        // Создаем словарь для хранения максимального уровня доступа для каждого документа
        Dictionary<int, (DocInfoGet docs, int MinAccessLevel)> documentAccessLevels = new();

        foreach (var entry in allDocuments)
        {
            // Проверяем, содержится ли документ в словаре
            if (documentAccessLevels.TryGetValue(entry.Document.Id, out (DocInfoGet docs, int MinAccessLevel) existingEntry))
            {
                // Если документ уже существует, обновляем, если новый уровень доступа выше
                if (entry.Access_level < existingEntry.MinAccessLevel)
                {
                    documentAccessLevels[entry.Document.Id] = (new DocInfoGet
                    {
                        Id = entry.Document.Id,
                        Name = entry.Document.Name,
                        Description = entry.Document.Desc,
                        CreatedDate = entry.Document.Created,
                        ExpireDate = entry.Document.ExpireDate,
                        Path = entry.Document.Path,
                        AuthorName = entry.Document.User.Name,
                        AuthorSurname = entry.Document.User.Surname,
                        AccessLevel = entry.Access_level,
                        GroupName = entry.Group?.Name,
                    }, entry.Access_level);
                }
            }
            else
            {
                // Если документ не существует, добавляем его
                documentAccessLevels[entry.Document.Id] = (new DocInfoGet
                {
                    Id = entry.Document.Id,
                    Name = entry.Document.Name,
                    Description = entry.Document.Desc,
                    CreatedDate = entry.Document.Created,
                    ExpireDate = entry.Document.ExpireDate,
                    Path = entry.Document.Path,
                    AuthorName = entry.User.Name,
                    AuthorSurname = entry.Document.User.Surname,
                    AccessLevel = entry.Access_level,
                    GroupName = entry.Group?.Name,
                }, entry.Access_level);
            }
        }

        // Получаем уникальные документы с наивысшим уровнем доступа
        List<DocInfoGet> availableDocuments = documentAccessLevels.Values.Select(x => x.docs).ToList();

        return availableDocuments;
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
                AccessLevel = 0,
            }).FirstOrDefaultAsync(d => d.Id == id);
    }

    //Получение одного документа с правами автора
    public async Task<DocInfoGetId?> GetByIdAndUser(int id, HttpContext httpContext)
    {
        int userId = await GetUserId(httpContext);

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
    public async Task<DocInfoGetId?> GetByIdAndUserGroup(int id, HttpContext httpContext)
    {
        int userId = await GetUserId(httpContext);

        var UserDoc = await DbContext.DocumentAccesses
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
                AccessLevel = da.Access_level,
            }).FirstOrDefaultAsync(d => d.Id == id);

        if (UserDoc != null) 
        { 
            return UserDoc;
        }

        return await GetByIdAndGroup(id, httpContext);
    }

    //Получение одного документа с правами групп
    public async Task<DocInfoGetId?> GetByIdAndGroup(int id, HttpContext httpContext)
    {
        int userId = await GetUserId(httpContext);

        List<int> groups = await DbContext.UserGroups
            .AsNoTracking()
            .Where(g => g.User.Id == userId)
            .Select(g => g.Group.Id)
            .ToListAsync();

        // Получаем документы, доступные через группы (если автор состоит в каких-либо группах)
        var docs = await DbContext.DocumentAccesses
            .AsNoTracking()
            .Include(da => da.Document)
            .Include(da => da.Document.User)
            .Where(da => groups.Contains(da.Group.Id) && da.Document.Id == id)
            .Select(da => new { da.Document, da.Document.User, da.Access_level })
            .ToListAsync();

        if (!docs.Any())
        {
            return null; // Не найден доступ к документу
        }

        // Получаем максимальный уровень доступа
        var maxAccessLevelEntry = docs.OrderBy(x => x.Access_level).First();

        // Возвращаем DTO документа с максимальным уровнем доступа
        return new DocInfoGetId
        {
            Id = maxAccessLevelEntry.Document.Id,
            Name = maxAccessLevelEntry.Document.Name,
            Description = maxAccessLevelEntry.Document.Desc,
            CreatedDate = maxAccessLevelEntry.Document.Created,
            ExpireDate = maxAccessLevelEntry.Document.ExpireDate,
            Path = maxAccessLevelEntry.Document.Path,
            AuthorName = maxAccessLevelEntry.Document.User.Name,
            AuthorSurname = maxAccessLevelEntry.Document.User.Name,
            AccessLevel = maxAccessLevelEntry.Access_level,
        };
    }

    public async Task Delete(int id)
    {
        Document? document = await DbContext.Documents.FindAsync(id);

        if (document == null)
        {
            return;
        }

        DbContext.Documents.Remove(document);
        await DbContext.SaveChangesAsync();
    }

    public async Task UpdateDocumentAsync(DocInfoGetId updatedDocument)
    {
        Document? document = await DbContext.Documents.FindAsync(updatedDocument.Id);

        if (document == null)
        {
            return;
        }

        document.Name = updatedDocument.Name;
        document.Desc = updatedDocument.Description;
        document.ExpireDate = updatedDocument.ExpireDate;
        document.Path = updatedDocument.Path;

        await DbContext.SaveChangesAsync();
    }

    public List<Folder> ParseFolders(List<DocInfoGet> documents)
    {
        Dictionary<string, Folder> folderDictionary = new();

        // Заполняем словарь папками и документами
        foreach (DocInfoGet document in documents)
        {
            string[] parts = document.Path.Split('\\');
            string currentFolder = string.Empty;

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
                        string parentFolder = string.Join("\\", parts.Take(i));
                        folderDictionary[parentFolder].SubFolders.Add(folderDictionary[currentFolder]);
                    }
                }

                else // остальные элементы - это папки
                {
                    if (i > 0 && !folderDictionary[string.Join("\\", parts.Take(i))].SubFolders.Contains(folderDictionary[currentFolder]))
                    {
                        string parentFolder = string.Join("\\", parts.Take(i));
                        folderDictionary[parentFolder].SubFolders.Add(folderDictionary[currentFolder]);
                    }
                }
            }
        }

        List<Folder> folderTree = folderDictionary.Values.ToList().Take(1).ToList();

        return folderTree;
    }
}
