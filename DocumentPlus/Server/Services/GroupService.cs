using DocumentPlus.Data.AppContextDB;
using DocumentPlus.Data.Models;
using DocumentPlus.Shared.Dto.Groups;
using DocumentPlus.Shared.Dto.Users;
using Microsoft.EntityFrameworkCore;

namespace DocumentPlus.Server.Services
{
    public class GroupService
    {
        private readonly AppContextDB dbContext;

        public GroupService(
        AppContextDB dbContext)
        {
            this.dbContext = dbContext;
        }
        public async Task<List<GroupInfo>> GetGroups()
        {
            return await dbContext
                .Groups
                .Select(g => new GroupInfo()
                {
                    Id = g.Id,
                    Name = g.Name
                })
                .ToListAsync();
        }

        // Тут больше работа по пользователям
        public async Task<int> Kick(DeleteUserGroupsRequest data)
        {
            foreach (GroupInfo groupInfo in data.Groups)
            {
                Group group = dbContext.Groups
                    .Where(g => g.Id == groupInfo.Id).FirstOrDefault();

                foreach (UserInfo userInfo in data.Users)
                {
                    User? user = dbContext
                        .Users
                        .Where(u => u.Id == Convert.ToInt32(userInfo.Id)).FirstOrDefault();

                    await dbContext
                        .UserGroups
                        .Where(ug => ug.User == user && ug.Group == group)
                        .ExecuteDeleteAsync();
                }
            }

            return 0;
        }

        public async Task<int> Add(GroupWithUsers groupInfo)
        {
            Group? group = dbContext.Groups
                .Where(g => g.Id == groupInfo.Id)
                .FirstOrDefault();

            if (group is null)
            {
                return -1;
            }

            foreach (UserInfo userInfo in groupInfo.Users)
            {
                User? user = dbContext.Users
                    .Where(u => u.Id == Convert.ToInt32(userInfo.Id))
                    .FirstOrDefault();

                if (user is null)
                {
                    continue;
                }

                UserGroup? userInDb = dbContext.UserGroups
                     .Where(ug => ug.User.Id == user.Id && ug.Group.Id == group.Id)
                     .FirstOrDefault();

                if (userInDb is not null)
                {
                    continue;
                }

                UserGroup ug = new UserGroup()
                {
                    User = user,
                    Group = group
                };
                await dbContext.UserGroups.AddAsync(ug);

            }

            await dbContext.SaveChangesAsync();
            return 0;
        }


        // Стандартное создай, измени, удали

        public async Task<int> Create(GroupInfo groupInfo)
        {
            if (String.IsNullOrEmpty(groupInfo.Name))
            {
                return -1;
            }

            Group? groupFetched = await dbContext.Groups
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Name == groupInfo.Name);

            if (groupFetched != null)
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
        public async Task<int> Edit(GroupEdit groupEdit)
        {
            if (groupEdit is null)
            {
                return -1;
            }

            if (groupEdit.OldName != groupEdit.Name)
            {
                Group? groupNewLogin = await dbContext.Groups
                .FirstOrDefaultAsync(u => u.Name == groupEdit.Name);

                if (groupNewLogin is not null)
                {
                    return -2;
                }
            }

            Group? groupChange = await dbContext.Groups
                   .FirstOrDefaultAsync(u => u.Name == groupEdit.OldName);

            if (groupChange is null)
            {
                return -3;
            }

            groupChange.Name = groupEdit.Name;

            await dbContext.SaveChangesAsync();
            return 0;
        }

        public async Task<int> Delete(GroupInfo groupInfo)
        {
            if (String.IsNullOrEmpty(groupInfo.Name))
            {
                return -1;
            }

            await dbContext
                .Groups
                .Where(u => u.Name == groupInfo.Name)
                .ExecuteDeleteAsync();

            await dbContext.SaveChangesAsync();

            return 0;
        }


    }
}
