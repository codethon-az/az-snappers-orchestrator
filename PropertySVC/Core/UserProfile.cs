using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PropertySVC.Core
{
    public class UserProfile
    {
        public string UserId { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MiddleName { get; set; }
        public int CreditScore { get; set; }
        public int AUM { get; set; }
        public int RelationshipAge { get; set; }
        public string Phone { get; set; }
        public string EmailAddress { get; set; }
        public string AccountNumber { get; set; }
    }
}
