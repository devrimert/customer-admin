using System.Net.Http.Headers;
using System.Net.Http.Json;
using CustomerAdmin.Api.Features.Contacts;
using CustomerAdmin.Api.Features.Customers;

namespace CustomerAdmin.Api.Tests.Infrastructure;

/// <summary>
/// Shared helpers for the endpoint tests.
/// </summary>
public abstract class ApiTestBase(ApiFactory factory)
{
    protected ApiFactory Factory { get; } = factory;

    protected HttpClient Client { get; } = factory.CreateClient();

    /// <summary>Lets xUnit cancel a running request when the test run is stopped.</summary>
    protected static CancellationToken Ct => TestContext.Current.CancellationToken;

    /// <summary>
    /// A short random text. Tests put it into names, so searches only find their own data.
    /// </summary>
    protected static string Tag() => Guid.NewGuid().ToString("N")[..8];

    protected async Task<CustomerDetailResponse> CreateCustomerAsync(string? name = null)
    {
        var request = new CustomerRequest { Name = name ?? $"Customer {Tag()}" };
        var response = await Client.PostAsJsonAsync("/api/customers", request, Ct);
        response.EnsureSuccessStatusCode();

        return (await response.Content.ReadFromJsonAsync<CustomerDetailResponse>(Ct))!;
    }

    protected async Task<ContactResponse> CreateContactAsync(
        int customerId,
        string firstName,
        string lastName = "Test",
        bool isPrimary = false)
    {
        var request = new ContactRequest { FirstName = firstName, LastName = lastName, IsPrimary = isPrimary };
        var response = await Client.PostAsJsonAsync($"/api/customers/{customerId}/contacts", request, Ct);
        response.EnsureSuccessStatusCode();

        return (await response.Content.ReadFromJsonAsync<ContactResponse>(Ct))!;
    }

    protected async Task<CustomerDetailResponse> GetCustomerAsync(int id) =>
        (await Client.GetFromJsonAsync<CustomerDetailResponse>($"/api/customers/{id}", Ct))!;

    protected Task<HttpResponseMessage> PatchAsync(string url, params object[] operations) =>
        Client.PatchAsync(
            url,
            JsonContent.Create(operations, new MediaTypeHeaderValue("application/json-patch+json")),
            Ct);

    protected static object TestVersion(uint version) => new { op = "test", path = "/version", value = version };

    protected static object Replace(string path, object? value) => new { op = "replace", path, value };

    /// <summary>Builds a PUT body from a loaded customer, like the edit form will do.</summary>
    protected static UpdateCustomerRequest ToUpdateRequest(CustomerDetailResponse customer) => new()
    {
        Name = customer.Name,
        Email = customer.Email,
        Phone = customer.Phone,
        Street = customer.Street,
        PostalCode = customer.PostalCode,
        City = customer.City,
        Note = customer.Note,
        IsActive = customer.IsActive,
        Version = customer.Version
    };
}