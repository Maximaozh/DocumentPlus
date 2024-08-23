using DocumentPlus.Data.AppContextDB;
using DocumentPlus.Data.Models;
using DocumentPlus.Shared.Dto.Access;
using Microsoft.EntityFrameworkCore;

namespace DocumentPlus.Server.Services
{
    public class AccessService
    {
        private readonly AppContextDB dbContext;

        public AccessService(
        AppContextDB dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<List<DocumentsNamed>> GetDocks(DocumentsBySearch filter)
        {
            return String.IsNullOrEmpty(filter.Search)
                ? await dbContext
                .Documents
                .Select(g => new DocumentsNamed()
                {
                    Id = g.Id,
                    Name = g.Name
                })
                .ToListAsync()
                : await dbContext
                 .Documents
                 .Where(d => d.Name.Contains(filter.Search) || d.Desc.Contains(filter.Search))
                 .Select(g => new DocumentsNamed()
                 {
                     Id = g.Id,
                     Name = g.Name
                 })
                 .ToListAsync();
        }

        public async Task<List<GroupAccess>> GetNoRights(DocumentsNamed filter)
        {
            return await dbContext
                .Groups
                .Select(g => new GroupAccess()
                {
                    Id = g.Id,
                    Name = g.Name,
                    Identifier = "-1"
                })
                .ToListAsync();
            /*
                .Where(g => !dbContext
                        .DocumentAccesses
                        .Any(da => da.Group.Id != g.Id))*/
        }

        public async Task<List<GroupAccess>> GetEdit(DocumentsNamed filter)
        {
            return await dbContext
                .DocumentAccesses
                .Where(da => da.Document.Id == Convert.ToInt32(filter.Id) && da.Access_level == 1)
                .Select(da => new GroupAccess()
                {
                    Id = da.Group.Id,
                    Name = da.Group.Name,
                    Identifier = da.Access_level.ToString()
                })
                .ToListAsync();
        }

        public async Task<List<GroupAccess>> GetRead(DocumentsNamed filter)
        {
            return await dbContext
                 .DocumentAccesses
                 .Where(da => da.Document.Id == Convert.ToInt32(filter.Id) && da.Access_level == 2)
                 .Select(da => new GroupAccess()
                 {
                     Id = da.Group.Id,
                     Name = da.Group.Name,
                     Identifier = da.Access_level.ToString()
                 })
                 .ToListAsync();
        }


        public async Task<int> SetRights(SetRights rights)
        {
            Document? doc = dbContext
                .Documents
                .Where(d =>
                d.Id == rights.Document.Id).FirstOrDefault();

            Group? group = dbContext
                .Groups
                .Where(g => g.Id == rights.GroupAccess.Id)
                .FirstOrDefault();

            if (rights.GroupAccess.Identifier == "-1")
            {
                await dbContext
                    .DocumentAccesses
                    .Where(da => da.Group.Id == group.Id && da.Document.Id == doc.Id)
                    .ExecuteDeleteAsync();
            }
            else if (rights.GroupAccess.Identifier == "1" || rights.GroupAccess.Identifier == "2")
            {
                DocumentAccess? da = dbContext
                    .DocumentAccesses
                    .Where(da => da.Group.Id == group.Id && da.Document.Id == doc.Id)
                    .FirstOrDefault();

                if (da is null)
                {
                    da = new DocumentAccess()
                    {
                        Group = group,
                        Document = doc,
                        Access_level = Convert.ToInt32(rights.GroupAccess.Identifier)
                    };
                    dbContext.Add(da);
                }
                else
                {
                    da.Access_level = Convert.ToInt32(rights.GroupAccess.Identifier);
                }
            }

            dbContext.SaveChangesAsync();
            return 0;
        }


    }
}
