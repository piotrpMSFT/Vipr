﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using Vipr.Core.CodeModel;

namespace Vipr.Writer.CSharp
{
    internal static class NamesService
    {
        private const string DataServiceKey = "global::Microsoft.OData.Client.Key";
        private const string EntitySet = "global::Microsoft.OData.Client.EntitySet";
        private const string ProtocolVersion = "global::Microsoft.OData.Client.ODataProtocolVersion";
        private const string CurrentProtocolVersion = "global::Microsoft.OData.Client.ODataProtocolVersion.V4";

        public const String ExtensionNamespace = "Microsoft.OData.ProxyExtensions";

        public static Identifier GetPublicTypeName(OdcmType odcmType)
        {
            return GetPublicTypeName(odcmType, false);
        }

        public static Identifier GetPublicTypeName(OdcmType odcmType, bool isCollection)
        {
            if (odcmType == null)
            {
                return new Identifier(string.Empty, "void");
            }

            if (odcmType.Namespace == OdcmNamespace.Edm)
            {
                return GetPrimitiveTypeName(odcmType);
            }

            if (odcmType is OdcmClass && ((OdcmClass)odcmType).Kind == OdcmClassKind.Entity)
            {
                return GetConcreteInterfaceName(odcmType);
            }

            if (odcmType is OdcmClass && ((OdcmClass)odcmType).Kind == OdcmClassKind.MediaEntity)
            {
                return GetConcreteInterfaceName(odcmType);
            }

            var resolvedName = ResolveIdentifier(odcmType);

            return new Identifier(resolvedName.Namespace, resolvedName.Name + (isCollection ? "[]" : string.Empty));
        }

        public static Identifier GetContainerName(OdcmClass odcmContainer)
        {
            return ResolveIdentifier(odcmContainer);
        }

        public static Identifier GetConcreteTypeName(OdcmType odcmType)
        {
            if (odcmType is OdcmPrimitiveType)
                return GetPrimitiveTypeName(odcmType);

            return ResolveIdentifier(odcmType);
        }

        public static Identifier GetPrimitiveTypeName(OdcmType odcmType)
        {
            return GetPrimitiveTypeName(odcmType.Name);
        }

        public static Identifier GetPrimitiveTypeName(string odcmTypeName)
        {
            switch (odcmTypeName)
            {
                case "Binary": return new Identifier("System", "Byte[]");
                case "Boolean": return new Identifier("System", "Boolean");
                case "Byte": return new Identifier("System", "Byte");
                case "Date": return new Identifier("System", "DateTimeOffset");
                case "DateTimeOffset": return new Identifier("System", "DateTimeOffset");
                case "Decimal": return new Identifier("System", "Decimal");
                case "Double": return new Identifier("System", "Double");
                case "Duration": return new Identifier("System", "TimeSpan");
                case "Guid": return new Identifier("System", "Guid");
                case "Int16": return new Identifier("System", "Int16");
                case "Int32": return new Identifier("System", "Int32");
                case "Int64": return new Identifier("System", "Int64");
                case "SByte": return new Identifier("System", "SByte");
                case "Single": return new Identifier("System", "Single");
                case "Stream": return new Identifier("Microsoft.OData.Client", "DataServiceStreamLink");
                case "String": return new Identifier("System", "String");
                case "TimeOfDay": return new Identifier("System", "DateTimeOffset");
                case "IStream":return new Identifier("System.IO", "Stream");
            }

            return null;
        }

        public static string GetPrimitiveTypeKeyword(OdcmPrimitiveType odcmType)
        {
            switch (odcmType.Name)
            {
                case "Byte": return "byte";
                case "SByte": return "sbyte";
                case "Int16": return "short";
                case "UInt16": return "ushort";
                case "Int32": return "int";
                case "UInt32": return "uint";
                case "Int64": return "long";
                case "UInt64": return "ulong";
            }

            return null;
        }

        public static Identifier GetEnumTypeName(OdcmEnum odcmEnum)
        {
            return ResolveIdentifier(odcmEnum);
        }

        public static Identifier GetConcreteInterfaceName(OdcmType odcmType)
        {
            var resolvedName = ResolveIdentifier(odcmType);

            return new Identifier(resolvedName.Namespace, "I" + resolvedName.Name);
        }

        public static string GetPropertyFieldName(OdcmProperty odcmProperty)
        {
            return "_" + odcmProperty.Name;
        }

        public static Identifier GetExtensionTypeName(string extensionTypeName)
        {
            return new Identifier(ExtensionNamespace, extensionTypeName);
        }

        public static Identifier GetFetcherTypeName(OdcmType odcmType)
        {
            var resolvedName = ResolveIdentifier(odcmType);

            return new Identifier(resolvedName.Namespace, resolvedName.Name + "Fetcher");
        }

        public static Identifier GetFetcherInterfaceName(OdcmType odcmType)
        {
            var fetcherTypeName = GetFetcherTypeName(odcmType);
            return GetConcreteInterfaceName(fetcherTypeName);
        }

        private static Identifier GetConcreteInterfaceName(Identifier typeName)
        {
            return new Identifier(typeName.Namespace, "I" + typeName.Name);
        }

        internal static string GetFetcherCollectionFieldName(OdcmProperty odcmProperty)
        {
            return GetPropertyFieldName(odcmProperty) + "Collection";
        }

        public static string GetFetcherFieldName(OdcmProperty odcmProperty)
        {
            return GetPropertyFieldName(odcmProperty) + "Fetcher";
        }

        public static Identifier GetCollectionTypeName(OdcmClass odcmClass)
        {
            Identifier instanceTypeName = GetConcreteTypeName(odcmClass);

            return new Identifier(instanceTypeName.Namespace, instanceTypeName.Name + "Collection");
        }

        public static Identifier GetCollectionInterfaceName(OdcmClass odcmClass)
        {
            Identifier collectionTypeName = GetCollectionTypeName(odcmClass);

            return new Identifier(collectionTypeName.Namespace, "I" + collectionTypeName.Name);
        }

        public static string GetConcreteFieldName(OdcmProperty property)
        {
            return GetPropertyFieldName(property) + "Concrete";
        }

        public static Identifier GetEntityContainerInterfaceName(OdcmClass odcmContainer)
        {
            var containerName = GetEntityContainerTypeName(odcmContainer);

            return new Identifier(containerName.Namespace, "I" + containerName.Name);
        }

        public static Identifier GetEntityContainerTypeName(OdcmClass odcmContainer)
        {
            var resolvedName = ResolveIdentifier(odcmContainer);

            return new Identifier(resolvedName.Namespace, resolvedName.Name);
        }

        public static string GetNamespaceName(OdcmNamespace @namespace)
        {
            return ResolveNamespace(@namespace.Name);
        }

        public static string GetPropertyName(OdcmProperty odcmProperty)
        {
            return GetPropertyName(odcmProperty.Name);
        }

        public static string GetPropertyName(string propertyName)
        {
            if (ConfigurationService.Settings.ForcePropertyPascalCasing)
                propertyName = propertyName.Substring(0, 1).ToUpper() + propertyName.Substring(1);

            return propertyName;
        }

        private static Identifier ResolveIdentifier(OdcmType odcmType)
        {
            return ResolveIdentifier(odcmType.Namespace.Name, odcmType.Name);
        }

        private static Identifier ResolveIdentifier(OdcmClass odcmContainer)
        {
            return ResolveIdentifier(odcmContainer.Namespace.Name, odcmContainer.Name);
        }

        private static Identifier ResolveIdentifier(string @namespace, string name)
        {
            var resolvedNamespace = "global::" + ResolveNamespace(@namespace);

            var resolvedName = ResolveName(@namespace, name);

            return new Identifier(resolvedNamespace, resolvedName);
        }

        private static string ResolveNamespace(string @namespace)
        {
            if (ConfigurationService.Settings.OdcmNamespaceToProxyNamespace.ContainsKey(@namespace))
            {
                @namespace = ConfigurationService.Settings.OdcmNamespaceToProxyNamespace[@namespace];
            }

            if (ConfigurationService.Settings.NamespacePrefix != null)
            {
                @namespace = String.Format("{0}.{1}", ConfigurationService.Settings.NamespacePrefix, @namespace);
            }

            return @namespace;
        }

        private static string ResolveName(string @namespace, string name)
        {
            if (ConfigurationService.Settings.OdcmClassNameToProxyClassName.ContainsKey(@namespace)
                && ConfigurationService.Settings.OdcmClassNameToProxyClassName[@namespace].ContainsKey(name))
            {
                name = ConfigurationService.Settings.OdcmClassNameToProxyClassName[@namespace][name];
            }

            return name;
        }

        public static string GetModelPropertyName(OdcmProperty odcmProperty)
        {
            return odcmProperty.Name;
        }
    }
}
