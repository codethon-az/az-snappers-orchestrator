using PropertySVC.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PropertySVC
{
    public class Response
    {
        public bool isHouse { get; set; }
        public bool propertyFound { get; set; }
        public bool quoteFound { get; set; }
        public List<Property> propertyList { get; set; }
        public string accountNumber { get; set; }
        public UserProfile user { get; set; }

        public Response()
        {
            propertyList = new List<Property>();
        }

    }

    public class Property
    {
        public string imageUrl { get; set; }
        public string zip;
        public string propertyId;
        public string address { get; set; }
        public string area { get; set; }
        public int numberOfBedrooms { get; set; }
        public int numberOfBathrooms { get; set; }
        public string cost { get; set; }
        public string status { get; set; }
        public string tax { get; set; }
        public QuoteDetails quoteDetails { get; set; }
        public NeighborhoodDetails neighborhoodDetails { get; set; }
    }

    public class QuoteDetails
    {
        public bool isPreApproved { get; set; }
        public float rateOfInterest { get; set; }
        public float downPayment { get; set; }
        public float monthlyPayment { get; set; }
    }

    public class NeighborhoodDetails
    {
        public int overallRanking { get; set; }
        public int population { get; set; }
    }
}
