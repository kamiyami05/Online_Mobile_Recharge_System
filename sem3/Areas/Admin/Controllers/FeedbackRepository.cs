using sem3.Models.Entities;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace sem3.Models.Repositories
{
    public class FeedbackRepository : IDisposable
    {
        private readonly OnlineRechargeDBEntities _context;

        public FeedbackRepository()
        {
            _context = new OnlineRechargeDBEntities();
        }

        public List<Feedback> GetAll()
        {
            return _context.Feedbacks
                .Include("User")
                .OrderByDescending(f => f.SubmitDate)
                .ToList();
        }

        public Feedback GetById(int id)
        {
            return _context.Feedbacks.Find(id);
        }

        public void Delete(int id)
        {
            var feedback = _context.Feedbacks.Find(id);
            if (feedback != null)
            {
                _context.Feedbacks.Remove(feedback);
                _context.SaveChanges();
            }
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}