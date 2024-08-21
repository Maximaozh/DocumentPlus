using DocumentPlus.Data.AppContextDB;
using DocumentPlus.Data.Models;
using DocumentPlus.Server.Data.Cryptographic;
using DocumentPlus.Server.Data.Jwt;
using DocumentPlus.Shared.Dto.Groups;
using Microsoft.EntityFrameworkCore;

namespace DocumentPlus.Server.Services
{
    public class GroupService
    {
        private readonly AppContextDB dbContext;
        private readonly PasswordHasher passwordHasher;
        private readonly JwtProvider jwtProvider;

        public GroupService(
        AppContextDB dbContext,
        PasswordHasher passwordHasher,
        JwtProvider jwtProvider)
        {
            this.dbContext = dbContext;
            this.passwordHasher = passwordHasher;
            this.jwtProvider = jwtProvider;
        }


        public async Task<int> Create(GroupInfo groupInfo)
        {
            Group? groupFetched = await dbContext.Groups
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Name == groupInfo.Name);

            if (groupFetched != null)
            {
                return -1;
            }

            if (String.IsNullOrEmpty(groupInfo.Name))
            {
                return -2;
            }

            Group group = new Group()
            {
                Name = groupInfo.Name
            };

            await dbContext.AddAsync(group);
            await dbContext.SaveChangesAsync();

            return 0;
        }

        public async Task<int> Delete(GroupInfo groupInfo)
        {
            await dbContext
                .Groups
                .Where(u => u.Name == groupInfo.Name)
                .ExecuteDeleteAsync();

            await dbContext.SaveChangesAsync();

            return 0;
        }

        public async Task<int> Kick(GroupWithUsers groupInfo)
        {
            await dbContext
                .Groups
                .Where(u => u.Name == groupInfo.Name)
                .ExecuteDeleteAsync();

            await dbContext.SaveChangesAsync();

            return 0;
        }

        //public async Task<int> Kick(GroupWithUsers groupInfo)
        //{
        //    foreach (UserInfo user in groupInfo.Users)
        //    {
        //        await dbContext.UserGroups.remo
        //    }


        //    await dbContext
        //        .Groups
        //        .Where(u => u.Name == groupInfo.Name)
        //        .ExecuteDeleteAsync();

        //    await dbContext.SaveChangesAsync();

        //    return 0;
        //}
    }
}
