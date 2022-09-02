using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json;
using Nexer.Domain.Models.DataTransferObjects;
using Nexer.Domain.Models.Enumerations;
using Nexer.WeatherAPI;
using Nexer.WeatherAPI.Responses;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Nexer.IntegrationTesting
{
    public class WeatherControllerIntegrationTest : IClassFixture<WebApplicationFactory<Startup>>
    {
        private readonly WebApplicationFactory<Startup> _factory;

        public WeatherControllerIntegrationTest(WebApplicationFactory<Startup> factory)
        {
            _factory = factory;
        }

        [Theory]
        [InlineData(1, "dockan", "2019-01-10")]
        [InlineData(1, "dockan", "2019-01-11")]
        [InlineData(1, "dockan", "2019-01-12")]
        [InlineData(1, "dockan", "2019-01-13")]
        [InlineData(1, "dockan", "2019-01-14")]
        [InlineData(1, "dockan", "2019-01-15")]
        [InlineData(1, "dockan", "2019-01-16")]
        [InlineData(1, "dockan", "2019-01-17")]
        [InlineData(1, "dockan", "2019-01-10", "temperature")]
        [InlineData(1, "dockan", "2019-01-11", "temperature")]
        [InlineData(1, "dockan", "2019-01-12", "temperature")]
        [InlineData(1, "dockan", "2019-01-13", "temperature")]
        [InlineData(1, "dockan", "2019-01-14", "temperature")]
        [InlineData(1, "dockan", "2019-01-15", "temperature")]
        [InlineData(1, "dockan", "2019-01-16", "temperature")]
        [InlineData(1, "dockan", "2019-01-17", "temperature")]
        [InlineData(1, "dockan", "2019-01-10", "humidity")]
        [InlineData(1, "dockan", "2019-01-11", "humidity")]
        [InlineData(1, "dockan", "2019-01-12", "humidity")]
        [InlineData(1, "dockan", "2019-01-13", "humidity")]
        [InlineData(1, "dockan", "2019-01-14", "humidity")]
        [InlineData(1, "dockan", "2019-01-15", "humidity")]
        [InlineData(1, "dockan", "2019-01-16", "humidity")]
        [InlineData(1, "dockan", "2019-01-17", "humidity")]
        [InlineData(1, "dockan", "2019-01-10", "rainfall")]
        [InlineData(1, "dockan", "2019-01-11", "rainfall")]
        [InlineData(1, "dockan", "2019-01-12", "rainfall")]
        [InlineData(1, "dockan", "2019-01-13", "rainfall")]
        [InlineData(1, "dockan", "2019-01-14", "rainfall")]
        [InlineData(1, "dockan", "2019-01-15", "rainfall")]
        [InlineData(1, "dockan", "2019-01-16", "rainfall")]
        [InlineData(1, "dockan", "2019-01-17", "rainfall")]
        [InlineData(2, "dockan", "2019-01-10")]
        [InlineData(2, "dockan", "2019-01-11")]
        [InlineData(2, "dockan", "2019-01-12")]
        [InlineData(2, "dockan", "2019-01-13")]
        [InlineData(2, "dockan", "2019-01-14")]
        [InlineData(2, "dockan", "2019-01-15")]
        [InlineData(2, "dockan", "2019-01-16")]
        [InlineData(2, "dockan", "2019-01-17")]
        [InlineData(2, "dockan", "2019-01-10", "temperature")]
        [InlineData(2, "dockan", "2019-01-11", "temperature")]
        [InlineData(2, "dockan", "2019-01-12", "temperature")]
        [InlineData(2, "dockan", "2019-01-13", "temperature")]
        [InlineData(2, "dockan", "2019-01-14", "temperature")]
        [InlineData(2, "dockan", "2019-01-15", "temperature")]
        [InlineData(2, "dockan", "2019-01-16", "temperature")]
        [InlineData(2, "dockan", "2019-01-17", "temperature")]
        [InlineData(2, "dockan", "2019-01-10", "humidity")]
        [InlineData(2, "dockan", "2019-01-11", "humidity")]
        [InlineData(2, "dockan", "2019-01-12", "humidity")]
        [InlineData(2, "dockan", "2019-01-13", "humidity")]
        [InlineData(2, "dockan", "2019-01-14", "humidity")]
        [InlineData(2, "dockan", "2019-01-15", "humidity")]
        [InlineData(2, "dockan", "2019-01-16", "humidity")]
        [InlineData(2, "dockan", "2019-01-17", "humidity")]
        [InlineData(2, "dockan", "2019-01-10", "rainfall")]
        [InlineData(2, "dockan", "2019-01-11", "rainfall")]
        [InlineData(2, "dockan", "2019-01-12", "rainfall")]
        [InlineData(2, "dockan", "2019-01-13", "rainfall")]
        [InlineData(2, "dockan", "2019-01-14", "rainfall")]
        [InlineData(2, "dockan", "2019-01-15", "rainfall")]
        [InlineData(2, "dockan", "2019-01-16", "rainfall")]
        [InlineData(2, "dockan", "2019-01-17", "rainfall")]
        public async Task ShouldGetDataForDeviceAndSensorType(int endpoint, string deviceId, string date, string sensorType = "")
        {
            var client = _factory.CreateClient();
            
            HttpResponseMessage responseMessage = null;

            switch (endpoint)
            {
                case 1:
                    if (string.IsNullOrEmpty(sensorType))
                    {
                        responseMessage = await client.GetAsync($"/api/v1/devices/{deviceId}/data/{date}");
                    }
                    else
                    {
                        responseMessage = await client.GetAsync($"/api/v1/devices/{deviceId}/data/{date}/{sensorType}");
                    }
                    break;
                case 2:
                    if (string.IsNullOrEmpty(sensorType))
                    {
                        responseMessage = await client.GetAsync($"/api/v1/getdatafordevice?deviceId={deviceId}&date={date}");
                    }
                    else
                    {
                        responseMessage = await client.GetAsync($"/api/v1/getdata?deviceId={deviceId}&date={date}&sensorType={sensorType}");
                    }
                    break;
            }


            Assert.True(responseMessage.IsSuccessStatusCode);
            Assert.Equal(HttpStatusCode.OK, responseMessage.StatusCode);

            var content = await ReadContentAsync<WeatherResponseDTO>(responseMessage);

            Assert.NotNull(content);
            Assert.DoesNotContain(content.Notifications, x => x.Type == NotificationTypeEnum.Error);
            Assert.NotNull(content.Data);
            Assert.NotNull(content.Data.FileName);
            Assert.Equal($"{date}.csv", content.Data.FileName);

            if (!string.IsNullOrEmpty(sensorType))
            {
                switch (sensorType)
                {
                    case "temperature":
                        Assert.NotEmpty(content.Data.TemperatureList);
                        Assert.Null(content.Data.HumidityList);
                        Assert.Null(content.Data.RainfallList);
                        break;
                    case "humidity":
                        Assert.NotEmpty(content.Data.HumidityList);
                        Assert.Null(content.Data.TemperatureList);
                        Assert.Null(content.Data.RainfallList);
                        break;
                    case "rainfall":
                        Assert.NotEmpty(content.Data.RainfallList);
                        Assert.Null(content.Data.TemperatureList);
                        Assert.Null(content.Data.HumidityList);
                        break;
                }
            }
            else
            {
                Assert.NotEmpty(content.Data.HumidityList);
                Assert.NotEmpty(content.Data.TemperatureList);
                Assert.NotEmpty(content.Data.RainfallList);
            }
        }

        private async Task<CustomResponse<T>> ReadContentAsync<T>(HttpResponseMessage responseMessage)
        {
            var jsonResult = await responseMessage.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<CustomResponse<T>>(jsonResult);
        }
    }
}
