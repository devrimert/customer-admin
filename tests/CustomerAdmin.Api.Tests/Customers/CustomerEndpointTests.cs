using System.Net;
using System.Net.Http.Json;
using CustomerAdmin.Api.Features.Customers;
using CustomerAdmin.Api.Tests.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using CustomerAdmin.Api.Data;

namespace CustomerAdmin.Api.Tests.Customers;

[Collection(ApiCollection.Name)]
public sealed class CustomerEndpointTests(ApiFactory factory) : ApiTestBase(factory)
{
    [Fact]
    public async Task Create_returns_201_with_location_trimmed_name_and_version()
    {
        var request = new CustomerRequest { Name = "  Mueller GmbH  ", Email = "info@mueller.example" };

        var response = await Client.PostAsJsonAsync("/api/customers", request, Ct);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var customer = await response.Content.ReadFromJsonAsync<CustomerDetailResponse>(Ct);
        Assert.NotNull(customer);
        Assert.Equal("Mueller GmbH", customer.Name);
        Assert.True(customer.Version > 0);
        Assert.Equal($"/api/customers/{customer.Id}", response.Headers.Location?.AbsolutePath);
    }

    [Theory]
    [InlineData("", null, null)]
    [InlineData("Valid GmbH", "not-an-email", null)]
    [InlineData("Valid GmbH", null, "abc")]
    [InlineData("Valid GmbH", null, "12")]
    public async Task Create_with_invalid_data_returns_400(string name, string? email, string? phone)
    {
        var request = new CustomerRequest { Name = name, Email = email, Phone = phone };

        var response = await Client.PostAsJsonAsync("/api/customers", request, Ct);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>(Ct);
        Assert.NotNull(problem);
        Assert.NotEmpty(problem.Errors);
    }

    [Fact]
    public async Task Get_unknown_customer_returns_404_as_problem_details()
    {
        var response = await Client.GetAsync("/api/customers/999999", Ct);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task Search_ignores_case_and_finds_customers_by_contact_name()
    {
        var tag = Tag();
        var byOwnName = await CreateCustomerAsync($"ACME {tag}");
        var byContact = await CreateCustomerAsync($"Other {Tag()}");
        await CreateContactAsync(byContact.Id, "Anna", $"Search{tag}");

        var foundByName = await Client.GetFromJsonAsync<List<CustomerSummaryResponse>>(
            $"/api/customers?search={Uri.EscapeDataString($"acme {tag}")}", Ct);
        var foundByContact = await Client.GetFromJsonAsync<List<CustomerSummaryResponse>>(
            $"/api/customers?search={Uri.EscapeDataString($"anna search{tag}")}", Ct);

        Assert.Equal([byOwnName.Id], foundByName!.Select(c => c.Id));
        Assert.Equal([byContact.Id], foundByContact!.Select(c => c.Id));
    }

    [Fact]
    public async Task Search_treats_percent_sign_as_text()
    {
        var tag = Tag();
        var withPercent = await CreateCustomerAsync($"Discount 50% {tag}");
        await CreateCustomerAsync($"Discount 500 {tag}");

        // Without escaping, "50%" would also match "500"
        var found = await Client.GetFromJsonAsync<List<CustomerSummaryResponse>>(
            $"/api/customers?search={Uri.EscapeDataString($"50% {tag}")}", Ct);

        Assert.Equal([withPercent.Id], found!.Select(c => c.Id));
    }

    [Fact]
    public async Task Inactive_customers_are_hidden_unless_requested()
    {
        var tag = Tag();
        var customer = await CreateCustomerAsync($"Inactive {tag}");
        var deactivate = ToUpdateRequest(customer) with { IsActive = false };
        (await Client.PutAsJsonAsync($"/api/customers/{customer.Id}", deactivate, Ct)).EnsureSuccessStatusCode();

        var activeOnly = await Client.GetFromJsonAsync<List<CustomerSummaryResponse>>(
            $"/api/customers?search={tag}", Ct);
        var all = await Client.GetFromJsonAsync<List<CustomerSummaryResponse>>(
            $"/api/customers?search={tag}&includeInactive=true", Ct);

        Assert.Empty(activeOnly!);
        Assert.Single(all!);
    }

    [Fact]
    public async Task Update_with_stale_version_returns_409()
    {
        var customer = await CreateCustomerAsync();
        var firstSave = ToUpdateRequest(customer) with { City = "Ulm" };
        (await Client.PutAsJsonAsync($"/api/customers/{customer.Id}", firstSave, Ct)).EnsureSuccessStatusCode();

        // Second user still has the old version
        var secondSave = ToUpdateRequest(customer) with { City = "Neu-Ulm" };
        var response = await Client.PutAsJsonAsync($"/api/customers/{customer.Id}", secondSave, Ct);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal("Ulm", (await GetCustomerAsync(customer.Id)).City);
    }

    [Fact]
    public async Task Patch_changes_only_the_given_field()
    {
        var created = await CreateCustomerAsync();
        var withData = ToUpdateRequest(created) with { Email = "info@example.com", City = "Ulm" };
        var putResponse = await Client.PutAsJsonAsync($"/api/customers/{created.Id}", withData, Ct);
        var customer = (await putResponse.Content.ReadFromJsonAsync<CustomerDetailResponse>(Ct))!;

        var response = await PatchAsync(
            $"/api/customers/{customer.Id}",
            TestVersion(customer.Version),
            Replace("/isActive", false));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var patched = await GetCustomerAsync(customer.Id);
        Assert.False(patched.IsActive);
        Assert.Equal("info@example.com", patched.Email);
        Assert.Equal("Ulm", patched.City);
        Assert.NotEqual(customer.Version, patched.Version);
    }

    [Fact]
    public async Task Patch_requires_version_test_as_first_operation()
    {
        var customer = await CreateCustomerAsync();
        var url = $"/api/customers/{customer.Id}";

        var withoutTest = await PatchAsync(url, Replace("/isActive", false));
        var testAtTheEnd = await PatchAsync(url, Replace("/isActive", false), TestVersion(customer.Version));

        Assert.Equal(HttpStatusCode.PreconditionRequired, withoutTest.StatusCode);
        Assert.Equal(HttpStatusCode.PreconditionRequired, testAtTheEnd.StatusCode);
    }

    [Theory]
    [InlineData("remove", "/email", null)]
    [InlineData("replace", "/version", 1)]
    [InlineData("replace", "/doesNotExist", "x")]
    [InlineData("replace", "/name", "")]
    public async Task Patch_that_cannot_be_applied_returns_422(string op, string path, object? value)
    {
        var customer = await CreateCustomerAsync();

        var response = await PatchAsync(
            $"/api/customers/{customer.Id}",
            TestVersion(customer.Version),
            new { op, path, value });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    [Fact]
    public async Task Patch_with_stale_version_returns_409()
    {
        var customer = await CreateCustomerAsync();
        var newer = ToUpdateRequest(customer) with { City = "Ulm" };
        (await Client.PutAsJsonAsync($"/api/customers/{customer.Id}", newer, Ct)).EnsureSuccessStatusCode();

        var response = await PatchAsync(
            $"/api/customers/{customer.Id}",
            TestVersion(customer.Version),
            Replace("/isActive", false));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Delete_removes_the_customer_and_its_contacts()
    {
        var customer = await CreateCustomerAsync();
        var contact = await CreateContactAsync(customer.Id, "Anna");

        var response = await Client.DeleteAsync($"/api/customers/{customer.Id}", Ct);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await Client.GetAsync($"/api/customers/{customer.Id}", Ct)).StatusCode);

        // Check the database directly: the contact is gone because of ON DELETE CASCADE
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CustomerAdminDbContext>();
        Assert.False(await db.Contacts.AnyAsync(c => c.Id == contact.Id, Ct));
    }
}