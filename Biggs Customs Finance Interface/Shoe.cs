using System;

namespace Biggs_Customs_Finance_Interface
{
    class Shoe
    {
        public int SKU { get; set; }
        public string Name { get; set; }
        public string Brand { get; set; }
        public string ShoeType { get; set; }
        public decimal Cost { get; set; }
        public string Keywords { get; set; }

        public Shoe(int sku, string name, string brand, string shoetype, decimal cost, string keywords)
        {
            SKU = sku;
            Name = name;
            Brand = brand;
            ShoeType = shoetype;
            Cost = cost;
            Keywords = keywords;
        }
    }
}
