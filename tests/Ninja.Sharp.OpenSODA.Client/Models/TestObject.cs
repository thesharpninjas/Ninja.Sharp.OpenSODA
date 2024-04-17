using Ninja.Sharp.OpenSODA.Attributes;

namespace Ninja.Sharp.OpenSODA.Client.Models
{
    [Collection("TestObject")]
    internal class TestObject
    {
        public string One { get; set; } = string.Empty;
        public string Two { get; set; } = string.Empty;
        public string Three { get; set; } = string.Empty;
    }
}
