using System.Net;
using System.Net.Http.Json;
using CustomerAdmin.Api.Data;
using CustomerAdmin.Api.Domain;
using CustomerAdmin.Api.Features.Contacts;
using CustomerAdmin.Api.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace CustomerAdmin.Api.Tests.Contacts;

[Collection(ApiCollection.Name)]
public sealed class ContactEndpointTests(ApiFactory factory) : ApiTestBase(factory)
{
    [Fact]
    public async Task New_primary_contact_takes_over_from_the_previous_one()
    {
        var customer = await CreateCustomerAsync();
        var anna = await CreateContactAsync(customer.Id, "Anna", isPrimary: true);
        var ben = await CreateContactAsync(customer.Id, "Ben", isPrimary: true);

        var contacts = (await GetCustomerAsync(customer.Id)).Contacts;

        Assert.Equal(ben.Id, contacts[0].Id);
        Assert.True(contacts[0].IsPrimary);
        Assert.False(contacts.Single(c => c.Id == anna.Id).IsPrimary);
    }

    [Fact]
    public async Task Losing_the_primary_flag_changes_the_version()
    {
        var customer = await CreateCustomerAsync();
        var anna = await CreateContactAsync(customer.Id, "Anna", isPrimary: true);
        await CreateContactAsync(customer.Id, "Ben", isPrimary: true);

        // Anna was changed in the background, so her old version is stale now
        var request = new UpdateContactRequest
        {
            FirstName = "Anna",
            LastName = anna.LastName,
            IsPrimary = false,
            Version = anna.Version
        };
        var response = await Client.PutAsJsonAsync($"/api/customers/{customer.Id}/contacts/{anna.Id}", request, Ct);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Patch_can_make_a_contact_primary()
    {
        var customer = await CreateCustomerAsync();
        var anna = await CreateContactAsync(customer.Id, "Anna");
        var ben = await CreateContactAsync(customer.Id, "Ben", isPrimary: true);

        var response = await PatchAsync(
            $"/api/customers/{customer.Id}/contacts/{anna.Id}",
            TestVersion(anna.Version),
            Replace("/isPrimary", true));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var contacts = (await GetCustomerAsync(customer.Id)).Contacts;
        Assert.True(contacts.Single(c => c.Id == anna.Id).IsPrimary);
        Assert.False(contacts.Single(c => c.Id == ben.Id).IsPrimary);
    }

    [Fact]
    public async Task Database_rejects_a_second_primary_contact()
    {
        var customer = await CreateCustomerAsync();
        await CreateContactAsync(customer.Id, "Anna", isPrimary: true);

        // Bypass the service on purpose: the rule must hold even if application code gets it wrong
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CustomerAdminDbContext>();
        db.Contacts.Add(new Contact { CustomerId = customer.Id, FirstName = "Ben", LastName = "Test", IsPrimary = true });

        var exception = await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync(Ct));
        var postgres = Assert.IsType<PostgresException>(exception.InnerException);
        Assert.Equal(DatabaseNames.OnePrimaryContactPerCustomer, postgres.ConstraintName);
    }

    [Fact]
    public async Task Contact_of_another_customer_is_not_reachable()
    {
        var owner = await CreateCustomerAsync();
        var other = await CreateCustomerAsync();
        var anna = await CreateContactAsync(owner.Id, "Anna");

        var get = await Client.GetAsync($"/api/customers/{other.Id}/contacts/{anna.Id}", Ct);
        var delete = await Client.DeleteAsync($"/api/customers/{other.Id}/contacts/{anna.Id}", Ct);

        Assert.Equal(HttpStatusCode.NotFound, get.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, delete.StatusCode);
        Assert.Single((await GetCustomerAsync(owner.Id)).Contacts);
    }

    [Fact]
    public async Task List_returns_404_for_unknown_customer_and_empty_list_for_customer_without_contacts()
    {
        var customer = await CreateCustomerAsync();

        var unknown = await Client.GetAsync("/api/customers/999999/contacts", Ct);
        var empty = await Client.GetFromJsonAsync<List<ContactResponse>>($"/api/customers/{customer.Id}/contacts", Ct);

        Assert.Equal(HttpStatusCode.NotFound, unknown.StatusCode);
        Assert.Empty(empty!);
    }

    [Fact]
    public async Task List_search_matches_first_and_last_name()
    {
        var customer = await CreateCustomerAsync();
        var anna = await CreateContactAsync(customer.Id, "Anna", "Muster");
        await CreateContactAsync(customer.Id, "Ben", "Beispiel");

        var found = await Client.GetFromJsonAsync<List<ContactResponse>>(
            $"/api/customers/{customer.Id}/contacts?search={Uri.EscapeDataString("anna mus")}", Ct);

        Assert.Equal([anna.Id], found!.Select(c => c.Id));
    }

    [Fact]
    public async Task Delete_removes_the_contact()
    {
        var customer = await CreateCustomerAsync();
        var anna = await CreateContactAsync(customer.Id, "Anna");

        var response = await Client.DeleteAsync($"/api/customers/{customer.Id}/contacts/{anna.Id}", Ct);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Empty((await GetCustomerAsync(customer.Id)).Contacts);
    }
}