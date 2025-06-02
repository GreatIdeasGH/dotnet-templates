using GreatIdeas.PagedList;
using GreatIdeas.Template.Application.Features.Audits;
using GreatIdeas.Template.WebAPI.Extensions;

namespace GreatIdeas.Template.WebAPI.Endpoints;

public sealed class AuditEndpoints : IEndpoint
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup(ApiRouteNames.AuditEndpoint).WithTags("audits").WithOpenApi();

        // GET: api/Audit
        group
            .MapGet("paged", GetPagedAudits)
            .WithName("GetPagedAudits")
            .WithDescription("Get audits with pagination")
            .WithSummary("Get audits with pagination")
            .Produces<ApiPagingResponse<AuditResponse>>()
            .ProducesCommonForbiddenErrors()
            .RequireAuthorization(AuditPolicy.CanView());

        // GET: api/Audit/{auditId}
        group
            .MapGet("{auditId:guid}", GetAudit)
            .WithName("GetAudit")
            .WithDescription("Get existing audit by id")
            .WithSummary("Get existing audit by id")
            .Produces<AuditDetailResponse>()
            .ProducesCommonForbiddenErrors()
            .RequireAuthorization(AuditPolicy.CanManage());
    }

    public static async Task<IResult> GetPagedAudits(
        [AsParameters] AuditPagingParameters pagingParams,
        IGetPagedAuditHandler handler,
        HttpResponse response,
        CancellationToken token
    )
    {
        var result = await handler.GetPagedAudits(pagingParams, token);
        if (result.IsError)
        {
            return Results.Extensions.Problem(result.Errors);
        }

        var pagination = new PagedListMetaData(result.Value);
        var paged = new ApiPagingResponse<AuditResponse>(result.Value, pagination);

        response.Headers.Append("X-Pagination", JsonSerializer.Serialize(pagination));
        return TypedResults.Ok(paged);
    }

    public static async Task<IResult> GetAudit(
        Guid auditId,
        IGetAuditLogByIdHandler handler,
        CancellationToken token
    )
    {
        var response = await handler.GetAuditLogById(auditId, token);
        return response.Match(
            data => TypedResults.Ok(data),
            errors => Results.Extensions.Problem(errors)
        );
    }
}
