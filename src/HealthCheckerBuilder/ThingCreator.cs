using Amazon.IoT;
using Amazon.IoT.Model;

namespace HealthCheckerBuilder
{
    public class ThingCreator
    {
        public CreateThingResponse CreateThing(string region, string thingName)
        {
            IAmazonIoT amazonIoT = new AmazonIoTClient(Amazon.RegionEndpoint.GetBySystemName(region));

            DescribeEndpointResponse endpoint = amazonIoT.DescribeEndpointAsync().Result;


            var createThingResponse = amazonIoT.CreateThingAsync(new CreateThingRequest() { ThingName = thingName }).Result;

            return createThingResponse;
        }

        public CreateKeysAndCertificateResponse CreateCertificate(string region, string identifier, string awsIoTPolicy, CreateThingResponse createThingResponse)
        {
            IAmazonIoT amazonIoT = new AmazonIoTClient(Amazon.RegionEndpoint.GetBySystemName(region));
            var createKeysAndCertificateResponse =
                amazonIoT.CreateKeysAndCertificateAsync(new CreateKeysAndCertificateRequest() { SetAsActive = true }).Result;

            AttachCertificate(amazonIoT, createThingResponse, createKeysAndCertificateResponse);
            AttachPolicy(amazonIoT, identifier, awsIoTPolicy, createKeysAndCertificateResponse);

            return createKeysAndCertificateResponse;
        }

        private void AttachCertificate(IAmazonIoT amazonIoT, CreateThingResponse createThingResponse, CreateKeysAndCertificateResponse createKeysAndCertificateResponse)
        {
            var attachThingPrincipalResponse = amazonIoT.AttachThingPrincipalAsync(new AttachThingPrincipalRequest() { ThingName = createThingResponse.ThingName, Principal = createKeysAndCertificateResponse.CertificateArn }).Result;
        }

        private void AttachPolicy(IAmazonIoT amazonIoT, string identifier, string awsIoTPolicy, CreateKeysAndCertificateResponse createKeysAndCertificateResponse)
        {
            //amazonIoT.AttachPolicyAsync(new AttachPolicyRequest
            //{
            //	PolicyName = "",
            //	Target = createKeysAndCertificateResponse.CertificateArn
            //});
            var attachThingPrincipalResponse =
                amazonIoT.AttachPrincipalPolicyAsync(new AttachPrincipalPolicyRequest()
                {
                    Principal = createKeysAndCertificateResponse.CertificateArn,
                    PolicyName = CreateIoTPolicy(amazonIoT, identifier, awsIoTPolicy).PolicyName
                }).Result;

        }

        private CreatePolicyResponse CreateIoTPolicy(IAmazonIoT amazonIoT, string identifier, string awsIoTPolicy)
        {
            var createPolicyResponse = amazonIoT.CreatePolicyAsync(new CreatePolicyRequest() { PolicyName = "SDDS_IoT_Policy_" + identifier, PolicyDocument = awsIoTPolicy });

            return createPolicyResponse.Result;
        }

    }
}