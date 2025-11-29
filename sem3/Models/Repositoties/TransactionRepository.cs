using System;
using System.Collections.Generic;
using System.Linq;
using sem3.Models.Entities;

namespace sem3.Models.Repositories
{
	public class TransactionRepository : IDisposable
	{
		private readonly OnlineRechargeDBEntities _context;

		public TransactionRepository()
		{
			_context = new OnlineRechargeDBEntities();
		}

        public List<Transaction> GetAll()
        {
            return _context.Transactions
                           .Include("User")
                           .Include("PaymentDetails")
                           .Include("TransactionScripts")
                           .OrderByDescending(t => t.TransactionID)
                           .ToList();
        }
        public Transaction GetById(int id)
		{
			return _context.Transactions.Find(id);
		}
		public void Dispose()
		{
			_context.Dispose();
		}
	}
}