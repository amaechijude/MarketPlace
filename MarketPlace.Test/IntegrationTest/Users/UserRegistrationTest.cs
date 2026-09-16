using System.Net.Http.Json;
using MarketPlace.Api.Features.Users.Login;
using MarketPlace.Api.Features.Users.Register;
using MarketPlace.Test.IntegrationTest.SetUp;

namespace MarketPlace.Test.IntegrationTest.Users;

public sealed class UserRegistrationTest(CustomWebApplicationFactory factory)
    : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task RegisterWithInvalidEmail_ShouldFail()
    {
        RegisterUserRequest request = new("gmail", "Password@123");

        var response = await httpClient.PostAsJsonAsync("auth/register", request);
        Assert.False(response.IsSuccessStatusCode);
    }

    [Fact]
    public async Task RegisterWithInvalidPassword_ShouldFail()
    {
        RegisterUserRequest request = new("user@gmail", "@123");

        var response = await httpClient.PostAsJsonAsync("auth/register", request);
        Assert.False(response.IsSuccessStatusCode);
    }

    [Fact]
    public async Task RegisterWithValidEmail_ButContainsWhitespace_ShouldFail()
    {
        RegisterUserRequest request = new("use r@gmail", "Password@123");

        var response = await httpClient.PostAsJsonAsync("auth/register", request);
        Assert.False(response.IsSuccessStatusCode);
    }

    [Fact]
    public async Task RegisterWithValidEmailAndPassword_ShouldSucceed()
    {
        RegisterUserRequest request = new("testuser@gmail", "Password@123");

        var response = await httpClient.PostAsJsonAsync("auth/register", request);

        Assert.True(response.IsSuccessStatusCode);
    }

    [Fact]
    public async Task MoreThanSix_ConcurrentLoginAttampt_WithSameEmail_ShouldRateLimit()
    {
        var requests = await Task.WhenAll(
            Enumerable
                .Range(0, 6)
                .Select(_ => new EmailLoginRequest("testuser@gmail", "Password@123"))
                .Select(s => httpClient.PostAsJsonAsync("auth/login/email", s))
        );

        Assert.Contains(
            requests,
            r => r is { StatusCode: System.Net.HttpStatusCode.TooManyRequests }
        );
    }
}
