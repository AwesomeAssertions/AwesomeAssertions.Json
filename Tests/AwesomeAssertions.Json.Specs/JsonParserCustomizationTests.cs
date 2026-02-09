using System;
using System.IO;
using AwesomeAssertions.Equivalency;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;
using Xunit.Sdk;

namespace AwesomeAssertions.Json.Specs
{
    public sealed class JsonParserCustomizationTests : IDisposable
    {
        // restore JsonAssertionConfiguration.ParseFunction
        // after each test
        #region setup/teardown
        private readonly Func<string, JToken> prevParser;

        public JsonParserCustomizationTests()
        {
            prevParser = JsonAssertionConfiguration.ParseFunction;
        }

        public void Dispose()
        {
            JsonAssertionConfiguration.ParseFunction = prevParser;
            GC.SuppressFinalize(this);           
        }
        #endregion

        private readonly Action testSubject = () =>
        {
            var date = new JValue("2000-01-01T00:00:00Z");

            date.Should().BeEquivalentTo("\"2000-01-01T00:00:00Z\"");
        };

        [Fact]
        public void Assertion_of_string_containing_date__with_default_parser__fails()
        {
            testSubject.Should()
                .Throw<Xunit.Sdk.XunitException>(because: "default parser creates a DateTime when meets a date-like string")
                .Which
                .Message.Should().Contain("JSON document has a string instead of a date at $.");
        }

        [Fact]
        public void Assertion_of_string_containing_date__with_custom_parser__should_succeed()
        {
            JsonAssertionConfiguration.ParseFunction = CustomParser;

            testSubject.Should().NotThrow(); 
        }

        // this is the same as JToken.Parse()
        // with additional step:
        // jsonReader.DateParseHandling = DateParseHandling.None;
        private static JToken CustomParser(string json)
        {
            using var jsonReader = new JsonTextReader(new StringReader(json));
            
            jsonReader.DateParseHandling = DateParseHandling.None;

            var result = JToken.Load(jsonReader);
            while (jsonReader.Read())
            {
            }

            return result;
        }
    }
}
