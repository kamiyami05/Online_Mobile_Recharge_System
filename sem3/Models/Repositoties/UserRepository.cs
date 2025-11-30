using System;
using System.Collections.Generic;
using System.Linq;
using sem3.Models.Entities;

namespace sem3.Models.Repositories
{
    public class UserRepository : IDisposable
    {
        private readonly OnlineRechargeDBEntities _context;

        public UserRepository()
        {
            _context = new OnlineRechargeDBEntities();
        }

        public List<User> GetAll()
        {
            return _context.Users.ToList();
        }
        public void Update(User user)
        {
            var existingUser = _context.Users.Find(user.UserID);
            if (existingUser != null)
            {
                existingUser.Active = user.Active;
                _context.SaveChanges();
            }
        }
        public User GetById(int id)
        {
            return _context.Users.Find(id);
        }
        public void Dispose()
        {
            _context.Dispose();
        }
    }
}