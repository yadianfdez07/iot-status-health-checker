using Amazon.Lambda.Core;
using Newtonsoft.Json;
using System.Net.Http;
using System.Threading.Tasks;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace HealthCheckerBuilder
{
    public class Function
    {

        /// <summary>
        /// A simple function that takes a string and does a ToUpper
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task FunctionHandler(CloudFormationRequest<IotThingConfig> input, ILambdaContext context)
        {
            LambdaLogger.Log("ENVIRONMENT VARIABLES: " + JsonConvert.SerializeObject(System.Environment.GetEnvironmentVariables()));
            LambdaLogger.Log("CONTEXT: " + JsonConvert.SerializeObject(context));
            LambdaLogger.Log("INPUT: " + JsonConvert.SerializeObject(input));
            var policy = "{\"Version\":\"2012-10-17\",\"Statement\":[{\"Effect\":\"Allow\",\"Action\":[\"iot:Connect\"],\"Resource\":\"*\"},{\"Effect\":\"Allow\",\"Action\":[\"*\"],\"Resource\":[\"*\"]},{\"Effect\":\"Allow\",\"Action\":[\"iot:Receive\"],\"Resource\":[\"*\"]},{\"Effect\":\"Allow\",\"Action\":[\"iot:Subscribe\"],\"Resource\":[\"*\"]}]}";

            if (input.RequestType.ToLower().Equals("create"))
            {
                var thingCreator = new ThingCreator();

                var thingResponse = thingCreator.CreateThing(input.ResourceProperties.REGION, input.ResourceProperties.THING_NAME);

                var certificateResponse = thingCreator.CreateCertificate(input.ResourceProperties.REGION, input.ResourceProperties.THING_NAME, policy, thingResponse);

                var data = new
                {
                    CertificateArn = certificateResponse.CertificateArn,
                    CertificatePem = certificateResponse.CertificatePem,
                    CertificatePrivateKey = certificateResponse.KeyPair.PrivateKey,
                    CertificatePublicKey = certificateResponse.KeyPair.PublicKey
                };

                var response = new CloudFormationResponse
                {
                    Status = "SUCCESS",
                    Reason = "Is a create request",
                    PhysicalResourceId = $"{input.ResourceProperties.THING_NAME}",
                    StackId = input.StackId,
                    RequestId = input.RequestId,
                    LogicalResourceId = input.LogicalResourceId,
                    Data = data
                };

                var jsonContent = new StringContent(JsonConvert.SerializeObject(response));

                LambdaLogger.Log("RESPONSE: " + JsonConvert.SerializeObject(response));

                jsonContent.Headers.Remove("Content-Type");

                using (var client = new HttpClient())
                {
                    var postResponse = await client.PutAsync(input.ResponseURL, jsonContent);
                    postResponse.EnsureSuccessStatusCode();
                }

            }
            else
            {
                var response = new CloudFormationResponse
                {
                    Status = "SUCCESS",
                    Reason = "Is not a create request",
                    PhysicalResourceId = $"{input.ResourceProperties.THING_NAME}",
                    StackId = input.StackId,
                    RequestId = input.RequestId,
                    LogicalResourceId = input.LogicalResourceId,
                };

                var jsonContent = new StringContent(JsonConvert.SerializeObject(response));

                LambdaLogger.Log("RESPONSE: " + JsonConvert.SerializeObject(response));

                jsonContent.Headers.Remove("Content-Type");

                using (var client = new HttpClient())
                {
                    var postResponse = await client.PutAsync(input.ResponseURL, jsonContent);
                    postResponse.EnsureSuccessStatusCode();
                }

            }
        }
    }

    public class CloudFormationResponse
    {
        public string Status { get; set; }
        public string Reason { get; set; }
        public string PhysicalResourceId { get; set; }
        public string StackId { get; set; }
        public string RequestId { get; set; }
        public string LogicalResourceId { get; set; }
        public object Data { get; set; }
    }
}