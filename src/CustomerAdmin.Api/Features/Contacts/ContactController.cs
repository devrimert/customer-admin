using CustomerAdmin.Api.Common;
using Microsoft.AspNetCore.JsonPatch.SystemTextJson;
using Microsoft.AspNetCore.Mvc;

namespace CustomerAdmin.Api.Features.Contacts;

[ApiController]
[Route("api/customers/{customerId:int}/contacts")]
public sealed class ContactController(ContactService contacts) : ControllerBase
{
    /// <summary>Lists the contacts of a customer, primary contact first. The search matches first and last name.</summary>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<ContactResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<ContactResponse>>> GetList(
        int customerId,
        [FromQuery] string? search,
        CancellationToken cancellationToken)
    {
        var list = await contacts.GetListAsync(customerId, search, cancellationToken);
        return list is null ? NotFound() : Ok(list);
    }
    /// <summary>
    /// Gets the contact info by customer and contact id.
    /// </summary>
    [HttpGet("{contactId:int}")]
    [ProducesResponseType<ContactResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ContactResponse>> GetById(
        int customerId,
        int contactId,
        CancellationToken cancellationToken)
    {
        var contact = await contacts.GetByIdAsync(customerId, contactId, cancellationToken);
        return contact is null ? NotFound() : Ok(contact);
    }

    /// <summary>
    /// Creates a contact
    /// </summary>
    [HttpPost]
    [ProducesResponseType<ContactResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ContactResponse>> Create(
        int customerId,
        ContactRequest request,
        CancellationToken cancellationToken)
    {
        var contact = await contacts.CreateAsync(customerId, request, cancellationToken);

        return contact is null
            ? NotFound()
            : CreatedAtAction(nameof(GetById), new { customerId, contactId = contact.Id }, contact);
    }

    /// <summary>Replaces all fields of the contact.</summary>
    [HttpPut("{contactId:int}")]
    [ProducesResponseType<ContactResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ContactResponse>> Update(
        int customerId,
        int contactId,
        UpdateContactRequest request,
        CancellationToken cancellationToken)
    {
        var contact = await contacts.UpdateAsync(customerId, contactId, request, cancellationToken);
        return contact is null ? NotFound() : Ok(contact);
    }

    /// <summary>
    /// Changes single fields of a contact with JSON Patch (RFC 6902).
    /// The first operation has to test the version, after that only "replace" is allowed.
    /// Example:
    /// [{ "op": "test", "path": "/version", "value": 742 }, { "op": "replace", "path": "/isPrimary", "value": true }]
    /// </summary>
    /// <remarks>Status codes follow RFC 5789: 409 if the version test fails, 422 if the patch can't be applied.</remarks>
    [HttpPatch("{contactId:int}")]
    [Consumes("application/json-patch+json")]
    [ProducesResponseType<ContactResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status428PreconditionRequired)]
    public async Task<ActionResult<ContactResponse>> Patch(
        int customerId,
        int contactId,
        JsonPatchDocument<UpdateContactRequest> patch,
        CancellationToken cancellationToken)
    {
        if (this.CheckOperations(patch) is { } invalidPatch)
        {
            return invalidPatch;
        }

        var request = await contacts.GetUpdateRequestAsync(customerId, contactId, cancellationToken);
        if (request is null)
        {
            return NotFound();
        }

        if (this.ApplyAndValidate(patch, request) is { } patchProblem)
        {
            return patchProblem;
        }

        var contact = await contacts.UpdateAsync(customerId, contactId, request, cancellationToken);
        return contact is null ? NotFound() : Ok(contact);
    }

    /// <summary>Permanently deletes the contact (personal data, GDPR right to erasure).</summary>
    [HttpDelete("{contactId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int customerId, int contactId, CancellationToken cancellationToken)
    {
        var deleted = await contacts.DeleteAsync(customerId, contactId, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }
}