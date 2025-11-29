using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using sem3.Models.Entities;

namespace sem3.Models.Repositories
{
    public class PlanRepository : IDisposable
    {
        private readonly OnlineRechargeDBEntities _context;

        public PlanRepository()
        {
            _context = new OnlineRechargeDBEntities();
        }

        public List<RechargePlan> GetAll()
        {
            return _context.RechargePlans.OrderByDescending(p => p.PlanID).ToList();
        }

        public RechargePlan GetById(int id)
        {
            return _context.RechargePlans.Find(id);
        }

        public void Create(RechargePlan plan)
        {
            _context.RechargePlans.Add(plan);
            _context.SaveChanges();
        }

        public void Update(RechargePlan plan)
        {
            var existingPlan = _context.RechargePlans.Find(plan.PlanID);

            if (existingPlan != null)
            {

                _context.Entry(existingPlan).CurrentValues.SetValues(plan);
                _context.SaveChanges();
            }
        }

        public void Delete(int id)
        {
            var plan = _context.RechargePlans.Find(id);
            if (plan != null)
            {
                _context.RechargePlans.Remove(plan);
                _context.SaveChanges();
            }
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}