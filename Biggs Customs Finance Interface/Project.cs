using System;

namespace Biggs_Customs_Finance_Interface
{
    class Project
    {
        public Product Product { get; set; }
        public int OrderNumber { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal CustomCost { get; set; }
        public decimal TotalIncome { get; set; }
        public string CustomerFirstName { get; set; }
        public string CustomerLastName { get; set; }
        public string PhoneNumber { get; set; }
        public string Instagram { get; set; }
        public string ThumbnailLocation { get; set; }
    }
}
