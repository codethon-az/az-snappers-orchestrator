using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using PropertySVC.Model;

namespace PropertySVC.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PropertyController : ControllerBase
    {
        private const string DB_CONN = "Server=tcp:az-snappers-db-server.database.windows.net,1433;Initial Catalog=az-snappers-db;Persist Security Info=False;User ID=az-snappers;Password=@ugm3ntedRealt0rs;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";
        private const string ML_API = "https://wklyhttptrigger.azurewebsites.net/api/VisionHttpTrigger";
        private const string MTG_API = "https://az-snappers-mortgages.azurewebsites.net/api/quote";
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

        // GET: api/Property/5/userid
        //[HttpGet("{id}", Name = "Get")]
        [Route("{username}")]
        public string Get(string username)
        {
            var imgSearchReq = new SearchImageRequest { orig_image_url = HttpContext.Request.Query["image"] };
            var response = new Response();

            // get all images from DB
            using(var sqlConn = new SqlConnection(DB_CONN))
            {
                sqlConn.Open();
                var query = "SELECT Path from [dbo].[Image]";
                using(SqlCommand cmd = new SqlCommand(query, sqlConn))
                {
                    using(var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            imgSearchReq.comp_image_urls.Add(reader.GetString(0));
                        }
                    }
                }
            }
            // verify if the image is a house
            VerifyImage verifyImg;
            using (var httpClient = new HttpClient())
            {
                using (var res = httpClient.GetAsync(ML_API + "/?image_url=" + imgSearchReq.orig_image_url))
                {
                    var apiResponse = res.Result.Content.ReadAsStringAsync().Result;
                    if (res.Result.IsSuccessStatusCode)
                        verifyImg = JsonConvert.DeserializeObject<VerifyImage>(apiResponse);
                    else
                        verifyImg = new VerifyImage { is_house = false };
                }
                response.isHouse = verifyImg.is_house;
            }
            // send query image and all DB images to ML model
            if (!verifyImg.is_house)
            {
                return JsonConvert.SerializeObject(verifyImg);
            }
            SearchImageResponse imgSearchResponse;
            using (var httpClient = new HttpClient())
            {
                var httpContent = new StringContent(JsonConvert.SerializeObject(imgSearchReq), Encoding.UTF8, "application/json");
                {
                    using (var res = httpClient.PostAsync(ML_API, httpContent))
                    {
                        var apiResponse = res.Result.Content.ReadAsStringAsync().Result;
                        if (res.Result.IsSuccessStatusCode)
                        {
                            imgSearchResponse = JsonConvert.DeserializeObject<SearchImageResponse>(apiResponse);
                            response.propertyFound = true;
                        }
                        else
                        {
                            imgSearchResponse = new SearchImageResponse { image_url = "https://azsnappersblob.blob.core.windows.net/images/HouseNotFound.PNG", message = "Property not found" };
                            imgSearchResponse = new SearchImageResponse { image_url = "https://azsnappersblob.blob.core.windows.net/images/07722-01.01.PNG", message = "Property not found" };
                            response.propertyFound = false;
                        }
                    }
                }
            }
            // get property from return image url
            using (var sqlConn = new SqlConnection(DB_CONN))
            {
                sqlConn.Open();
                var query = $"select top 1 P.*, I.Path from [dbo].[image] I, [dbo].[Property] P where I.PropertyId = P.PropertyId and I.Path = '{imgSearchResponse.image_url}'";
                using (SqlCommand cmd = new SqlCommand(query, sqlConn))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            response.propertyList.Add(new Property
                            {
                                imageUrl = reader.GetString(16),
                                address = $"{reader.GetString(1)}, {reader.GetString(3)}, {reader.GetString(4)} {reader.GetString(5)}",
                                area = reader.GetDouble(14).ToString(),
                                numberOfBedrooms = reader.GetInt32(7),
                                numberOfBathrooms = reader.GetInt32(8),
                                cost = reader.GetDouble(13).ToString(),
                                status = reader.GetString(12),
                                tax = reader.GetDouble(15).ToString(),
                                zip = reader.GetString(5),
                                propertyId = reader.GetInt32(0).ToString()
                            });
                        }
                    }
                }
            }
            // get user details
            using (var sqlConn = new SqlConnection(DB_CONN))
            {
                sqlConn.Open();
                var query = $"SELECT * from [dbo].[UserProfile] where username='{username}'";
                using (SqlCommand cmd = new SqlCommand(query, sqlConn))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            response.user = new Core.UserProfile
                            {
                                UserId = reader.GetInt32(0).ToString(),
                                Username = reader.GetString(1),
                                FirstName = reader.GetString(3),
                                LastName = reader.GetString(4),
                                MiddleName = reader.GetString(5),
                                CreditScore = reader.GetInt32(6),
                                AUM = reader.GetInt32(7),
                                RelationshipAge = reader.GetInt32(8),
                                Phone = reader.GetString(9),
                                EmailAddress = reader.GetString(10),
                                AccountNumber = reader.GetString(11)
                            };
                        }
                    }
                }
            }
            // send user and property details to mortgage service
            var mtgReq = new MortgageQuoteRequest();
            mtgReq.AccountDetails = new AccountDetails { 
                AccountNumber = response.user.AccountNumber, 
                AUM = response.user.AUM.ToString(), 
                Tenure = response.user.RelationshipAge, 
                UserId = response.user.UserId 
            };
            var age = new Random().Next(5, 50);
            mtgReq.PropertyDetails = new PropertyDetails
            {
                Zipcode = response.propertyList[0].zip,
                Area = response.propertyList[0].area.ToString(),
                Cost = response.propertyList[0].cost,
                AgeInYears = age,
                contrustedIn = DateTime.Now.AddYears(-1 * age),
                NumberOfBedrooms = response.propertyList[0].numberOfBedrooms,
                PropertyId = response.propertyList[0].propertyId
            };
            MortgageQuoteResponse mtgResponse;
            using (var httpClient = new HttpClient())
            {
                var httpContent = new StringContent(JsonConvert.SerializeObject(mtgReq), Encoding.UTF8, "application/json");
                {
                    using (var res = httpClient.PostAsync(MTG_API, httpContent))
                    {
                        var apiResponse = res.Result.Content.ReadAsStringAsync().Result;
                        if (res.Result.IsSuccessStatusCode)
                        {
                            mtgResponse = JsonConvert.DeserializeObject<MortgageQuoteResponse>(apiResponse);
                            response.quoteFound = true;
                        }
                        else
                        {
                            mtgResponse = new MortgageQuoteResponse { 
                                AccountNumber = mtgReq.AccountDetails.AccountNumber, 
                                UserId = response.user.UserId,
                                IsPreApproved = false,
                                PropertyId = mtgReq.PropertyDetails.PropertyId,
                                DownPayment = 0,
                                MonthlyEMI = 0,
                                RateofInterest = "0"
                            };
                            response.quoteFound = false;
                        }
                    }
                }
            }
            // send user, property and mortgage details back to UI

            return JsonConvert.SerializeObject(response);
        }


        //// GET: api/Property/Listing
        //[HttpGet("{id}", Name = "Listing")]
        //public PrpertyListRes Listing()
        //{

            
        //    QuoteResponse response = new QuoteResponse() { HouseCost = 2500000, IsPreapproved = true, RateofInterest = "2.40" };
        //    House house = new House() { AgeInYears = 10, Area = "2000Sqft", contrustiedIn = DateTime.Now.AddYears(-80), Cost = 500000, NumberOfBedrooms = 4, Zipcode = "07698",quoteDetails= response };
        //    List<House> houses = new List<House>();
        //    houses.Add(house);
        //    houses.Add(house);
        //    houses.Add(house);
        //    houses.Add(house);
        //    Account acc = new Account() { AccountNumber = "345654", KeyACcount = "2019-12-01:478.78.47", FA = "478", office = "474" };
        //    PrpertyListRes objPrpertyListRes = new PrpertyListRes() { accotDetails = acc, houseList = houses, IsHouse = true };
        //    var jsonresponse = Newtonsoft.Json.JsonConvert.SerializeObject(objPrpertyListRes);
        //                // return new string[] { jsonresponse };

        //    return objPrpertyListRes;
        //}
    }
}
