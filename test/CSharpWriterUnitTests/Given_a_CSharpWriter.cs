using System.Linq;
using FluentAssertions;
using Microsoft.Its.Recipes;
using Vipr.Writer.CSharp;
using Xunit;

namespace CSharpWriterUnitTests
{
    public class Given_a_CSharpWriter
    {
        [Fact]
        public void When_source_code_is_generated_Then_it_is_produced_in_multiple_files()
        {
            var model = Any.OdcmModel();

            var csWriter = new CSharpWriter();

            var proxyFiles = csWriter.GenerateProxy(model);

            proxyFiles.Count()
                .Should().BeGreaterThan(1, "Because each artifact should be in its own file");
        }
    }
}
