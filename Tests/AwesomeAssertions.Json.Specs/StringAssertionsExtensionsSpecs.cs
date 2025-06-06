using System;
using Newtonsoft.Json.Linq;
using Xunit;
using Xunit.Sdk;

namespace AwesomeAssertions.Json.Specs
{
    // ReSharper disable ExpressionIsAlwaysNull
    public class StringAssertionsExtensionsSpecs
    {
        #region BeValidJson

        [Fact]
        public void When_checking_valid_json_BeValidJson_should_succeed()
        {
            // Arrange
            string subject = "{ id: 42, admin: true }";

            // Act
            Action act = () => subject.Should().BeValidJson();

            // Assert
            act.Should().NotThrow();
        }

        [Fact]
        public void When_checking_valid_json_BeValidJson_should_enable_consecutive_jtoken_assertions()
        {
            // Arrange
            string subject = "{ id: 42 }";

            // Act
            object which = subject.Should().BeValidJson().Which;

            // Assert
            which.Should().BeAssignableTo<JToken>();
        }

        [Fact]
        public void When_checking_null_BeValidJson_should_fail()
        {
            // Arrange
            string subject = null;

            // Act
            Action act = () => subject.Should().BeValidJson("null is not allowed");

            // Assert
            act.Should().Throw<XunitException>()
                .Which.Message.Should()
                .Match("Expected subject to be valid JSON because null is not allowed, but parsing failed with \"*\".");
        }

        [Fact]
        public void When_checking_invalid_json_BeValidJson_should_fail()
        {
            // Arrange
            string subject = "invalid json";

            // Act
            Action act = () => subject.Should().BeValidJson("we like {0}", "JSON");

            // Assert
            act.Should()
                .Throw<XunitException>()
                .Which.Message.Should()
                .Match("Expected subject to be valid JSON because we like JSON, but parsing failed with \"*\".");
        }

        #endregion

    }
}
