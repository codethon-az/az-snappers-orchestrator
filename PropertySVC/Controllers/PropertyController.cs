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
using Microsoft.Extensions.Logging;
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

        private readonly ILogger _logger;

        public PropertyController(ILogger<PropertyController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public string Get()
        {
            _logger.LogInformation("Orchestrator service is healthy");
            return "Orchestrator service is healthy";
        }

        // GET: api/Property/5/userid
        //[HttpGet("{id}", Name = "Get")]
        [Route("{username}")]
        public string Get(string username)
        {
            _logger.LogInformation($"Getting property details for {username} for image url {HttpContext.Request.Query["image"]}");
            var imgSearchReq = new SearchImageRequest { orig_image_url = HttpContext.Request.Query["image"] };
            var response = new Response();


            _logger.LogInformation("ORCHESTRATOR SERVICE: get all images from DB");
            using (var sqlConn = new SqlConnection(DB_CONN))
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
            _logger.LogInformation($"ORCHESTRATOR SERVICE: retrieved {imgSearchReq.comp_image_urls.Count} images from DB");


            _logger.LogInformation($"ORCHESTRATOR SERVICE: verify if the image is a house");
            VerifyImage verifyImg;
            using (var httpClient = new HttpClient())
            {
                _logger.LogInformation($"ORCHESTRATOR SERVICE: calling {ML_API}/? image_url={imgSearchReq.orig_image_url}");
                using (var res = httpClient.GetAsync(ML_API + "/?image_url=" + imgSearchReq.orig_image_url))
                {
                    var apiResponse = res.Result.Content.ReadAsStringAsync().Result;
                    _logger.LogInformation($"ORCHESTRATOR SERVICE: received response from {ML_API}/? image_url={imgSearchReq.orig_image_url} ---- {res.Result.StatusCode}");
                    if (res.Result.IsSuccessStatusCode)
                    {
                        _logger.LogInformation($"ORCHESTRATOR SERVICE: Deserializing response {apiResponse}");
                        verifyImg = JsonConvert.DeserializeObject<VerifyImage>(apiResponse);
                        _logger.LogInformation($"ORCHESTRATOR SERVICE: Deserialized response {string.Join(';', verifyImg.tags.ToArray())}");
                    }
                    else
                    {
                        _logger.LogInformation($"ORCHESTRATOR SERVICE: Image is not a house");
                        verifyImg = new VerifyImage { is_house = false };
                        _logger.LogInformation($"ORCHESTRATOR SERVICE: Image is {string.Join(';', verifyImg.tags.ToArray())}");
                    }
                }
                response.isHouse = verifyImg.is_house;
            }

            _logger.LogInformation("ORCHESTRATOR SERVICE: get user details from DB");
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
                                userId = reader.GetInt32(0).ToString(),
                                username = reader.GetString(1),
                                firstName = reader.GetString(3),
                                lastName = reader.GetString(4),
                                middleName = reader.GetString(5),
                                creditScore = reader.GetInt32(6),
                                aum = reader.GetInt32(7),
                                relationshipAge = reader.GetInt32(8),
                                phone = reader.GetString(9),
                                emailAddress = reader.GetString(10),
                                accountNumber = reader.GetString(11)
                            };
                        }
                    }
                }
            }
            _logger.LogInformation($"ORCHESTRATOR SERVICE: retrieved {JsonConvert.SerializeObject(response.user)}  from DB");


            if (!verifyImg.is_house)
            {
                _logger.LogInformation($"ORCHESTRATOR SERVICE: returning early since the image is not a house");
                return JsonConvert.SerializeObject(response);
            }

            _logger.LogInformation($"ORCHESTRATOR SERVICE: send query image and all DB images to ML model");
            SearchImageResponse imgSearchResponse;
            using (var httpClient = new HttpClient())
            {
                var httpContent = new StringContent(JsonConvert.SerializeObject(imgSearchReq), Encoding.UTF8, "application/json");
                {
                    _logger.LogInformation($"ORCHESTRATOR SERVICE: calling {ML_API} using {httpContent}");
                    using (var res = httpClient.PostAsync(ML_API, httpContent))
                    {
                        var apiResponse = res.Result.Content.ReadAsStringAsync().Result;
                        _logger.LogInformation($"ORCHESTRATOR SERVICE: received response from {ML_API} for image matching ---- {res.Result.StatusCode}");
                        if (res.Result.IsSuccessStatusCode)
                        {
                            _logger.LogInformation($"ORCHESTRATOR SERVICE: Deserializing response {apiResponse}");
                            imgSearchResponse = JsonConvert.DeserializeObject<SearchImageResponse>(apiResponse);
                            response.propertyFound = true;
                            _logger.LogInformation($"ORCHESTRATOR SERVICE: Deserialized response {imgSearchResponse.image_url} and {imgSearchResponse.message}");
                        }
                        else
                        {
                            response.propertyFound = false;
                            var p = new Random().Next(0, 65);
                            _logger.LogInformation($"ORCHESTRATOR SERVICE: Image not matched... picking a random image {p}");
                            if (p == 0)
                            {
                                imgSearchResponse = new SearchImageResponse { image_url = "https://azsnappersblob.blob.core.windows.net/images/HouseNotFound.PNG", message = "Property not found" };
                            }
                            else
                            {
                                imgSearchResponse = new SearchImageResponse { image_url = imgSearchReq.comp_image_urls[p - 1], message = "Randomly chosen house image" };
                            }
                            _logger.LogInformation($"ORCHESTRATOR SERVICE: Image not matched... picked a random image {JsonConvert.SerializeObject(imgSearchResponse)}");
                        }
                    }
                }
            }

            _logger.LogInformation($"ORCHESTRATOR SERVICE: get property from return image url");
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
            _logger.LogInformation($"ORCHESTRATOR SERVICE: retrieved property details from DB --- {JsonConvert.SerializeObject(response.propertyList)}");

            var mtgReq = new MortgageQuoteRequest();
            mtgReq.AccountDetails = new AccountDetails { 
                AccountNumber = response.user.accountNumber, 
                AUM = response.user.aum.ToString(), 
                Tenure = response.user.relationshipAge, 
                UserId = response.user.userId 
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

            _logger.LogInformation($"ORCHESTRATOR SERVICE: send user and property details to mortgage service --- {JsonConvert.SerializeObject(mtgReq)}");

            MortgageQuoteResponse mtgResponse;
            using (var httpClient = new HttpClient())
            {
                var httpContent = new StringContent(JsonConvert.SerializeObject(mtgReq), Encoding.UTF8, "application/json");
                {
                    _logger.LogInformation($"ORCHESTRATOR SERVICE: calling {MTG_API} using {httpContent}");
                    using (var res = httpClient.PostAsync(MTG_API, httpContent))
                    {
                        var apiResponse = res.Result.Content.ReadAsStringAsync().Result;
                        _logger.LogInformation($"ORCHESTRATOR SERVICE: received response from {MTG_API} for mortgage saearch ---- {res.Result.StatusCode}");
                        if (res.Result.IsSuccessStatusCode)
                        {
                            _logger.LogInformation($"ORCHESTRATOR SERVICE: Deserializing response {apiResponse}");
                            mtgResponse = JsonConvert.DeserializeObject<MortgageQuoteResponse>(apiResponse);
                            response.quoteFound = true;
                            _logger.LogInformation($"ORCHESTRATOR SERVICE: Deserialized response {JsonConvert.SerializeObject(mtgResponse)}");
                        }
                        else
                        {
                            mtgResponse = new MortgageQuoteResponse { 
                                AccountNumber = mtgReq.AccountDetails.AccountNumber, 
                                UserId = response.user.userId,
                                IsPreApproved = false,
                                PropertyId = mtgReq.PropertyDetails.PropertyId,
                                DownPayment = 0,
                                MonthlyEMI = 0,
                                RateofInterest = "0"
                            };
                            response.quoteFound = false;
                            _logger.LogInformation($"ORCHESTRATOR SERVICE: Image not matched... setting default dummy response {JsonConvert.SerializeObject(mtgResponse)}");
                        }
                        response.propertyList[0].quoteDetails.downPayment = mtgResponse.DownPayment;
                        response.propertyList[0].quoteDetails.isPreApproved = mtgResponse.IsPreApproved;
                        response.propertyList[0].quoteDetails.monthlyPayment = mtgResponse.MonthlyEMI;
                        response.propertyList[0].quoteDetails.rateOfInterest = mtgResponse.RateofInterest;
                    }
                }
            }

            _logger.LogInformation($"ORCHESTRATOR SERVICE: send user, property and mortgage details back to UI {JsonConvert.SerializeObject(response)}");
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
