using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Net;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using Microsoft.Playwright;
using System;
using System.Threading.Tasks;
using System;
using WebDriverManager.DriverConfigs.Impl;
using OpenQA.Selenium.Support.UI;

namespace CustomsExternal.Controllers
{
    public class ProxyController : ApiController
    {

        private readonly HttpClient _httpClient;

        public ProxyController()
        {
            _httpClient = new HttpClient();
        }
        public class FormData
        {
            public string Asmachta { get; set; }
            public string ImporterNum { get; set; }
        }

        private static IBrowser BrowserInstance;
        private static IPlaywright PlaywrightInstance;

        [HttpPost]
        [Route("api/fill-form")]
        public async Task FillFormAsync()
        {
            //var playwright = await Playwright.CreateAsync();
            //IBrowser browser = null;
            if (PlaywrightInstance == null)
            {
                PlaywrightInstance = await Playwright.CreateAsync();
                BrowserInstance = await PlaywrightInstance.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = false });
            }
            try
            {

                //browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = false });
                //var page = await browser.NewPageAsync();
                var page = await BrowserInstance.NewPageAsync();

                await page.GotoAsync("https://ecom.gov.il/voucherspa/input/380?clear=true");

                await page.FillAsync("#k_asmachta", "77765");
                await page.FillAsync("#k_importer_num", "025025040");
                await page.WaitForNavigationAsync();

                //await Task.Delay(5000); 
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            finally
            {
                //await browser.CloseAsync();
            }
        }




        //public void FillForm(FormData data)
        //{
        //    try
        //    {
        //        string driverPath = @"C:\Users\Admin\Downloads\chromedriver-win64\chromedriver-win64\chromedriver.exe";
        //        var options = new ChromeOptions();
        //        //options.AddArguments("headless");

        //        IWebDriver driver = new ChromeDriver(driverPath, options);

        //        driver.Navigate().GoToUrl("https://ecom.gov.il/voucherspa/input/380?clear=true");

        //        //var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

        //        var jsExecutor = (IJavaScriptExecutor)driver;

        //        jsExecutor.ExecuteScript("document.getElementById('k_asmachta').value = '77765';");
        //        jsExecutor.ExecuteScript("document.getElementById('k_asmachta').dispatchEvent(new Event('input'));");

        //        jsExecutor.ExecuteScript("document.getElementById('k_importer_num').value = '025025040';");
        //        jsExecutor.ExecuteScript("document.getElementById('k_importer_num').dispatchEvent(new Event('input'));");

        //        //driver.Quit();
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"Error: {ex.Message}");
        //    }
        //}


        //[HttpGet]
        //[Route("api/Proxy")]
        //public async Task<HttpResponseMessage> Proxy(string url)
        //{
        //    url = url.Trim('"');
        //    if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) ||
        //        !uri.Host.Contains("ecom.gov.il"))
        //    {
        //        return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Invalid URL");
        //    }

        //    // שליחת הבקשה לכתובת שנשלחה
        //    var response = await _httpClient.GetAsync(url);
        //    if (!response.IsSuccessStatusCode)
        //    {
        //        return Request.CreateResponse(response.StatusCode, "Error fetching content");
        //    }

        //    // קריאת התוכן של התגובה
        //    var content = await response.Content.ReadAsStringAsync();


        //    var httpResponse = Request.CreateResponse(response.StatusCode, content);
        //    httpResponse.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("text/html");

        //    return httpResponse;
        //    //var jsonResponse = new { content = content };
        //    //return Request.CreateResponse(response.StatusCode, jsonResponse);
        //}

    }
}