﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Linq;
using Microsoft.Its.Recipes;
using Microsoft.OData.ProxyExtensions;
using ODataV4TestService.SelfHost;
using Xunit;

namespace CSharpWriterUnitTests
{
    public class Given_an_OdcmClass_Entity_Collection_GetById_Indexer : EntityTestBase
    {
        private IStartedScenario _serviceMock;

        public Given_an_OdcmClass_Entity_Collection_GetById_Indexer()
        {
            Init();
        }

        [Fact]
        public void When_the_indexer_is_called_it_GETs_the_collection_by_name_and_passes_the_id_in_the_path()
        {
            var entitySetPath = Any.UriPath(1);
            var keyValues = Class.GetSampleKeyArguments().ToArray();
            var keyPredicate = ODataKeyPredicate.AsString(keyValues);
            var entityPath = string.Format("/{0}({1})", entitySetPath, keyPredicate);

            using (_serviceMock = new MockScenario()
                    .SetupGetEntity(entityPath, Class.Name + "s", ConcreteType.Initialize(Class.GetSampleKeyArguments()))
                    .Start())
            {
                var collection = _serviceMock.GetContext()
                    .UseJson(Model.ToEdmx(), true)
                    .CreateCollection(CollectionType, ConcreteType, entitySetPath);

                var fetcher = collection.GetIndexerValue<RestShallowObjectFetcher>(keyValues.Select(k => k.Item2).ToArray());

                var task = fetcher.ExecuteAsync();
                task.Wait();
            }
        }
    }
}
