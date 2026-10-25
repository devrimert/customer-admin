using CustomerAdmin.Api.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.JsonPatch.SystemTextJson;

namespace CustomerAdmin.Api.Features.Customers;

[ApiController]
[Route("api/customers")]
public sealed class CustomerController(CustomerService customers) : ControllerBase
{
    /// <summary>
    /// Lists customers sorted by name. Inactive customers are hidden unless requested.
    /// The search matches the customer name and the names of its contacts.
    /// </summary>
    [HttpGet]
    public Task<IReadOnlyList<CustomerSummaryResponse>> GetList(
        [FromQuery] string? search,
        [FromQuery] bool includeInactive,
        CancellationToken cancellationToken) =>
        customers.GetListAsync(search, includeInactive, cancellationToken);

    /// <summary>
    /// Gives detailed customer information by id.
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType<CustomerDetailResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CustomerDetailResponse>> GetById(int id, CancellationToken cancellationToken)
    {
        var customer = await customers.GetByIdAsync(id, cancellationToken);
        return customer is null ? NotFound() : Ok(customer);
    }

    /// <summary>
    /// Creates customers.
    /// </summary>
    [HttpPost]
    [ProducesResponseType<CustomerDetailResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CustomerDetailResponse>> Create(
        CustomerRequest request,
        CancellationToken cancellationToken)
    {
        var customer = await customers.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = customer.Id }, customer);
    }

    /// <summary>Replaces all fields.</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType<CustomerDetailResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CustomerDetailResponse>> Update(
        int id,
        UpdateCustomerRequest request,
        CancellationToken cancellationToken)
    {
        var customer = await customers.UpdateAsync(id, request, cancellationToken);
        return customer is null ? NotFound() : Ok(customer);
    }

    /// <summary>
    /// Changes single fields of a customer with JSON Patch (RFC 6902).
    /// The first operation has to test the version, after that only "replace" is allowed.
    /// Example:
    /// [{ "op": "test", "path": "/version", "value": 731 }, { "op": "replace", "path": "/isActive", "value": false }]
    /// </summary>
    /// <remarks>Status codes follow RFC 5789: 409 if the version test fails, 422 if the patch can't be applied.</remarks>
    [HttpPatch("{id:int}")]
    [Consumes("application/json-patch+json")]
    [ProducesResponseType<CustomerDetailResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status428PreconditionRequired)]
    public async Task<ActionResult<CustomerDetailResponse>> Patch(
        int id,
        JsonPatchDocument<UpdateCustomerRequest> patch,
        CancellationToken cancellationToken)
    {
        if (this.CheckOperations(patch) is { } invalidPatch)
        {
            return invalidPatch;
        }

        var request = await customers.GetUpdateRequestAsync(id, cancellationToken);
        if (request is null)
        {
            return NotFound();
        }

        if (this.ApplyAndValidate(patch, request) is { } patchProblem)
        {
            return patchProblem;
        }

        var customer = await customers.UpdateAsync(id, request, cancellationToken);
        return customer is null ? NotFound() : Ok(customer);
    }

    /// <summary>Permanently deletes the customer and all its contacts.</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var deleted = await customers.DeleteAsync(id, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }
}