using System.Net.Http.Json;
using MarketPlace.Api.Features.Auth.Register;
using MarketPlace.Api.Features.Users.Login;
using MarketPlace.Api.Features.Users.Register;
using MarketPlace.Test.IntegrationTest.SetUp;

namespace MarketPlace.Test.IntegrationTest.Users;

public sealed class UserRegistrationTests(CustomWebApplicationFactory factory)
    : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task Register_WithInvalidEmail_ReturnsBadRequest()
    {
        RegisterUserRequest request = new("gmail", "Password@123");

        var response = await httpClient.PostAsJsonAsync("auth/register", request);

        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Register_WithInvalidPassword_ReturnsBadRequest()
    {
        RegisterUserRequest request = new($"user-{Guid.NewGuid():N}@example.com", "@123");

        var response = await httpClient.PostAsJsonAsync("auth/register", request);

        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Register_WithWhitespaceInEmail_ReturnsBadRequest()
    {
        RegisterUserRequest request = new("use r@gmail", "Password@123");

        var response = await httpClient.PostAsJsonAsync("auth/register", request);

        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Register_WithValidEmailAndPassword_ReturnsOtpDetails()
    {
        RegisterUserRequest request = new(
            $"testuser-{Guid.NewGuid():N}@example.com",
            "Password@123"
        );

        var response = await httpClient.PostAsJsonAsync("auth/register", request);
        var registration = await response.Content.ReadFromJsonAsync<RegisterUserResponse>();

        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(registration);
        Assert.NotEqual(Guid.Empty, registration.OtpId);
        Assert.Equal("Registration Successful. Check email for otp", registration.Message);
    }

    [Fact]
    public async Task Register_WithExistingEmail_ReturnsLoginInstruction()
    {
        string email = $"existing-{Guid.NewGuid():N}@example.com";
        RegisterUserRequest request = new(email, "Password@123");

        var firstResponse = await httpClient.PostAsJsonAsync("auth/register", request);
        var secondResponse = await httpClient.PostAsJsonAsync("auth/register", request);
        var registration = await secondResponse.Content.ReadFromJsonAsync<RegisterUserResponse>();

        Assert.Equal(System.Net.HttpStatusCode.OK, firstResponse.StatusCode);
        Assert.Equal(System.Net.HttpStatusCode.OK, secondResponse.StatusCode);
        Assert.NotNull(registration);
        Assert.Equal(Guid.Empty, registration.OtpId);
        Assert.Equal("Login", registration.Message);
    }

    [Fact]
    public async Task RegisterVerify_WithInvalidOtp_ReturnsBadRequest()
    {
        RegisterUserRequest request = new($"verify-{Guid.NewGuid():N}@example.com", "Password@123");

        var registerResponse = await httpClient.PostAsJsonAsync("auth/register", request);
        var registration = await registerResponse.Content.ReadFromJsonAsync<RegisterUserResponse>();
        Assert.NotNull(registration);

        var verifyResponse = await httpClient.PostAsJsonAsync(
            "auth/register/verify",
            new RegisterUserVerifyOtpRequest(registration.OtpId, "000000")
        );

        Assert.Equal(System.Net.HttpStatusCode.BadRequest, verifyResponse.StatusCode);
    }

    [Fact]
    public async Task Login_WithSixConcurrentAttemptsForSameEmail_ReturnsTooManyRequests()
    {
        string email = $"rate-limit-{Guid.NewGuid():N}@example.com";
        var requests = await Task.WhenAll(
            Enumerable
                .Range(0, 6)
                .Select(_ => new EmailLoginRequest(email, "Password@123"))
                .Select(s => httpClient.PostAsJsonAsync("auth/login/email", s))
        );

        Assert.Contains(
            requests,
            r => r is { StatusCode: System.Net.HttpStatusCode.TooManyRequests }
        );
    }

    [Fact]
    public async Task Login_WhenIpRequestCapacityIsExceeded_ReturnsTooManyRequestsWithRetryAfter()
    {
        var requests = await Task.WhenAll(
            Enumerable
                .Range(0, 16)
                .Select(_ => new EmailLoginRequest(
                    $"ip-rate-limit-{Guid.NewGuid():N}@example.com",
                    "Password@123"
                ))
                .Select(request => httpClient.PostAsJsonAsync("auth/login/email", request))
        );

        var rejectedResponse = Assert.Single(
            requests,
            response => response.StatusCode == System.Net.HttpStatusCode.TooManyRequests
        );

        Assert.True(rejectedResponse.Headers.Contains("x-rate-limit-retry-after"));
    }
}
