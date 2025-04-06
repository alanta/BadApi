using BadApi.Data;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace BadApi.Tests;

public class When_accessing_the_api
{
    [Fact]
    public async Task It_should_return_401_when_not_authenticated()
    {
        // Arrange
        var server = new WebApplicationFactory<Program>();
        var client = server.CreateClient();

        // Act
        var response = await client.GetAsync("/users");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }
    
    [Fact]
    public async Task It_should_return_200_when_properly_authenticated()
    {
        // Arrange
        var server = new WebApplicationFactory<Program>();
        server.SetupUser("jimmie", "TopSecret!", ["admin"]);
        
        var client = server.CreateClient();

        // Act
        var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "/users")
            .WithBasicAuth("jimmie", "TopSecret!");
        var response = await client.SendAsync(httpRequestMessage);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }
}

public static class AuthenticationExtensions
{
    public static void SetupUser(this WebApplicationFactory<Program> server, string name, string password, string[] roles)
    {
        var database = server.Server.Services.GetRequiredService<Database>();
        
        var user = new UserEntity
        {
            Id = 0,
            Name = name,
            Roles = roles
        };
        
        database.Users.Upsert(user);
        database.Users.SetPassword(user.Id, password);
    }
    
    public static HttpRequestMessage WithBasicAuth(this HttpRequestMessage request, string username, string password)
    {
        var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password}"));
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", credentials);
        return request;
    }
}