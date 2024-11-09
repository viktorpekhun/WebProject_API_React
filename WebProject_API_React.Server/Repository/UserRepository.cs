using System;
using WebProject_API_React.Server.Data;
using WebProject_API_React.Server.Models;
using WebProject_API_React.Server.Repository.IRepository;

namespace WebProject_API_React.Server.Repository
{
    public class UserRepository : Repository<User>, IUserRepository
    {
        private ApplicationDbContext _context;

        public UserRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

    }
}
