using System;
using AwesomeAssertions.Equivalency;

namespace AwesomeAssertions.Json
{
    /// <summary>
    /// Represents the run-time type-specific behavior of a JSON structural equivalency assertion. It is the equivalent of <see cref="AwesomeAssertions.Equivalency.EquivalencyAssertionOptions{T}"/>
    /// </summary>
    public sealed class JsonAssertionOptions<T> : EquivalencyOptions<T>, IJsonAssertionOptions<T>
    {
        public JsonAssertionOptions(EquivalencyOptions<T> equivalencyAssertionOptions) : base(equivalencyAssertionOptions)
        {

        }
        public new IJsonAssertionRestriction<T, TProperty> Using<TProperty>(Action<IAssertionContext<TProperty>> action)
        {
            return new JsonAssertionRestriction<T, TProperty>(base.Using(action));
        }
    }
}
