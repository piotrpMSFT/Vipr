using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using Microsoft.Its.Recipes;
using Xunit;

namespace CSharpWriterUnitTests
{
    public class Given_an_OdcmClass_Entity_Uninitialized : NavigationPropertyTestBase
    {
        public Given_an_OdcmClass_Entity_Uninitialized()
        {
            base.Init(m =>
            {
                var @namespace = m.Namespaces[0];
                NavTargetClass = Any.OdcmEntityClass(@namespace);
                @namespace.Types.Add(NavTargetClass);

                var @class = @namespace.Classes.First();
                NavigationProperty = Any.OdcmProperty(p =>
                {
                    p.Class = @class;
                    p.Type = NavTargetClass;
                    p.IsCollection = true;
                });

                m.Namespaces[0].Classes.First().Properties.Add(NavigationProperty);
            });
        }

        [Fact]
        public void When_not_bound_to_Context_and_updated_through_Concrete_accessor_then_throws_InvalidOperationException()
        {
            var instance = Activator.CreateInstance(ConcreteType);

            var relatedInstance = Activator.CreateInstance(NavTargetConcreteType);

            var collection = Activator.CreateInstance(typeof(List<>).MakeGenericType(NavTargetConcreteType));

            collection.InvokeMethod("Add", new[] { relatedInstance });

            Action act = () => instance.SetPropertyValue(NavigationProperty.Name, collection);

            act.ShouldThrow<TargetInvocationException>()
                .WithInnerException<InvalidOperationException>()
                .WithInnerMessage("Not Initialized");
        }
    }
}