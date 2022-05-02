using Amazon.Lambda.Core;
using Newtonsoft.Json;
using System;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace HealthStatusChecker
{
    public class Function
    {

        /// <summary>
        /// A simple function that takes a string and does a ToUpper
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public Output FunctionHandler(Input input, ILambdaContext context)
        {
            LambdaLogger.Log("ENVIRONMENT VARIABLES: " + JsonConvert.SerializeObject(System.Environment.GetEnvironmentVariables()));
            LambdaLogger.Log("CONTEXT: " + JsonConvert.SerializeObject(context));
            LambdaLogger.Log("INPUT: " + JsonConvert.SerializeObject(input));

            var response = new Output { Message = "Hello World!!!", DateTime = DateTime.Now, Identifier = Guid.NewGuid() };

            return response;
        }
    }

    public class Input
    {
        public int Id { get; set; }

        public string Description { get; set; }
    }

    public class Output
    {
        public string Message { get; set; }
        public DateTime DateTime { get; set; }
        public Guid Identifier { get; set; }
    }
}