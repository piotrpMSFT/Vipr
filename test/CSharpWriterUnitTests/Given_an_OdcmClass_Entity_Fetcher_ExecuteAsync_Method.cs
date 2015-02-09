﻿using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Its.Recipes;
using Microsoft.OData.ProxyExtensions;
using Moq;
using ODataV4TestService.SelfHost;
using Vipr.Core;
using Vipr.Core.CodeModel;
using Xunit;

namespace CSharpWriterUnitTests
{
    /// <summary>
    /// Summary description for Given_an_OdcmModel
    /// </summary>
    public class Given_an_OdcmClass_Entity_Fetcher_ExecuteAsync_Method : EntityTestBase
    {
        private IStartedScenario _mockedService;

        public Given_an_OdcmClass_Entity_Fetcher_ExecuteAsync_Method()
        {
            Init();
        }

        [Fact]
        public void It_retrieves_a_value_from_its_own_path()
        {
            var instanceName = Any.UriPath(1);

            var entityPath = "/" + instanceName;

            var keyValues = Class.GetSampleKeyArguments().ToArray();

            using (_mockedService = new MockScenario()
                    .SetupGetEntity(entityPath, Class.Name + "s", ConcreteType.Initialize(keyValues))
                    .Start())
            {
                var fetcher = _mockedService
                    .GetContext()
                    .UseJson(Model.ToEdmx(), true)
                    .CreateFetcher(FetcherType, instanceName);

                var result = fetcher.InvokeMethod<Task>("ExecuteAsync").GetPropertyValue<EntityBase>("Result");

                result.ValidatePropertyValues(keyValues);
            }
        }
    }
}
