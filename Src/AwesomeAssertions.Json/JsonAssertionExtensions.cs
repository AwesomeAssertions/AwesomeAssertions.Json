﻿using System.Diagnostics;
using AwesomeAssertions.Execution;
using JetBrains.Annotations;
using Newtonsoft.Json.Linq;

namespace AwesomeAssertions.Json
{
    /// <summary>
    ///     Contains extension methods for JToken assertions.
    /// </summary>
    [DebuggerNonUserCode]
    public static class JsonAssertionExtensions
    {
        /// <summary>
        ///     Returns an <see cref="JTokenAssertions"/> object that can be used to assert the current <see cref="JToken"/>.
        /// </summary>
        [Pure]
        public static JTokenAssertions Should(this JToken jToken)
        {
            return new JTokenAssertions(jToken, AssertionChain.GetOrCreate());
        }

        /// <summary>
        ///     Returns an <see cref="JTokenAssertions"/> object that can be used to assert the current <see cref="JObject"/>.
        /// </summary>
        [Pure]
        public static JTokenAssertions Should(this JObject jObject)
        {
            return new JTokenAssertions(jObject, AssertionChain.GetOrCreate());
        }

        /// <summary>
        ///     Returns an <see cref="JTokenAssertions"/> object that can be used to assert the current <see cref="JValue"/>.
        /// </summary>
        [Pure]
        public static JTokenAssertions Should(this JValue jValue)
        {
            return new JTokenAssertions(jValue, AssertionChain.GetOrCreate());
        }
    }
}
