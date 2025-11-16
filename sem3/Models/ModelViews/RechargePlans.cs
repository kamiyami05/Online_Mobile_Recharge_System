using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace sem3.Models.ModelViews
{
    public class RechargePlans
    {
        public int PlanID { get; set; }
        
        public string PlanType { get; set; }

        public string PlanName { get; set; }

        public decimal Amount { get; set; }

        public int TalkTimeMinutes { get; set; }
        public int DataMB { get; set; }
        public string Details { get; set; }
        public bool IsActive { get; set; }

    }
}