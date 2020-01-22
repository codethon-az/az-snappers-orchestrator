using System;
using System.Collections.Generic;
using System.Text;

namespace Core
{
    public class House
    {
        public string Zipcode { get; set; }

        public string Address { get; set; } 

        public string Area { get; set; }
        public DateTime contrustiedIn { get; set; }
        public int AgeInYears { get; set; }
        public int NumberOfBedrooms { get; set; }

        public int NumberOfBathooms { get; set; }

        public decimal Cost { get; set; }

        public QuoteResponse quoteDetails { get; set; }

    }
}