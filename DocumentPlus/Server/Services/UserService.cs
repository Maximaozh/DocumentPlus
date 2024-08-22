using DocumentPlus.Data.AppContextDB;
using DocumentPlus.Data.Models;
using DocumentPlus.Server.Data.Cryptographic;
using DocumentPlus.Server.Data.Jwt;
using DocumentPlus.Shared.Dto;
using DocumentPlus.Shared.Dto.Groups;
using DocumentPlus.Shared.Dto.Users;
using DocumentPlus.Shared.Dto.Users.Controls;
using Microsoft.EntityFrameworkCore;

namespace DocumentPlus.Server.Services
{
    public class UserService
    {
        private readonly AppContextDB dbContext;
        private readonly PasswordHasher passwordHasher;
        private readonly JwtProvider jwtProvider;

        public UserService(
        AppContextDB dbContext,
        PasswordHasher passwordHasher,
        JwtProvider jwtProvider)
        {
            this.dbContext = dbContext;
            this.passwordHasher = passwordHasher;
            this.jwtProvider = jwtProvider;
        }

        public async Task<LoginResponse> AuthenticateUser(UserLogin userLogin)
        {
            User? user = await dbContext.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Login == userLogin.Login);

            if (user == null)
            {
                return null;
            }

            if (userLogin.Login != user.Login)
            {
                return null;
            }
            string key = passwordHasher.GenerateHashBCrypt(userLogin.Password);
            if (!passwordHasher.Verify(userLogin.Password, user.Password))
            {
                return null;
            }

            UserInfo userInfo = new UserInfo()
            {
                Id = user.Id.ToString(),
                Login = user.Login,
                Role = user.Role,
                Name = user.Name,
                Surname = user.Surname
            };
            LoginResponse response = new LoginResponse() { Token = jwtProvider.GenerateJWT(userInfo), User = userInfo };
            return response;
        }

        public async Task<UsersTableResponse> GetUsers(UsersTableRequest filter)
        {
            string search = String.IsNullOrEmpty(filter.Search) ? "" : filter.Search.ToLower();
            int skipCount = filter.Page * filter.PageSize;

            if (skipCount < 0)
            {
                skipCount = 0;
            }

            int totatlCount = dbContext.Users
                .AsNoTracking()
                .Where(u => u.Login.ToLower().Contains(search) ||
                                         u.Surname.ToLower().Contains(search) ||
                                         u.Name.ToLower().Contains(search))
                .Count();


            IQueryable<UserInfo> query = dbContext.Users
                .AsNoTracking()
                .Where(u => u.Login.ToLower().Contains(search) ||
                                         u.Surname.ToLower().Contains(search) ||
                                         u.Name.ToLower().Contains(search))
                .Select(u => new UserInfo()
                {
                    Id = u.Id.ToString(),
                    Login = u.Login,
                    Role = u.Role,
                    Name = u.Name,
                    Surname = u.Surname
                });



            if (!string.IsNullOrWhiteSpace(filter.Order))
            {
                switch (filter.SortDirection)
                {
                    case 1: // По возрастанию
                        query = filter.Order switch
                        {
                            "Login" => query.OrderBy(u => u.Login),
                            "Role" => query.OrderBy(u => u.Role),
                            "Surname" => query.OrderBy(u => u.Surname),
                            "Name" => query.OrderBy(u => u.Name),
                            _ => query.OrderBy(u => u.Id)
                        };
                        break;

                    case 2: // По убыванию
                        query = filter.Order switch
                        {
                            "Login" => query.OrderByDescending(u => u.Login),
                            "Role" => query.OrderByDescending(u => u.Role),
                            "Surname" => query.OrderByDescending(u => u.Surname),
                            "Name" => query.OrderByDescending(u => u.Name),
                            _ => query.OrderByDescending(u => u.Id)
                        };
                        break;
                }
            }
            query = query
                .Skip(skipCount)
                .Take(filter.PageSize);
            return new UsersTableResponse() { Users = await query.ToListAsync(), TotalCount = totatlCount };
        }
        public async Task<GroupsByUsersResponse> GetGroupsByUsers(GroupsByUsersRequest filter)
        {
            int skipCount = filter.Page * filter.PageSize;

            if (skipCount < 0)
            {
                skipCount = 0;
            }

            List<int> userIds = filter.Users.Select(u => Convert.ToInt32(u.Id)).ToList();

            int selectedCount = filter.Users.Count();

            int totalCount = dbContext.UserGroups
               .Where(ug => userIds.Contains(ug.User_ID))
               .GroupBy(ug => ug.Group_ID)
               .Where(g => g.Select(ug => ug.User_ID).Distinct().Count() == selectedCount
                         && g.Select(ug => ug.User_ID).All(id => userIds.Contains(id))
               )
               .Select(g => new GroupInfo
               {
                   Id = g.First().Group.Id,
                   Name = g.First().Group.Name
               })
               .Count();


            List<GroupInfo> groups = dbContext.UserGroups
               .Where(ug => userIds.Contains(ug.User_ID))
               .GroupBy(ug => ug.Group_ID)
               .Where(g => g.Select(ug => ug.User_ID).Distinct().Count() == selectedCount
                         && g.Select(ug => ug.User_ID).All(id => userIds.Contains(id))
               )
               .Select(g => new GroupInfo
               {
                   Id = g.First().Group.Id,
                   Name = g.First().Group.Name
               })
               .ToList();



            return new GroupsByUsersResponse() { Groups = groups, TotalCount = totalCount };
        }

        public async Task<int> GetCount()
        {
            return dbContext.Users.Count();
        }

        public async Task<int> Registrate(UserRegistrate user)
        {
            if (user is null)
            {
                return -1;
            }

            User? userFromDB = await dbContext.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Login == user.Login);

            if (userFromDB is not null)
            {
                return -2;
            }

            User registerUser = new User
            {
                Login = user.Login,
                Password = passwordHasher.GenerateHashBCrypt(user.Password),
                Role = user.Role,
                Name = user.Name,
                Surname = user.Surname
            };

            await dbContext.AddAsync(registerUser);
            await dbContext.SaveChangesAsync();

            return 0;
        }
        public async Task<int> Edit(UserEdit user)
        {
            if (user is null)
            {
                return -1;
            }

            if (user.OldLogin != user.Login)
            {
                User? userFromNewLogin = await dbContext.Users
                .FirstOrDefaultAsync(u => u.Login == user.Login);

                if (userFromNewLogin is not null)
                {
                    return -2;
                }
            }

            User? userFromDB = await dbContext.Users
                .FirstOrDefaultAsync(u => u.Login == user.OldLogin);

            if (userFromDB is null)
            {
                return -3;
            }

            userFromDB.Login = user.Login;
            userFromDB.Password = passwordHasher.GenerateHashBCrypt(user.Password);
            userFromDB.Role = user.Role;
            userFromDB.Name = user.Name;
            userFromDB.Surname = user.Surname;

            await dbContext.SaveChangesAsync();

            return 0;
        }

        public async Task<int> Delete(UserInfo user)
        {
            if (user is null)
            {
                return -1;
            }

            User? userFromDB = await dbContext.Users
                .FirstOrDefaultAsync(u => u.Login == user.Login);

            if (userFromDB is null)
            {
                return -2;
            }

            try
            {
                dbContext.Users
                    .Where(u => u.Login == user.Login)
                    .ExecuteDelete();
            }
            catch (Exception)
            {
                return -3;
            }


            await dbContext.SaveChangesAsync();

            return 0;
        }
    }
}
