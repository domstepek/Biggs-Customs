using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Biggs_Customs_Finance_Interface
{
    class Order
    {
        public int OrderNumber { get; set; }
        public int SKU { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime SellingDate { get; set; }
        public decimal CustomCost { get; set; }
        public decimal TotalIncome { get; set; }
        public string CustomerFirstName { get; set; }
        public string CustomerLastName { get; set; }
        public string PhoneNumber { get; set; }
        public string Instagram { get; set; }
    }
}
