using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net.Http;
using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[assembly: Parallelize(Workers = 10, Scope = ExecutionScope.MethodLevel)]
namespace Session2Part1Homework
{
    [TestClass]
    public class HttpClientTest
    {
        private static HttpClient httpClient;

        private static readonly string BaseURL = "https://petstore.swagger.io/v2/";

        private static readonly string PetsEndpoint = "pet";

        private static string GetURL(string enpoint) => $"{BaseURL}{enpoint}";

        private static Uri GetURI(string endpoint) => new Uri(GetURL(endpoint));

        private readonly List<PetModel> cleanUpList = new List<PetModel>();



        [TestInitialize]
        public void TestInitialize()
        {
            httpClient = new HttpClient();
        }

        [TestCleanup]
        public async Task TestCleanUp()
        {
           foreach (var data in cleanUpList)
            {
                var httpResponse = await httpClient.DeleteAsync(GetURL($"{PetsEndpoint}/{data.Id}"));
            }
        }


        [TestMethod]
        public async Task PutMethod()
        {
            #region create data
            PetModel petData = new PetModel();
            List<Category> category = new List<Category>();

            // Create Json Object
            category.Add(new Category()
            {
                Id = 01,
                Name = "Pet"
            });

            petData = new PetModel()
            {
                Id = 000001,
                Category = new Category()
                {
                    Id = 001,
                    Name = "Domestic"
                },
                Name = "Bingo",
                PhotoUrls = new List<string>() { "testUrl" },
                Tags = category,
                Status = "Available"
            };

            // Serialize Content
            var request = JsonConvert.SerializeObject(petData);
            var postRequest = new StringContent(request, Encoding.UTF8, "application/json");

            // Send Post Request
            await httpClient.PostAsync(GetURL(PetsEndpoint), postRequest);

            #endregion

            #region get Username of the created data

            // Get Request
            var getResponse = await httpClient.GetAsync(GetURI($"{UsersEndpoint}/{petData.Username}"));

            // Deserialize Content
            var listUserData = JsonConvert.DeserializeObject<PetModel>(getResponse.Content.ReadAsStringAsync().Result);

            // filter created data
            var createdUserData = listUserData.Username;

            #endregion

            #region send put request to update data

            // Update value of userData
            userData = new UserModel()
            {
                Id = listUserData.Id,
                Username = "test123.put.updated",
                FirstName = listUserData.FirstName,
                LastName = listUserData.LastName,
                Email = listUserData.Email,
                Password = listUserData.Password,
                Phone = listUserData.Phone,
                UserStatus = listUserData.UserStatus,
            };
            
            // Serialize Content
            request = JsonConvert.SerializeObject(petData);
            postRequest = new StringContent(request, Encoding.UTF8, "application/json");

            // Send Put Request
            var httpResponse = await httpClient.PutAsync(GetURL($"{UsersEndpoint}/{createdUserData}"), postRequest);

            // Get Status Code
            var statusCode = httpResponse.StatusCode;

            #endregion

            #region get updated data

            // Get Request
            getResponse = await httpClient.GetAsync(GetURI($"{UsersEndpoint}/{petData.Username}"));

            // Deserialize Content
            listUserData = JsonConvert.DeserializeObject<PetModel>(getResponse.Content.ReadAsStringAsync().Result);

            // filter created data
            createdUserData = listUserData.Username;

            #endregion

            #region cleanup data

            // Add data to cleanup list
            cleanUpList.Add(listUserData);

            #endregion

            #region assertion

            // Assertion
            Assert.AreEqual(HttpStatusCode.OK, statusCode, "Status code is not equal to 201");
            Assert.AreEqual(petData.Username, createdUserData, "Username not matching");

            #endregion
            
        }





        /// <summary>
        /// Reusable method
        /// </summary>
        private async Task<HttpResponseMessage> SendAsyncFunction(HttpMethod method, string url, PetModel userData = null)
        {
            HttpRequestMessage httpRequestMessage = new HttpRequestMessage();

            httpRequestMessage.Method = method;
            httpRequestMessage.RequestUri = GetURI(url);
            httpRequestMessage.Headers.Add("Accept", "application/json");

            if (userData != null)
            {
                // Serialize Content
                var request = JsonConvert.SerializeObject(userData);
                httpRequestMessage.Content = new StringContent(request, Encoding.UTF8, "application/json");
            } 

            var httpResponse = await httpClient.SendAsync(httpRequestMessage);

            return httpResponse;
        }


       

    }
}
