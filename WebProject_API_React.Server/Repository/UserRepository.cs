using System;
using WebProject.Server.Data;
using WebProject.Server.Models;
using WebProject.Server.Repository.IRepository;

namespace WebProject.Server.Repository
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
