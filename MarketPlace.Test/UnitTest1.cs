using MarketPlace.Api.Features.Users.Login;

namespace MarketPlace.Test
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            Assert.Equal(4, 2 * 2);
        }

        [Theory]
        [InlineData("hello")]
        public void Test_NotNull(object data)
        {
            Assert.NotNull(data);
        }
    }
}
