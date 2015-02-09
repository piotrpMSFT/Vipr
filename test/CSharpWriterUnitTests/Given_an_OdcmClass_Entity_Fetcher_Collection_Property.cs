﻿using System;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
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
    public class Given_an_OdcmClass_Entity_Fetcher_Collection_Property : EntityTestBase
    {
        private IStartedScenario _mockedService;
        private object propertyValue;

        public Given_an_OdcmClass_Entity_Fetcher_Collection_Property()
        {
            base.Init();
        }

        [Fact]
        public void The_Collection_path_is_the_fetcher_path_plus_property_name()
        {
            var collectionProperty =
                Model.GetProperties()
                    .Where(p => p.Class.Kind == OdcmClassKind.Entity)
                    .Where(p => p.IsCollection)
                    .RandomElement();

            var entityPath = Any.UriPath(1);

            var propertyPath = "/" + entityPath + "/" + collectionProperty.Name;

            using (_mockedService = new MockScenario()
                    .SetupGetWithEmptyResponse(propertyPath)
                    .Start())
            {
                var fetcher = _mockedService
                    .GetContext()
                    .UseJson(Model.ToEdmx(), true)
                    .CreateFetcher(Proxy.GetClass(collectionProperty.Class.Namespace, collectionProperty.Class.Name + "Fetcher"), entityPath);

                var propertyValue = fetcher.GetPropertyValue(collectionProperty.Name);

                propertyValue.InvokeMethod<Task>("ExecuteAsync").Wait();
            }
        }

        [Fact]
        public void Its_value_is_cached_and_reused_between_requests()
        {
            var collectionProperty =
                Model.GetProperties()
                    .Where(p => p.Class.Kind == OdcmClassKind.Entity)
                    .Where(p => p.IsCollection)
                    .RandomElement();

            var fetcher = DataServiceContextWrapperExtensions.CreateFetcher(null,
                Proxy.GetClass(collectionProperty.Class.Namespace, collectionProperty.Class.Name + "Fetcher"),
                collectionProperty.Name);

            fetcher.GetPropertyValue(collectionProperty.Name)
                .Should().Be(fetcher.GetPropertyValue(collectionProperty.Name), "Because the value should be cached.");
        }
    }
}
