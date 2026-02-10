using System;
using Newtonsoft.Json.Linq;

namespace AwesomeAssertions.Json
{
    /// <summary>
    /// </summary>
    public static class JsonAssertionConfiguration
    {
        private static Func<string, JToken> _parser = JToken.Parse;

        public static Func<string, JToken> ParseFunction
        {
            get
            {
                return _parser;
            }

            set
            {
                if (value is null)
                    throw new ArgumentNullException(nameof(ParseFunction));
                _parser = value;
            }
        }
    }
}
