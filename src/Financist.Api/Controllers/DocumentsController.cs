using Asp.Versioning;
using Financist.Application.Features.Documents;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Financist.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Authorize]
[Route("api/v{version:apiVersion}/documents")]
public sealed class DocumentsController : ControllerBase
{
    private readonly IDocumentService _documentService;

    public DocumentsController(IDocumentService documentService)
    {
        _documentService = documentService;
    }

    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(DocumentImportDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<DocumentImportDto>> Upload([FromForm] UploadDocumentApiRequest request, CancellationToken cancellationToken)
    {
        if (request.File is null || request.File.Length == 0)
        {
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Invalid document upload",
                Detail = "A non-empty file is required."
            });
        }

        await using var stream = request.File.OpenReadStream();

        var response = await _documentService.UploadAsync(
            new UploadDocumentRequest
            {
                Content = stream,
                FileName = request.File.FileName,
                ContentType = string.IsNullOrWhiteSpace(request.File.ContentType)
                    ? "application/octet-stream"
                    : request.File.ContentType,
                SizeBytes = request.File.Length
            },
            cancellationToken);

        return Created($"/api/v1/documents/{response.Id}", response);
    }

    public sealed class UploadDocumentApiRequest
    {
        public IFormFile? File { get; init; }
    }
}
