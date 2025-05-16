using Newtonsoft.Json;

namespace AwesomeAssertions.Json.Specs.Models
{
    public class PocoWithIgnoredProperty
    {
        public int Id { get; set; }

        [JsonIgnore]
        public string Name { get; set; }
    }
}
