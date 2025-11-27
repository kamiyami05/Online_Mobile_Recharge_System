using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace sem3.Services.Payment
{
    public interface IPaymentService
    {
        PaymentResult ProcessPayment(decimal amount, string phone, string planName, string operatorName);
    }

    public class PaymentResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string TransactionId { get; set; }
    }
}