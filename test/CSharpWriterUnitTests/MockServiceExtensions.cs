﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Its.Recipes;
using Microsoft.MockService;
using Microsoft.MockService.Extensions.ODataV4;
using Microsoft.OData.Client;
using Microsoft.OData.ProxyExtensions;
using Vipr.Core.CodeModel;

namespace CSharpWriterUnitTests
{
    public static class MockServiceExtensions
    {
        public static DataServiceContextWrapper GetContext(this MockService serviceMock,
            Func<Task<string>> tokenGetterFunction = null)
        {
            tokenGetterFunction = tokenGetterFunction ?? Any.TokenGetterFunction();

            return new DataServiceContextWrapper(new Uri(serviceMock.GetBaseAddress()), ODataProtocolVersion.V4,
                tokenGetterFunction);
        }

        public static DataServiceContextWrapper GetDefaultContext(this MockService serviceMock,
            OdcmModel model)
        {
            return serviceMock
                .GetContext()
                .UseJson(model.ToEdmx(), true);
        }

        public static object CreateContainer(this MockService serviceMock, Type containerType,
            Func<Task<string>> tokenGetterFunction = null)
        {
            tokenGetterFunction = tokenGetterFunction ?? Any.TokenGetterFunction();

            return Activator.CreateInstance(containerType,
                new object[] {new Uri(serviceMock.GetBaseAddress()), tokenGetterFunction});
        }

        public static MockService SetupPostEntityPropertyChanges(this MockService mockService, EntityArtifacts targetEntity, IEnumerable<Tuple<string, object>> keyValues, OdcmProperty property)
        {
            return mockService.SetupPostEntityChanges(targetEntity.Class.GetDefaultEntityPropertyPath(property, keyValues));
        }

        public static MockService SetupGetEntityProperty(this MockService mockService, EntityArtifacts targetEntity, IEnumerable<Tuple<string, object>> keyValues, OdcmProperty property)
        {
            return mockService.SetupGetEntity(
                targetEntity.Class.GetDefaultEntityPropertyPath(property, keyValues),
                targetEntity.Class.GetDefaultEntitySetName(),
                targetEntity.ConcreteType.Initialize(targetEntity.Class.GetSampleKeyArguments()));
        }

        public static MockService SetupPostEntity(this MockService mockService, EntityArtifacts targetEntity, IEnumerable<Tuple<string, object>> propertyValues = null)
        {
            propertyValues = propertyValues ?? targetEntity.Class.GetSampleKeyArguments();

            return mockService.SetupPostEntity(
                targetEntity.Class.GetDefaultEntitySetPath(), 
                targetEntity.Class.GetDefaultEntitySetName(),
                targetEntity.ConcreteType.Initialize(propertyValues));
        }

        public static MockService SetupGetEntity(this MockService mockService, EntityArtifacts targetEntity, 
            IEnumerable<Tuple<string, object>> keyValues = null, IEnumerable<string> expandTargets = null)
        {
            keyValues = keyValues ?? targetEntity.Class.GetSampleKeyArguments();

            keyValues = keyValues.ToList();

            return mockService.SetupGetEntity(targetEntity.Class.GetDefaultEntityPath(keyValues),
                targetEntity.Class.GetDefaultEntitySetName(), targetEntity.ConcreteType.Initialize(keyValues),
                expandTargets);
        }

        public static MockService SetupGetEntitySet(this MockService mockService, EntityArtifacts targetEntity,
            IEnumerable<string> expandTargets = null)
        {
            return mockService.SetupGetEntity(targetEntity.Class.GetDefaultEntitySetPath(), targetEntity.Class.GetDefaultEntitySetName(),
                targetEntity.ConcreteType.Initialize(targetEntity.Class.GetSampleKeyArguments()), expandTargets);
        }
    }
}
