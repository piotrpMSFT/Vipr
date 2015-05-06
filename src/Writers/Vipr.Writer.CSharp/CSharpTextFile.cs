using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Vipr.Core;

namespace Vipr.Writer.CSharp
{
    public class CSharpTextFile : TextFile
    {
        private readonly SourceCodeBuilder _builder = new SourceCodeBuilder();

        private readonly IEnumerable<string> _dependencies = new List<string>
        {
            "global::Microsoft.OData.Client",
            "global::Microsoft.OData.Edm",
            "System",
            "System.Collections.Generic",
            "System.ComponentModel",
            "System.Linq",
            "System.Reflection",
            "System.Threading.Tasks",
        };

        public CSharpTextFile(Enum @enum)
        {
            Write(_dependencies);

            _("namespace {0}", @enum.Identifier.Namespace.Replace("global::", ""));
            using (_builder.IndentBraced)
            {
                Write(@enum);
            }

            Contents = _builder.ToString();
            RelativePath = @enum.Identifier.Namespace.Replace("global::", "") + "\\" + @enum.Identifier.Name;
        }

        public CSharpTextFile(Class @class)
        {
            Write(_dependencies);

            _("namespace {0}", @class.Identifier.Namespace.Replace("global::", ""));
            using (_builder.IndentBraced)
            {
                Write(@class);
            }

            Contents = _builder.ToString();
            RelativePath = @class.Identifier.Namespace.Replace("global::", "") + "\\" + @class.Identifier.Name;
        }

        public CSharpTextFile(Interface @interface)
        {
            Write(_dependencies);

            _("namespace {0}", @interface.Identifier.Namespace.Replace("global::", ""));
            using (_builder.IndentBraced)
            {
                Write(@interface);
            }

            Contents = _builder.ToString();
            RelativePath = @interface.Identifier.Namespace.Replace("global::", "") + "\\" + @interface.Identifier.Name;
        }

        private void Write(IEnumerable<string> dependencies)
        {
            foreach (var dependency in dependencies)
            {
                WriteDependency(dependency);
            }
        }

        private void Write(IEnumerable<Class> classes)
        {
            foreach (var @class in classes)
            {
                Write(@class);
            }
        }

        private void WriteDependency(string dependency)
        {
            _("using {0};", dependency);
        }

        private void Write(Enum @enum)
        {
            WriteDescription(@enum.Description);
            _("public enum {0} : {1}", @enum.Identifier.Name, @enum.UnderlyingType);
            using (_builder.IndentBraced)
            {
                Write(@enum.Members);
            }
        }

        private void Write(IEnumerable<EnumMember> enumMembers)
        {
            foreach (var enumMember in enumMembers)
            {
                Write(enumMember);
            }
        }

        private void Write(EnumMember enumMember)
        {
            _builder.Write("{0}", enumMember.Name);
            if (enumMember.Value.HasValue) _builder.Write(" = {0}", enumMember.Value);
            _builder.Write(",");
        }

        private void Write(Class @class)
        {
            WriteDescription(@class.Description);
            Write(@class.Attributes);

            _("{0}{1}partial class {2}{3}", @class.AccessModifier, @class.AbstractModifier, @class.Identifier.Name,
                GetInheritenceString(@class.Interfaces, @class.BaseClass));

            using (_builder.IndentBraced)
            {
                Write(@class.Fields);

                Write(@class.Properties);

                Write(@class.Constructors);

                Write(@class.Methods);

                Write(@class.Indexers);

                Write(@class.NestedClasses);
            }
        }

        private void Write(IEnumerable<Constructor> constructors)
        {
            foreach (var constructor in constructors)
            {
                Write(constructor as dynamic);
            }
        }

        private void Write(IEnumerable<Indexer> indexers)
        {
            foreach (var indexer in indexers)
            {
                Write(indexer as dynamic);
            }
        }

        private void Write(IEnumerable<Field> fields)
        {
            foreach (var field in fields)
            {
                Write(field as dynamic);
            }
        }

        private void Write(IEnumerable<Method> methods)
        {
            foreach (var method in methods)
            {
                Write(method as dynamic);
            }
        }

        private void Write(AddAsyncMethod method)
        {
            WriteSignature(method);
            using (_builder.IndentBraced)
            {
                _("if (Entity == null)");

                using (_builder.IndentBraced)
                {
                    _("Context.AddObject(Path, item);");
                }

                _("else");

                using (_builder.IndentBraced)
                {
                    _("var lastSlash = Path.LastIndexOf('/');");
                    _("var shortPath = (lastSlash >= 0 && lastSlash < Path.Length - 1) ? Path.Substring(lastSlash + 1) : Path;");
                    _("Context.AddRelatedObject(Entity, shortPath, item);");
                }

                _("if (!deferSaveChanges)");

                using (_builder.IndentBraced)
                {
                    _("return Context.SaveChangesAsync();");
                }

                _("else");

                using (_builder.IndentBraced)
                {
                    _("var retVal = new global::System.Threading.Tasks.TaskCompletionSource<object>();");
                    _("retVal.SetResult(null);");
                    _("return retVal.Task;");
                }
            }
        }

        private void Write(AddAsyncMediaMethod method)
        {
            WriteSignature(method);
            using (_builder.IndentBraced)
            {
                _("if (Entity == null)");

                using (_builder.IndentBraced)
                {
                    _("Context.AddObject(Path, item);");
                }

                _("else");

                using (_builder.IndentBraced)
                {
                    _("var lastSlash = Path.LastIndexOf('/');");
                    _("var shortPath = (lastSlash >= 0 && lastSlash < Path.Length - 1) ? Path.Substring(lastSlash + 1) : Path;");
                    _("Context.AddRelatedObject(Entity, shortPath, item);");
                }

                _("Context.SetSaveStream(item, stream, closeStream, new DataServiceRequestArgs()");
                _("{");
                using (_builder.Indent)
                {
                    _("ContentType = contentType");
                }
                _("});");

                _("if (!deferSaveChanges)");

                using (_builder.IndentBraced)
                {
                    _("return Context.SaveChangesAsync();");
                }

                _("else");

                using (_builder.IndentBraced)
                {
                    _("var retVal = new global::System.Threading.Tasks.TaskCompletionSource<object>();");
                    _("retVal.SetResult(null);");
                    _("return retVal.Task;");
                }
            }
        }

        private void Write(CollectionExecuteAsyncMethod method)
        {
            WriteSignature(method);
            using (_builder.IndentBraced)
            {
                _("return ExecuteAsyncInternal();");
            }
        }

        private void Write(CollectionCountAsyncMethod method)
        {
            WriteSignature(method);
            using (_builder.IndentBraced)
            {
                _("return new DataServiceQuerySingle<long>(Context, Path + \"/$count\").GetValueAsync();");
            }
        }

        private void Write(ContainerAddToCollectionMethod method)
        {
            WriteSignature(method);
            using (_builder.IndentBraced)
            {
                _("this.Context.AddObject(\"{0}\", (object) {1});", method.ModelCollectionName,
                    method.Parameters.First().Name);
            }
        }

        private void Write(ContainerGetPathMethod method)
        {
            WriteSignature(method);
            using (_builder.IndentBraced)
            {
                _("return propertyName == null ? _path : _path + \"/\" + propertyName;");
            }
        }

        private void Write(ContainerGetUrlMethod method)
        {
            WriteSignature(method);
            using (_builder.IndentBraced)
            {
                _("return this.Context.BaseUri;");
            }
        }

        private void Write(FetcherExecuteAsyncMethod method)
        {
            WriteSignature(method);
            using (_builder.IndentBraced)
            {
                _("return await EnsureQuery().ExecuteSingleAsync();");
            }
        }

        private void Write(ConcreteExecuteAsyncMethod method)
        {
            WriteSignature(method);
            using (_builder.IndentBraced)
            {
                _("var tsc = new global::System.Threading.Tasks.TaskCompletionSource<{0}>();", method.EntityIdentifier);
                _("tsc.SetResult(this);");
                _("return tsc.Task;");
            }
        }

        private void Write(CollectionGetByIdMethod method)
        {
            WriteSignature(method);
            using (_builder.IndentBraced)
            {
                _("return this[{0}];", method.Parameters.ToArgumentString());
            }
        }

        private void Write(CollectionGetByIdIndexer indexer)
        {
            WriteSignature(indexer);
            using (_builder.IndentBraced)
            {
                _("get");

                using (_builder.IndentBraced)
                {
                    _("var path = GetPath<{0}>((i) => {1});", NamesService.GetConcreteTypeName(indexer.OdcmClass), indexer.ParameterToPropertyMap.ToEquivalenceString("i"));
                    _("var fetcher = new {0}();", NamesService.GetFetcherTypeName(indexer.OdcmClass));
                    _("fetcher.Initialize(Context, path);");
                    _("");
                    _("return fetcher;");
                }
            }
        }

        private void Write(EnsureQueryMethod method)
        {
            WriteSignature(method);
            using (_builder.IndentBraced)
            {
                _("if (this._query == null)");

                using (_builder.IndentBraced)
                {
                    _("this._query = CreateQuery<{0}, {1}>();", method.FetchedType, method.FetchedTypeInterface);
                }

                _("return this._query;");
            }
            _();
            _("private {0} _query;", method.ReturnType);
        }

        private void Write(ContainerNameFromTypeMethod method)
        {
            WriteSignature(method);
            using (_builder.IndentBraced)
            {
                _("string resolvedType;");
                _("resolvedType = Context.DefaultResolveNameInternal(clientType,  \"{0}\", \"{1}\");", method.ServerNamespace, method.ClientNamespace);
                _("if (!string.IsNullOrEmpty(resolvedType))");
                using (_builder.IndentBraced)
                {
                    _("return resolvedType;");
                }

                _("return clientType.FullName;");
            }
        }

        private void Write(ContainerTypeFromNameMethod method)
        {
            WriteSignature(method);
            using (_builder.IndentBraced)
            {
                _("global::System.Type resolvedType;");
                _("resolvedType = Context.DefaultResolveTypeInternal(typeName,  \"{0}\", \"{1}\");", method.ClientNamespace, method.ServerNamespace);
                _("if (resolvedType != null)");
                using (_builder.IndentBraced)
                {
                    _("return resolvedType;");
                }

                _("return null;");
            }
        }

        private void Write(FetcherExpandMethod method)
        {
            WriteSignature(method);
            using (_builder.IndentBraced)
            {
                if (method.DefiningInterface == null)
                {
                    _("return ({0}) new {1}()",
                        NamesService.GetFetcherInterfaceName(method.OdcmClass),
                        NamesService.GetFetcherTypeName(method.OdcmClass));

                    using (_builder.IndentBraced)
                    {
                        _("_query = this.EnsureQuery().Expand<TTarget>(navigationPropertyAccessor)");
                    }
                    _(";");
                }
                else
                {
                    _("return ({0}) this;", NamesService.GetFetcherInterfaceName(method.OdcmClass));
                }
            }
        }

        private void Write(GeneratedEdmModelLoadModelFromStringMethod method)
        {
            WriteSignature(method);
            using (_builder.IndentBraced)
            {
                _("global::System.Xml.XmlReader reader = CreateXmlReader(Edmx);");
                _("try");
                using (_builder.IndentBraced)
                {
                    _("return global::Microsoft.OData.Edm.Csdl.EdmxReader.Parse(reader);");
                }
                _("finally");
                using (_builder.IndentBraced)
                {
                    _("((global::System.IDisposable)(reader)).Dispose();");
                }
            }
        }

        private void Write(GeneratedEdmModelGetInstanceMethod method)
        {
            WriteSignature(method);
            using (_builder.IndentBraced)
            {
                _("return ParsedModel;");
            }
        }

        private void Write(GeneratedEdmModelCreateXmlReaderMethod method)
        {
            WriteSignature(method);
            using (_builder.IndentBraced)
            {
                _("return global::System.Xml.XmlReader.Create(new global::System.IO.StringReader(edmxToParse));");
            }
        }

        private void Write(FetcherUpcastMethod method)
        {
            WriteSignature(method);
            using (_builder.IndentBraced)
            {
                _("var derivedFetcher = new {0}();", NamesService.GetFetcherTypeName(method.DerivedType));
                _("derivedFetcher.Initialize(this.Context, this.GetPath((string) null));");
                _("return ({0}) derivedFetcher;", NamesService.GetFetcherInterfaceName(method.DerivedType));
            }
        }

        private void Write(ConcreteUpcastMethod method)
        {
            WriteSignature(method);
            using (_builder.IndentBraced)
            {
                _("return ({0}) this;", NamesService.GetFetcherInterfaceName(method.DerivedType));
            }
        }

        private void Write(EntityInstanceFunctionMethod method)
        {
            WriteSignature(method);
            using (_builder.IndentBraced)
            {
                WriteEntityMethodBodyStart(method);
                _("return ({0}) Enumerable.Single<{1}>(await this.Context.ExecuteAsync<{1}>(requestUri, \"{2}\", true, new OperationParameter[]",
                    method.ReturnType.GenericParameters.First(), method.InstanceName, method.HttpMethod);
                using (_builder.IndentBraced)
                {
                    WriteMethodOperationParameters(method);
                }
                _("));");
            }
        }

        private void Write(EntityCollectionFunctionMethod method)
        {
            WriteSignature(method);
            using (_builder.IndentBraced)
            {
                WriteEntityMethodBodyStart(method);
                _("return (await this.Context.ExecuteAsync<{0}>(requestUri, \"{1}\", false, new OperationParameter[]",
                    method.InstanceName, method.HttpMethod);
                using (_builder.IndentBraced)
                {
                    WriteMethodOperationParameters(method);
                }
                _("));");
            }
        }

        private void Write(EntityVoidMethod method)
        {
            WriteSignature(method);
            using (_builder.IndentBraced)
            {
                WriteEntityMethodBodyStart(method);
                _("await this.Context.ExecuteAsync(requestUri, \"{0}\", new OperationParameter[{1}]", method.HttpMethod,
                    method.Parameters.Count());
                using (_builder.IndentBraced)
                {
                    WriteMethodOperationParameters(method);
                }
                _(");");
            }
        }

        private void WriteMethodOperationParameters(ServerMethod method)
        {
            foreach (var parameter in method.BodyParameters)
                _("new BodyOperationParameter(\"{0}\", (object) {0}),", parameter.Name);

            foreach (var parameter in method.UriParameters)
                _("new UriOperationParameter(\"{0}\", (object) {0}),", parameter.Name);
        }

        private void WriteEntityMethodBodyStart(Method method)
        {
            WriteInitializationEnforcer();

            _("Uri myUri = this.GetUrl();");
            _("if (myUri == (Uri) null)");
            _(" throw new Exception(\"cannot find entity\");");
            _("Uri requestUri = new Uri(myUri.ToString().TrimEnd('/') + \"/{0}\");", method.ModelName);
        }

        private void WriteInitializationEnforcer()
        {
            _("if (this.Context == null)");
            _("    throw new InvalidOperationException(\"Not Initialized\");");
        }

        private void WriteSignature(MethodSignature method, bool isForInterface = false)
        {
            var accessModifier = GetAccessModifier(method.Visibility);

            var asyncModifier = method.IsAsync ? "async " : "";

            var staticModifier = method.IsStatic ? "static " : "";

            var overrideModifier = method.IsOverriding ? "new " : "";

            var explicitName = method.DefiningInterface == null
                ? string.Empty
                : method.DefiningInterface + ".";

            if (explicitName != String.Empty) accessModifier = String.Empty;

            var genericParameters = method.GenericParameters == null
                ? string.Empty
                : "<" + String.Join(", ", method.GenericParameters) + ">";

            var template = isForInterface ? "{3}{4} {6}{7}({8});" : "{0}{1}{2}{3}{4} {5}{6}{7}({8})";

            WriteMethodDescription(method);
            _(template, accessModifier, asyncModifier, staticModifier, overrideModifier, method.ReturnType, explicitName, method.Name,
                genericParameters, method.Parameters.ToParametersString());
        }

        private string GetAccessModifier(Visibility visibility)
        {
            switch (visibility)
            {
                case Visibility.Internal:
                    return "internal ";
                case Visibility.Private:
                    return "private ";
                case Visibility.Protected:
                    return "protected ";
                case Visibility.ProtectedInternal:
                    return "protected internal ";
                case Visibility.Public:
                    return "public ";
            }
            return string.Empty;
        }

        private void WriteSignature(IndexerSignature indexer, bool? @public = true)
        {
            var accessModifier = @public.HasValue ? @public.Value ? "public" : "private" : "";

            _("{0} {1} this[{2}]", accessModifier, indexer.ReturnType, indexer.Parameters.ToParametersString());
        }

        private void Write(DefaultConstructor constructor)
        {
            var baseConstructor = constructor.IsDerived ? ": base()" : String.Empty;

            _("public {0}(){1}", constructor.Identifier.Name, baseConstructor);

            _("{");

            _("}");
        }

        private void Write(IEnumerable<Property> properties)
        {
            foreach (var property in properties)
            {
                Write(property as dynamic);
            }
        }

        private void Write(FetcherNavigationCollectionProperty property)
        {
            WriteDeclaration(property);

            using (_builder.IndentBraced)
            {
                _("get");

                using (_builder.IndentBraced)
                {
                    _("if (this.{0} == null)", property.FieldName);
                    using (_builder.IndentBraced)
                    {
                        _("this.{0} = new {1}(\n" +
                          "   Context != null ? Context.CreateQuery<{2}>(GetPath(\"{3}\")) : null,\n" +
                          "   Context,\n" +
                          "   this,\n" +
                          "   GetPath(\"{3}\"));", property.FieldName, property.CollectionType, property.InstanceType,
                            property.ModelName);
                    }
                    _("");
                    _("return this.{0};", property.FieldName);
                }
            }
        }

        private void Write(ContainerNavigationCollectionProperty property)
        {
            WriteDeclaration(property);

            using (_builder.IndentBraced)
            {
                _("get");

                using (_builder.IndentBraced)
                {
                    _("if (this.{0} == null)", property.FieldName);
                    using (_builder.IndentBraced)
                    {
                        _("this.{0} = new {1}(\n" +
                          "   Context != null ? Context.CreateQuery<{2}>(GetPath(\"{3}\")) : null,\n" +
                          "   Context,\n" +
                          "   this,\n" +
                          "   GetPath(\"{3}\"));", property.FieldName, property.CollectionType, property.InstanceType,
                            property.ModelName);
                    }
                    _("");
                    _("return this.{0};", property.FieldName);
                }
            }
        }

        private void Write(ConcreteNavigationCollectionProperty property)
        {
            WriteDeclaration(property);

            using (_builder.IndentBraced)
            {
                _("get");

                using (_builder.IndentBraced)
                {
                    _("return new {0}<{1}, {2}>(Context, ({3}<{2}>) {4});",
                        NamesService.GetExtensionTypeName("PagedCollection"),
                        NamesService.GetConcreteInterfaceName(property.OdcmType),
                        NamesService.GetConcreteTypeName(property.OdcmType),
                        "DataServiceCollection",
                        property.Name);
                }
            }
        }

        private void Write(ConcreteNavigationCollectionAccessorProperty property)
        {
            WriteDeclaration(property);

            using (_builder.IndentBraced)
            {
                _("get");

                using (_builder.IndentBraced)
                {
                    _("if (this.{0} == null)", property.FieldName);
                    using (_builder.IndentBraced)
                    {
                        _("this.{0} = new {1}<{2}>();", property.FieldName, NamesService.GetExtensionTypeName("EntityCollectionImpl"), property.InstanceType);
                        _("this.{0}.SetContainer(() => GetContainingEntity(\"{1}\"));", property.FieldName, property.ModelName);
                    }
                    _("");
                    _("return ({0})this.{1};", property.Type, property.FieldName);
                }

                _("set");

                using (_builder.IndentBraced)
                {
                    WriteInitializationEnforcer();

                    _("{0}.Clear();", property.Name);
                    _("if (value != null)");
                    using (_builder.IndentBraced)
                    {
                        _("foreach (var i in value)");
                        using (_builder.IndentBraced)
                        {
                            _("{0}.Add(i);", property.Name);
                        }
                    }
                }
            }
        }

        private void Write(Field field)
        {
            var defaultValue = field.DefaultValue == null
                ? String.Empty
                : String.Format(" = {0}", field.DefaultValue);

            var @const = field.IsConstant
                ? "const "
                : String.Empty;

            var @static = field.IsStatic
                ? "static "
                : String.Empty;

            _("private {0}{1}{2} {3}{4};", @const, @static, field.Type, field.Name, defaultValue);
        }

        private void Write(FetcherNavigationProperty property)
        {
            WriteDeclaration(property);

            using (_builder.IndentBraced)
            {
                _("get");

                using (_builder.IndentBraced)
                {
                    _("if (this.{0} == null)", property.FieldName);
                    using (_builder.IndentBraced)
                    {
                        _("this.{0} = new {1}();", property.FieldName, property.InstanceType);
                        _("this.{0}.Initialize(this.Context, GetPath(\"{1}\"));", property.FieldName, property.ModelName);
                    }
                    _("");
                    _("return this.{0};", property.FieldName);
                }
            }
        }

        private void Write(ContainerNavigationProperty property)
        {
            WriteDeclaration(property);

            using (_builder.IndentBraced)
            {
                _("get");

                using (_builder.IndentBraced)
                {
                    _("if (this.{0} == null)", property.FieldName);
                    using (_builder.IndentBraced)
                    {
                        _("this.{0} = new {1}();", property.FieldName, property.InstanceType);
                        _("this.{0}.Initialize(this.Context, GetPath(\"{1}\"));", property.FieldName, property.ModelName);
                    }
                    _("");
                    _("return this.{0};", property.FieldName);
                }

                _("private set");

                using (_builder.IndentBraced)
                {
                    _("this.{0} = ({1})value;", property.FieldName, property.InstanceType);
                }
            }
        }

        private void Write(ConcreteNavigationAccessorProperty property)
        {
            WriteDeclaration(property);

            using (_builder.IndentBraced)
            {
                _("get");

                using (_builder.IndentBraced)
                {
                    _("return this.{0};", property.FieldName);
                }

                _("set");

                using (_builder.IndentBraced)
                {
                    _("if (this.{0} != value)", property.FieldName);
                    using (_builder.IndentBraced)
                    {
                        _("this.{0} = value;", property.FieldName);
                        _("if (Context != null && Context.GetEntityDescriptor(this) != null && (value == null || Context.GetEntityDescriptor(value) != null))");
                        using (_builder.IndentBraced)
                        {
                            _("Context.SetLink(this, \"{0}\", value);", property.ModelName);
                        }
                    }
                }
            }
        }

        private void Write(ConcreteNavigationProperty property)
        {
            WriteDeclaration(property);

            using (_builder.IndentBraced)
            {
                _("get");

                using (_builder.IndentBraced)
                {
                    _("return this.{0};", property.FieldName);
                }

                _("set");

                using (_builder.IndentBraced)
                {
                    _("if (this.{0} != value)", property.Name);
                    using (_builder.IndentBraced)
                    {
                        _("this.{0} = ({1})value;", property.Name, property.FieldType);
                    }
                }
            }
        }

        private void WriteDeclaration(Property property, bool isForInterface = false)
        {
            WriteDescription(property.Description);
            var accessModifier = GetAccessModifier(property.Visibility);

            var template = isForInterface ? "{1} {3};" : "{0}{1} {2}{3}";

            var explicitName = property.DefiningInterface == null
                ? string.Empty
                : property.DefiningInterface + ".";

            if (explicitName != string.Empty)
                accessModifier = string.Empty;

            _(template, accessModifier, property.Type, explicitName, property.Name);
        }

        private void Write(Interface @interface)
        {
            WriteDescription(@interface.Description);
            Write(@interface.Attributes);
            _("public partial interface {0}{1}", @interface.Identifier.Name, GetInheritenceString(@interface.Interfaces));

            using (_builder.IndentBraced)
            {
                Write(@interface.Properties);

                Write(@interface.Methods);

                Write(@interface.Indexers);
            }
        }

        private void Write(IEnumerable<Attribute> attributes)
        {
            foreach (var attribute in attributes)
            {
                Write(attribute);
            }
        }

        private void Write(Attribute attribute)
        {
            _(attribute.ToString());
        }

        private void Write(IEnumerable<IndexerSignature> indexerSignatures)
        {
            foreach (var indexerSignature in indexerSignatures)
            {
                WriteSignature(indexerSignature, null);

                WriteInterfaceAutoPropertyBody(!indexerSignature.IsGettable, !indexerSignature.IsSettable);
            }
        }

        private void Write(IEnumerable<MethodSignature> methodSignatures)
        {
            foreach (var methodSignature in methodSignatures)
            {
                if (methodSignature.Visibility == Visibility.Public)
                {
                    WriteSignature(methodSignature, true);
                }
            }
        }

        private static string GetInheritenceString(IEnumerable<Type> interfaces, Type baseClass = null)
        {
            var inheritenceList = new List<string>();

            if (baseClass != null) inheritenceList.Add(baseClass.ToString());

            if (interfaces != null) inheritenceList.AddRange(interfaces.Select(i => i.ToString()));

            if (!inheritenceList.Any())
                return String.Empty;

            return ":" + string.Join(", ", inheritenceList);
        }

        private void Write(StructuralProperty property)
        {
            WriteDeclaration(property);

            using (_builder.IndentBraced)
            {
                WritePropertyGet(property);

                WritePropertySet(property);
            }
        }

        private void Write(ObsoletedProperty property)
        {
            _("[EditorBrowsable(EditorBrowsableState.Never)]");
            _("[Obsolete(\"Use {0} instead.\")]", property.UpdatedName);
            WriteDeclaration(property);

            using (_builder.IndentBraced)
            {
                WriteObsoletedPropertyGet(property);

                WriteObsoletedPropertySet(property);
            }
        }

        private void Write(StructuralCollectionProperty property)
        {
            WriteDeclaration(property);

            using (_builder.IndentBraced)
            {
                _("get");

                using (_builder.IndentBraced)
                {
                    _("if (this.{0} == default({1}))", property.FieldName, property.Type);

                    using (_builder.IndentBraced)
                    {
                        _("this.{0} = new {1}<{2}>();", property.FieldName,
                            NamesService.GetExtensionTypeName("NonEntityTypeCollectionImpl"), property.InstanceType);
                        _("this.{0}.SetContainer(() => GetContainingEntity(\"{1}\"));", property.FieldName, property.ModelName);
                    }

                    _("return this.{0};", property.FieldName);
                }
                _("set");
                using (_builder.IndentBraced)
                {
                    _("{0}.Clear();", property.Name);
                    _("if (value != null)");

                    using (_builder.IndentBraced)
                    {
                        _("foreach (var i in value)");

                        using (_builder.IndentBraced)
                        {
                            _("{0}.Add(i);", property.Name);
                        }
                    }
                }
            }
        }

        private void Write(IEnumerable<InterfaceProperty> properties)
        {
            foreach (var property in properties)
            {
                Write(property);
            }
        }

        private void Write(InterfaceProperty property)
        {
            WriteDeclaration(property);
            WriteInterfaceAutoPropertyBody(property.PrivateGet, property.PrivateSet);
        }

        private void Write(AutoProperty property)
        {
            WriteDeclaration(property);
            WriteAutoPropertyBody(property.PrivateGet, property.PrivateSet);
        }

        private void WriteDeclaration(InterfaceProperty property)
        {
            WriteDescription(property.Description);
            WritePropertyDeclaration(property.Type.ToString(), property.Name);
        }

        private void WriteDescription(string description)
        {
            if (!string.IsNullOrEmpty(description))
            {
                _builder.Write("/// <summary>");
                _builder.Write("/// " + description);
                _builder.Write("/// </summary>");
            }
        }

        private void WriteMethodDescription(MethodSignature method)
        {
            if (!string.IsNullOrEmpty(method.Description))
            {
                _builder.Write("/// <summary>");
                _builder.Write("/// " + method.Description);
                _builder.Write("/// </summary>");
            }

            if (method.Parameters != null)
            {
                var parameters = method.Parameters.Where(p => !string.IsNullOrEmpty(p.Description));
                foreach (var param in parameters)
                {
                    _builder.Write(string.Format("/// <param name=\"{0}\">", param.Name));
                    _builder.Write("/// " + param.Description);
                    _builder.Write("/// </param>");
                }
            }
        }

        private void WritePropertyDeclaration(string typeName, string name)
        {
            _builder.Write("{0} {1}", typeName, name);
        }

        private void WritePropertySet(StructuralProperty property)
        {
            _("set");
            using (_builder.IndentBraced)
            {
                _("if (value != {0})", property.FieldName);
                using (_builder.IndentBraced)
                {
                    _("{0} = value;", property.FieldName);
                    _("OnPropertyChanged(\"{0}\");", property.ModelName);
                }
            }
        }

        private void WritePropertyGet(StructuralProperty property)
        {
            _("get");
            using (_builder.IndentBraced)
            {
                _("return {0};", property.FieldName);
            }
        }

        private void WriteObsoletedPropertySet(ObsoletedProperty property)
        {
            _("set");
            using (_builder.IndentBraced)
            {
                _("{0} = value;", property.UpdatedName);
            }
        }

        private void WriteObsoletedPropertyGet(ObsoletedProperty property)
        {
            _("get");
            using (_builder.IndentBraced)
            {
                _("return {0};", property.UpdatedName);
            }
        }

        private void WriteInterfaceAutoPropertyBody(bool hideGet = false, bool hideSet = false)
        {
            _("{{{0}{1}}}", hideGet ? string.Empty : "get;", hideSet ? string.Empty : "set;");
        }

        private void WriteAutoPropertyBody(bool privateGet = false, bool privateSet = false)
        {
            _("{{{0}get; {1}set;}}", privateGet ? "private " : string.Empty, privateSet ? "private " : string.Empty);
        }

        private void Write(CollectionConstructor collectionConstructor)
        {
            _("internal {0}(global::Microsoft.OData.Client.DataServiceQuery inner," +
              "{1} context," +
              "object entity," +
              "string path)" +
              ": base(inner, context, entity as {2}, path)",
                NamesService.GetCollectionTypeName(collectionConstructor.OdcmClass).Name,
                NamesService.GetExtensionTypeName("DataServiceContextWrapper"),
                NamesService.GetExtensionTypeName("EntityBase"));
            _("{");

            _("}");
        }

        private void Write(EntityContainerConstructor entityContainerConstructor)
        {
            _("public {0}(global::System.Uri serviceRoot, global::System.Func<global::System.Threading.Tasks.Task<string>> accessTokenGetter)",
                entityContainerConstructor.Name);
            using (_builder.IndentBraced)
            {
                _("Context = new {0}(serviceRoot, global::Microsoft.OData.Client.ODataProtocolVersion.V4, accessTokenGetter);", NamesService.GetExtensionTypeName("DataServiceContextWrapper"));
                _("Context.MergeOption = global::Microsoft.OData.Client.MergeOption.OverwriteChanges;");
                _("Context.ResolveName = new global::System.Func<global::System.Type, string>(this.ResolveNameFromType);");
                _("Context.ResolveType = new global::System.Func<string, global::System.Type>(this.ResolveTypeFromName);");
                _("this.OnContextCreated();");
                _("Context.Format.LoadServiceModel = GeneratedEdmModel.GetInstance;");
                _("Context.Format.UseJson();");
            }
            _("partial void OnContextCreated();");
        }

        private void _(string text, params object[] args)
        {
            WriteDebugComment();
            _builder.WriteLine(text, args);
        }

        private void _(string text)
        {
            WriteDebugComment();

            _builder.WriteLine(text);
        }

        private void _()
        {
            _(String.Empty);
        }

        private void WriteDebugComment()
        {
            if (Debugger.IsAttached)
            {
                var zed = new StackFrame(2, true);
                _builder.WriteLine("// {0}", zed.ToString());
            }
        }
    }
}