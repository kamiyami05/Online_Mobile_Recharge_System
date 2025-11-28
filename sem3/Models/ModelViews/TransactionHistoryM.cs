using System;

namespace sem3.Models.ModelViews
{
    public class TransactionHistoryM
    {
        public int TransactionID { get; set; }
        public string TransactionType { get; set; }
        public decimal Amount { get; set; }
        public DateTime TransactionDate { get; set; }
        public string Status { get; set; }
        public string MobileNumber { get; set; }
        public string PlanDetails { get; set; }
        public string Operator { get; set; }
    }
}