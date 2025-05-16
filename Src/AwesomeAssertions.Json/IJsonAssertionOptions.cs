using System;
using FluentAssertions.Equivalency;

namespace AwesomeAssertions.Json
{
    /// <summary>
    /// Provides the run-time details of the <see cref="AwesomeAssertions.Json.JsonAssertionOptions{T}" /> class.
    /// </summary>
    public interface IJsonAssertionOptions<T>
    {
        /// <summary>
        /// Overrides the comparison of subject and expectation to use provided <paramref name="action" />
        /// when the predicate is met.
        /// </summary>
        /// <param name="action">
        /// The assertion to execute when the predicate is met.
        /// </param>
        IJsonAssertionRestriction<T, TProperty> Using<TProperty>(Action<IAssertionContext<TProperty>> action);
    }
}
