using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Its.Recipes;
using Microsoft.OData.Client;
using Microsoft.OData.ProxyExtensions;
using Microsoft.Owin;
using Moq;
using Newtonsoft.Json.Linq;
using ODataV4TestService.SelfHost;
using Vipr.Core.CodeModel;

namespace CSharpWriterUnitTests
{
    public static class MockScenarioExtensions
    {
        public static DataServiceContextWrapper GetContext(this MockScenario serviceMock,
            Func<Task<string>> tokenGetterFunction = null)
        {
            tokenGetterFunction = tokenGetterFunction ?? Any.TokenGetterFunction();

            return new DataServiceContextWrapper(new Uri(serviceMock.GetBaseAddress()), ODataProtocolVersion.V4,
                tokenGetterFunction);
        }
        public static DataServiceContextWrapper GetDefaultContext(this MockScenario serviceMock,
            OdcmModel model)
        {
            return serviceMock
                .GetContext()
                .UseJson(model.ToEdmx(), true);
        }

        public static object CreateContainer(this IStartedScenario serviceMock, Type containerType,
            Func<Task<string>> tokenGetterFunction = null)
        {
            tokenGetterFunction = tokenGetterFunction ?? Any.TokenGetterFunction();

            return Activator.CreateInstance(containerType,
                new object[] {new Uri(serviceMock.GetBaseAddress()), tokenGetterFunction});
        }

        public static MockScenario SetupPostEntity(this MockScenario mockService, string entitySetPath,
            string entitySetName, object response = null)
        {
            mockService
                .Setup(c => c.Request.Method == "POST" &&
                            c.Request.Path.Value == entitySetPath,
                    (b, c) =>
                    {
                        c.Response.StatusCode = 201;
                        c.Response.WithDefaultODataHeaders();
                        c.Response.SetResponseBody(mockService.GetBaseAddress(), entitySetName, response);
                    });

            return mockService;
        }

        public static MockScenario SetupPostEntityChanges(this MockScenario mockService, string entitySetPath)
        {
            mockService
                .Setup(c => c.Request.Method == "POST" &&
                            c.Request.Path.Value == entitySetPath,
                    (b, c) =>
                    {
                        c.Response.StatusCode = 200;
                        c.Response.WithDefaultODataHeaders();
                    });

            return mockService;
        }

        public static MockScenario SetupPatchEntityChanges(this MockScenario mockService, string entitySetPath)
        {
            mockService
                .Setup(c => c.Request.Method == "PATCH" &&
                            c.Request.Path.Value == entitySetPath,
                    (b, c) =>
                    {
                        c.Response.StatusCode = 200;
                        c.Response.WithDefaultODataHeaders();
                    });

            return mockService;
        }

        public static MockScenario SetupPostEntity(this MockScenario mockService, OdcmClass @class, Type type)
        {
            return mockService.SetupPostEntity(@class.DefaultEntitySetPath(), @class.DefaultEntitySetName(),
                type.Initialize(@class.GetSampleKeyArguments()));
        }

        public static MockScenario SetupPostEntity(this MockScenario mockService, OdcmClass @class, object response)
        {
            return mockService.SetupPostEntity(@class.DefaultEntitySetPath(), @class.DefaultEntitySetName(),
                response);
        }

        public static MockScenario SetupGetEntity(this MockScenario mockService, OdcmClass @class, Type type,
            IEnumerable<string> expandTargets = null)
        {
            return mockService.SetupGetEntity(@class.DefaultEntitySetPath(), @class.DefaultEntitySetName(),
                type.Initialize(@class.GetSampleKeyArguments()), expandTargets);
        }

        public static MockScenario SetupGetEntity(this MockScenario mockService, string entitySetPath,
            string entitySetName, object response, IEnumerable<string> expandTargets = null)
        {
            var jObject = response.ToJObject();

            jObject.AddOdataContext(mockService.GetBaseAddress(), entitySetName);

            mockService
                .Setup(c => c.Request.Method == "GET" &&
                            c.Request.Path.Value == entitySetPath &&
                            expandTargets == null || c.Request.Query["$expand"] == string.Join(",", expandTargets),
                    (b, c) =>
                    {
                        c.Response.StatusCode = 200;
                        c.Response.WithDefaultODataHeaders();
                        c.Response.Write(jObject.ToString());
                    });

            return mockService;
        }

        public static MockScenario SetupGetWithEmptyResponse(this MockScenario mockService, string entityPropertyPath)
        {
            mockService
                .Setup(c => c.Request.Method == "GET" &&
                            c.Request.Path.Value == entityPropertyPath,
                    (b, c) =>
                    {
                        c.Response.StatusCode = 200;
                    });

            return mockService;
        }

        // This method is not yet working
        // It needs to be able to set up odata context and related properties
        public static MockScenario SetupGetEntityProperty(this MockScenario mockService, string entityPropertyPath,
            string entitySetName, string entityKeyPredicate, string propertyName, object propertyValue)
        {
            var jObject = new JObject();

            if (propertyValue != null)
            {
                jObject.AddOdataContext(mockService.GetBaseAddress(), entitySetName, entityKeyPredicate, propertyName);
                jObject.Add(new JProperty("value", SerializePropertyValue(propertyValue)));
            }

            mockService
                .Setup(c => c.Request.Method == "GET" &&
                            c.Request.Path.Value == entityPropertyPath,
                    (b, c) =>
                    {
                        c.Response.StatusCode = 200;
                        c.Response.WithDefaultODataHeaders();
                        if(propertyValue != null)
                            c.Response.Write(jObject.ToString());
                    });

            return mockService;
        }

        private static string SerializePropertyValue(object propertyValue)
        {
            if (propertyValue is string)
                return propertyValue as string;

            return JObject.FromObject(propertyValue).ToString();
        }

        private static void SetResponseBody(this IOwinResponse owinResponse, string baseAddres, string entitySetName, object response)
        {
            if (response == null)
                return;

            var jObject = response.ToJObject();

            jObject.AddOdataContext(baseAddres, entitySetName);

            owinResponse.Write(jObject.ToString());
        }
    }
}
