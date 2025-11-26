using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace sem3.Services.Payment
{
    public class VisaPayment : IPaymentService
    {
        public PaymentResult ProcessPayment(decimal amount, string phone, string planName, string operatorName)
        {
            // Fake Visa processing logic
            return new PaymentResult
            {
                Success = true,
                Message = "Visa payment successful!",
                TransactionId = Guid.NewGuid().ToString()
            };
        }
    }
}