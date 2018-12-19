using System;
using System.Collections.Generic;
using System.Text;

namespace DivisaAlexaSkill.Domain
{
    public class DivisaResultModel
    {
        public Result Result { get; set; }
        public string Status { get; set; }
    }

    public class Result
    {
        public DateTime Updated { get; set; }
        public string Source { get; set; }
        public string Target { get; set; }
        public double Value { get; set; }
        public double Quantity { get; set; }
        public double Amount { get; set; }
    }
}
