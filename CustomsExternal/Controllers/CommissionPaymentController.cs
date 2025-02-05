using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web.Http;

namespace CustomsExternal.Controllers
{
    public class CommissionPaymentController : ApiController
    {

    //    public async Task<IHttpActionResult> CaptureFrame(string transactionId)
    //    {
    //        var client = new HttpClient();
    //        var url = "https://secure.tranzila.com/api/v1/capture";

    //        var requestData = new
    //        {
    //            transaction_id = transactionId,
    //            amount = 200
    //        };

    //        var json = JsonSerializer.Serialize(requestData);
    //        var content = new StringContent(json, Encoding.UTF8, "application/json");

    //        var response = await client.PostAsync(url, content);

    //        if (response.IsSuccessStatusCode)
    //        {
    //            return Ok("Frame captured successfully");
    //        }
    //        else
    //        {
    //            return BadRequest("Failed to capture frame");
    //        }

    //    }
    //    // POST: api/CommissionPayment
    //    [HttpPost]
    //    [Route("authorize")]
    //    public async Task<IHttpActionResult> AuthorizePayment(PaymentRequest request)
    //    {
    //        string tranzilaUrl = "https://secure5.tranzila.com/cgi-bin/tranzila.cgi";
    //        string suplierId = "0962360";

    //        var postData = new StringContent(
    //            $"supplier={suplierId}&sum=200&tranmode=A&ccno={request.CardNumber}&expdate={request.ExpiryDate}&mycvv={request.CVV}",
    //            Encoding.UTF8,
    //            "application/x-www-form-urlencoded"
    //        );

    //        using (var httpClient = new HttpClient())
    //        {
    //            var response = await httpClient.PostAsync(tranzilaUrl, postData);

    //            if (response.IsSuccessStatusCode)
    //            {
    //                var responseBody = await response.Content.ReadAsStringAsync();
    //                return Ok(new { message = "Authorization hold successful", data = responseBody });
    //            }

    //            return BadRequest("Failed to authorize payment" + response.ReasonPhrase);
    //        }
    //    }

    //    [HttpPost]
    //    public IHttpActionResult HandleWebhook(TranzilaResponse response)
    //    {
    //        if (response != null)
    //        {
    //            Console.WriteLine($"Transaction Status: {response.Status}");
    //            Console.WriteLine($"Token: {response.Token}");

    //        }
    //        return Ok(); 
    //    }

    //    public class TranzilaResponse
    //    {
    //        public string Status { get; set; }
    //        public string Token { get; set; }
    //        public string TransactionId { get; set; }
    //    }

    //    public class PaymentRequest
    //    {
    //        public string CardNumber { get; set; }
    //        public string ExpiryDate { get; set; }
    //        public string CVV { get; set; }
    //    }
    }
}
