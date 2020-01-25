using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PropertySVC.Core
{
    public class UserProfile
    {
        public string userId { get; set; }
        public string username { get; set; }
        public string password { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string middleName { get; set; }
        public int creditScore { get; set; }
        public int aum { get; set; }
        public int relationshipAge { get; set; }
        public string phone { get; set; }
        public string emailAddress { get; set; }
        public string accountNumber { get; set; }
    }
}
