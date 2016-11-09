using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication2
{
    class LineItem
    {
        public DateTime Date { get; set; }
        public Merchant Merchant { get; set; }
        public Decimal Amount { get; set; }
    }

    class MonthExpenditures
    {
        public DateTime Month { get; set; }
        public List<LineItem> Items = new List<LineItem>();
        public Decimal TotalAmount { get; set; }
    }

    class Merchant
    {
        public String Name { get; set; }
        public MerchantType Type { get; set; }
        public String Remark { get; set; }
    }

    enum MerchantType
    {
        UNCLASSIFIED,
        AMAZON,
        BOOK,
        CAR,
        CLOTH,
        ELECTRICITY,
        FLIPKART,
        FOOD,
        FURNITURE,
        GROCERY,
        MEDICAL,
        PAYMENT,
        PHONE,
        SALON,
        STUPID,
        TAX,
        TOYS,
        TRAVEL,
        TV,
        VEGGIES,
        PREVIOUSBALANCE
    }
}
