using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Text;

namespace WebFunctions
{
    public static class Function1
    {
        public const string secretKey = "6FE5E4FD341E4BE581C6C700BFE771BC";
        
        [FunctionName("Function1")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

            if(string.IsNullOrEmpty(req.Headers["X-Hub-Signature-256"]))
            {
                var isSignValid = VerifySignature(secretKey, requestBody, req.Headers["X-Hub-Signature-256"]);
                 log.LogInformation("Signature verified successfully");
                return new OkObjectResult(isSignValid);
            }
            else
            {
                  log.LogInformation("Signature verification failed");
                return new OkObjectResult("Header is null");
            }
                                              
        }

        static bool VerifySignature(string secret, string requestBody, string xHubSignature)
        {
            using (var hmac = new HMACSHA256(Encoding.ASCII.GetBytes(secret)))
            {
                byte[] signatureBytes = hmac.ComputeHash(Encoding.ASCII.GetBytes(requestBody));
                string calculatedSignature = "sha256=" + BitConverter.ToString(signatureBytes).Replace("-", "").ToLower();

                // Compare the calculated signature with the provided X-Hub-Signature
                return string.Equals(calculatedSignature, xHubSignature, StringComparison.OrdinalIgnoreCase);
            }
        }
    }
}
