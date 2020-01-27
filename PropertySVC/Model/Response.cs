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
        public UserProfile user { get; set; }
        public List<string> milestones { get; set; }

        public Response()
        {
            propertyList = new List<Property>();
            milestones = new List<string>();
            user = new UserProfile();
        }

    }

    public class Property
    {
        public string imageUrl { get; set; }
        public List<string> moreImages { get; set; }
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
        public Property()
        {
            quoteDetails = new QuoteDetails();
            neighborhoodDetails = new NeighborhoodDetails();
            moreImages = new List<string>();
        }
    }

    public class QuoteDetails
    {
        public bool isPreApproved { get; set; }
        public string rateOfInterest { get; set; }
        public decimal downPayment { get; set; }
        public decimal monthlyPayment { get; set; }
    }

    public class NeighborhoodDetails
    {
        public int overallRanking { get; set; }
        public int population { get; set; }
    }
}
