using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace PropertySVC.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PropertyController : ControllerBase
    {
        // GET: api/Property
        [HttpGet]
        public PrpertyListRes Get()
        {

            QuoteResponse response = new QuoteResponse() { HouseCost = 2500000, IsPreapproved = true, RateofInterest = "2.40" };
            House house = new House() { AgeInYears = 10, Area = "2000Sqft", contrustiedIn = DateTime.Now.AddYears(-80), Cost = 500000, NumberOfBedrooms = 4, Zipcode = "07698",NumberOfBathooms=3,Address="23 Cottage way , Fanwood , NJ 07023" ,quoteDetails = response };
            house.quoteDetails =new QuoteResponse();
            house.quoteDetails = response;
            List<House> houses = new List<House>();
            houses.Add(house);
            //houses.Add(house);
            //houses.Add(house);
            //houses.Add(house);
            Account acc = new Account() { AccountNumber = "345654", KeyACcount = "2019-12-01:478.78.47", FA = "478", office = "474" };
            PrpertyListRes objPrpertyListRes = new PrpertyListRes() { accotDetails = acc, houseList = houses, IsHouse = true };
            var jsonresponse = Newtonsoft.Json.JsonConvert.SerializeObject(objPrpertyListRes);
            // return new string[] { jsonresponse };
            return objPrpertyListRes;
        }

        // GET: api/Property/5
        [HttpGet("{id}", Name = "Get")]
        public string Get(int id)
        {
            return "value";
        }

        // POST: api/Property
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT: api/Property/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }



        // GET: api/Property/Listing
        [HttpGet("{id}", Name = "Listing")]
        public PrpertyListRes Listing()
        {

            
                         QuoteResponse response = new QuoteResponse() { HouseCost = 2500000, IsPreapproved = true, RateofInterest = "2.40" };
            House house = new House() { AgeInYears = 10, Area = "2000Sqft", contrustiedIn = DateTime.Now.AddYears(-80), Cost = 500000, NumberOfBedrooms = 4, Zipcode = "07698",quoteDetails= response };
            List<House> houses = new List<House>();
            houses.Add(house);
            houses.Add(house);
            houses.Add(house);
            houses.Add(house);
            Account acc = new Account() { AccountNumber = "345654", KeyACcount = "2019-12-01:478.78.47", FA = "478", office = "474" };
            PrpertyListRes objPrpertyListRes = new PrpertyListRes() { accotDetails = acc, houseList = houses, IsHouse = true };
            var jsonresponse = Newtonsoft.Json.JsonConvert.SerializeObject(objPrpertyListRes);
                        // return new string[] { jsonresponse };

            return objPrpertyListRes;
        }
    }
}
