using MaybeTypes;

namespace UnitTests.Optional
{
    public class OptionalTests
    {
        [Fact]
        public void Test_OptionalWithValueInt()
        {
            var optional = new Some<int>(5);
            Assert.Equal(5, (int)optional);
        }

        [Fact]
        public void Test_OptionalWithoutValueInt()
        {
            var optional = new None<int>();
            Assert.True(optional.Equals(None.Value));
        }

        [Fact]
        public void Test_OptionalWithoutValueWhenNoneString()
        {
            var optional = new None<string>();
            Assert.Equal("none", optional.Reduce("none"));
        }

        [Fact]
        public void Test_OptionalWithValueString()
        {
            var optional = new Some<string>("hello");
            Assert.Equal("hello", (string)optional);
        }
    }
}
