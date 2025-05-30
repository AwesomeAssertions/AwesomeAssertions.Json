using AwesomeAssertions.Formatting;
using Newtonsoft.Json.Linq;
using Xunit;

namespace AwesomeAssertions.Json.Specs
{
    // ReSharper disable InconsistentNaming
    public class JTokenFormatterSpecs
    {
        public JTokenFormatterSpecs()
        {
            Formatter.AddFormatter(new JTokenFormatter());
        }

        [Fact]
        public void Should_Handle_JToken()
        {
            // Act / Arrange
            var actual = Formatter.ToString(JToken.Parse("{}"));

            // Assert
            actual.Should().Be("{}");
        }

        [Fact]
        public void Should_preserve_indenting()
        {
            // Arrange
            var json = JToken.Parse("{ \"id\":1 }");

            // Act
            var actual = Formatter.ToString(json, new FormattingOptions
            {
                UseLineBreaks = true
            });

            // Assert
            actual.Should().Be(json.ToString(Newtonsoft.Json.Formatting.Indented));
        }

        [Fact]
        public void Should_Remove_line_breaks_and_indenting()
        {
            // Arrange
            var json = JToken.Parse("{ \"id\":1 }");

            // Act
            // ReSharper disable once RedundantArgumentDefaultValue
            var actual = Formatter.ToString(json, new FormattingOptions
            {
                UseLineBreaks = false
            });

            // Assert
            actual.Should().Be(json.ToString().RemoveNewLines());
        }
    }
}
