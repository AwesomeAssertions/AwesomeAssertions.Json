[![build](https://github.com/AwesomeAssertions/AwesomeAssertions.json/actions/workflows/build.yml/badge.svg)](https://github.com/AwesomeAssertions/AwesomeAssertions.json/actions/workflows/build.yml)
[![](https://img.shields.io/github/release/AwesomeAssertions/FluentAssertions.Json.svg?label=latest%20release)](https://github.com/AwesomeAssertions/FluentAssertions.Json/releases/latest)
[![](https://img.shields.io/nuget/dt/AwesomeAssertions.json.svg?label=nuget%20downloads)](https://www.nuget.org/packages/AwesomeAssertions.json)
[![](https://img.shields.io/librariesio/dependents/nuget/AwesomeAssertions.Json.svg?label=dependent%20libraries)](https://libraries.io/nuget/AwesomeAssertions.Json)
![](https://img.shields.io/badge/release%20strategy-githubflow-orange.svg)

### Available extension methods

- `BeEquivalentTo()`
- `ContainSingleElement()`
- `ContainSubtree()`
- `HaveCount()`
- `HaveElement()`
- `HaveValue()`
- `MatchRegex()`
- `NotBeEquivalentTo()`
- `NotHaveElement()`
- `NotHaveValue()`
- `NotMatchRegex()`

See "in-code" description for more information.

### Usage

Be sure to include `using FluentAssertions.Json;` otherwise false positives may occur.

```c#
using FluentAssertions;
using FluentAssertions.Json;
using Newtonsoft.Json.Linq;

... 
var actual = JToken.Parse(@"{ ""key1"" : ""value"" }");
var expected = JToken.Parse(@"{ ""key2"" : ""value"" }");
actual.Should().BeEquivalentTo(expected);
```

You can also use `IJsonAssertionOptions<>` with `Should().BeEquivalentTo()` assertions, which contains helper methods that you can use to specify the way you want to compare specific data types.

Example:

```c#
using FluentAssertions;
using FluentAssertions.Json;
using Newtonsoft.Json.Linq;

... 
var actual = JToken.Parse(@"{ ""value"" : 1.5 }");
var expected = JToken.Parse(@"{ ""value"" : 1.4 }");
actual.Should().BeEquivalentTo(expected, options => options
                .Using<double>(d => d.Subject.Should().BeApproximately(d.Expectation, 0.1))
                .WhenTypeIs<double>());
```