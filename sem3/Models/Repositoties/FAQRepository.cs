using System;
using System.Collections.Generic;
using System.Linq;
using sem3.Models.Entities;

namespace sem3.Models.Repositories
{
    public class FAQRepository : IDisposable
    {
        private readonly OnlineRechargeDBEntities _context;

        public FAQRepository()
        {
            _context = new OnlineRechargeDBEntities();
        }

        public List<FAQ> GetActiveFAQs()
        {
            return _context.FAQs
                           .Where(f => f.IsActive == true)
                           .OrderByDescending(f => f.FAQID)
                           .ToList();
        }
        public List<FAQ> GetAll()
        {
            return _context.FAQs.OrderByDescending(f => f.FAQID).ToList();
        }

        public FAQ GetById(int id)
        {
            return _context.FAQs.Find(id);
        }

        public void Add(FAQ faq)
        {
            faq.OrderIndex = 0;
            _context.FAQs.Add(faq);
            _context.SaveChanges();
        }

        public void Update(FAQ faq)
        {
            var item = _context.FAQs.Find(faq.FAQID);
            if (item != null)
            {
                item.Question = faq.Question;
                item.Answer = faq.Answer;
                item.IsActive = faq.IsActive;
                _context.SaveChanges();
            }
        }

        public void Delete(int id)
        {
            var item = _context.FAQs.Find(id);
            if (item != null)
            {
                _context.FAQs.Remove(item);
                _context.SaveChanges();
            }
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}