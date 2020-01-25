using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PropertySVC.Model
{
    public class MortgageQuoteRequest
    {
        public AccountDetails AccountDetails { get; set; } 
        public PropertyDetails PropertyDetails { get; set; }
    }

    public class AccountDetails
    {
        public string AccountNumber { get; set; }
        public string UserId { get; set; }
        public string AUM { get; set; }
        public int Tenure { get; set; }
    }

    public class PropertyDetails
    {
        public string Zipcode { get; set; }
        public string Area { get; set; }
        public DateTime contrustedIn { get; set; }
        public int AgeInYears { get; set; }
        public int NumberOfBedrooms { get; set; }
        public string Cost { get; set; }
        public string PropertyId { get; set; }
    }

    public class MortgageQuoteResponse
    {
        public string PropertyId { get; set; }
        public string UserId { get; set; }
        public string RateofInterest { get; set; }
        public bool IsPreApproved { get; set; }
        public decimal DownPayment { get; set; }
        public string AccountNumber { get; set; }
        public decimal MonthlyEMI { get; set; }

    }
}
