using Iyulab.Auth.Shared;
using System.Net.Http.Json;

namespace Iyulab.Auth.Client;

public class AuthService
{
    private readonly HttpClient _client;

    public AuthService(string endPoint)
    {
        _client = CreateClient(endPoint);
    }

    public async Task<Token?> SignInAsync(SignIn request)
    {
        var response = await _client.GetAsync($"");
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        return await response.Content.ReadFromJsonAsync<Token>();
    }

    public async Task<Token?> SignUpAsync(SignUp request)
    {
        var response = await _client.GetAsync($"");
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        return await response.Content.ReadFromJsonAsync<Token>();
    }

    public async Task<Token?> RefreshTokenAsync(string refreshToken)
    {
        var response = await _client.GetAsync($"");
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        return await response.Content.ReadFromJsonAsync<Token>();
    }

    public async Task SignOutAsync(string refreshToken)
    {
        await _client.GetAsync($"");
    }

    private HttpClient CreateClient(string endPoint)
    {
        var client = new HttpClient
        {
            BaseAddress = new Uri(endPoint)
        };
        return client;
    }

}
