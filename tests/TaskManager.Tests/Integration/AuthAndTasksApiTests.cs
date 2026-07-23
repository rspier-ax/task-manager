using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using TaskManager.Application.DTOs.Auth;
using TaskManager.Application.DTOs.Tasks;
using TaskManager.Infrastructure.Persistence;
using Xunit;
using TaskStatus = TaskManager.Domain.Enums.TaskStatus;

namespace TaskManager.Tests.Integration;

public class AuthAndTasksApiTests : IClassFixture<TaskManagerApiFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly TaskManagerApiFactory _factory;

    public AuthAndTasksApiTests(TaskManagerApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Health_returns_ok()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/health");
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("ok", json.GetProperty("status").GetString());
    }

    [Fact]
    public async Task Login_seed_returns_token()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginRequest(DbSeeder.DemoEmail, DbSeeder.DemoPassword));

        var body = await response.Content.ReadFromJsonAsync<AuthResponse>(JsonOptions);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.False(string.IsNullOrWhiteSpace(body?.Token));
        Assert.Equal(DbSeeder.DemoEmail, body!.Email);
    }

    [Fact]
    public async Task Tasks_without_token_returns_401()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/tasks");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Register_login_and_crud_tasks()
    {
        var client = _factory.CreateClient();
        var email = $"user-{Guid.NewGuid():N}@test.local";

        var register = await client.PostAsJsonAsync(
            "/api/auth/register",
            new RegisterRequest(email, "Secret1!", "Integration User"));
        Assert.Equal(HttpStatusCode.OK, register.StatusCode);

        var auth = await register.Content.ReadFromJsonAsync<AuthResponse>(JsonOptions);
        Assert.False(string.IsNullOrWhiteSpace(auth?.Token));
        client.WithBearer(auth!.Token);

        var create = await client.PostAsJsonAsync(
            "/api/tasks",
            new CreateTaskRequest("Integration task", "notes", TaskStatus.Todo, null));
        Assert.Equal(HttpStatusCode.Created, create.StatusCode);

        var created = await create.Content.ReadFromJsonAsync<TaskDto>(JsonOptions);
        Assert.NotNull(created);
        Assert.Equal("Integration task", created!.Title);

        var list = await client.GetAsync("/api/tasks");
        Assert.Equal(HttpStatusCode.OK, list.StatusCode);
        var tasks = await list.Content.ReadFromJsonAsync<List<TaskDto>>(JsonOptions);
        Assert.Contains(tasks!, t => t.Id == created.Id);

        var get = await client.GetAsync($"/api/tasks/{created.Id}");
        Assert.Equal(HttpStatusCode.OK, get.StatusCode);

        var update = await client.PutAsJsonAsync(
            $"/api/tasks/{created.Id}",
            new UpdateTaskRequest("Updated task", null, TaskStatus.Done, null));
        Assert.Equal(HttpStatusCode.OK, update.StatusCode);
        var updated = await update.Content.ReadFromJsonAsync<TaskDto>(JsonOptions);
        Assert.Equal("Updated task", updated!.Title);
        Assert.Equal(TaskStatus.Done, updated.Status);

        var delete = await client.DeleteAsync($"/api/tasks/{created.Id}");
        Assert.Equal(HttpStatusCode.NoContent, delete.StatusCode);

        var missing = await client.GetAsync($"/api/tasks/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, missing.StatusCode);
    }

    [Fact]
    public async Task Create_task_with_empty_title_returns_400()
    {
        var client = _factory.CreateClient();
        var token = await client.LoginAsDemoAsync();
        client.WithBearer(token);

        var response = await client.PostAsJsonAsync(
            "/api/tasks",
            new CreateTaskRequest("   ", null, TaskStatus.Todo, null));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Get_unknown_task_returns_404()
    {
        var client = _factory.CreateClient();
        var token = await client.LoginAsDemoAsync();
        client.WithBearer(token);

        var response = await client.GetAsync($"/api/tasks/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
