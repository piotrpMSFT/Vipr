using System.Linq;
using Microsoft.Its.Recipes;
using Microsoft.OData.ProxyExtensions;
using Moq;
using ODataV4TestService.SelfHost;
using Vipr.Core;
using Xunit;

namespace CSharpWriterUnitTests
{
    public class Given_an_OdcmClass_Entity_Navigation_Property_Key_forced_to_pascal_case : EntityTestBase
    {
        private IStartedScenario _mockedService;
        private string _camelCasedName;
        private readonly string _pascalCasedName;

        public Given_an_OdcmClass_Entity_Navigation_Property_Key_forced_to_pascal_case()
        {
            var configurationProviderMock = new Mock<IConfigurationProvider>();
                configurationProviderMock.Setup(c => c.ForcePropertyPascalCasing).Returns(true);
                ConfigurationProvider = configurationProviderMock.Object;

                Init(m =>
                {
                    var property = Class.Key.RandomElement();

                    _camelCasedName = Any.Char('a', 'z') + property.Name;

                    property.Rename(_camelCasedName);
                });

            _pascalCasedName = _camelCasedName.ToPascalCase();
        }

        [Fact(Skip = "https://github.com/Microsoft/Vipr/issues/26")]
        public void When_retrieved_through_Collection_GetById_method_then_request_is_sent_to_server_with_original_key_parameter_name()
        {
            var entitySetPath = Any.UriPath(1);
            var keyValues = Class.GetSampleKeyArguments().ToArray();
            var keyPredicate = ODataKeyPredicate.AsString(keyValues);
            var entityPath = string.Format("/{0}({1})", entitySetPath, keyPredicate);

            using (_mockedService = new MockScenario()
                    .SetupGetEntity(entityPath, Class.Name + "s", ConcreteType.Initialize(keyValues))
                    .Start())
            {
                var collection = _mockedService
                    .GetContext()
                    .UseJson(Model.ToEdmx(), true)
                    .CreateCollection(CollectionType, ConcreteType, entitySetPath);

                var fetcher = collection.InvokeMethod<RestShallowObjectFetcher>("GetById",
                    keyValues.Select(k => k.Item2).ToArray());

                fetcher.ExecuteAsync().Wait();
            }
        }

        [Fact(Skip = "https://github.com/Microsoft/Vipr/issues/26")]
        public void When_retrieved_through_Collection_GetById_indexer_then_request_is_sent_to_server_with_original_key_parameter_name()
        {
            var entitySetPath = Any.UriPath(1);
            var keyValues = Class.GetSampleKeyArguments().ToArray();
            var keyPredicate = ODataKeyPredicate.AsString(keyValues);
            var entityPath = string.Format("/{0}({1})", entitySetPath, keyPredicate);

            using (_mockedService = new MockScenario()
                    .SetupGetEntity(entityPath, Class.Name + "s", ConcreteType.Initialize(keyValues))
                    .Start())
            {
                var collection = _mockedService
                    .GetContext()
                    .UseJson(Model.ToEdmx(), true)
                    .CreateCollection(CollectionType, ConcreteType, entitySetPath);

                var fetcher =
                    collection.GetIndexerValue<RestShallowObjectFetcher>(keyValues.Select(k => k.Item2).ToArray());

                fetcher.ExecuteAsync().Wait();
            }
        }
    }
}