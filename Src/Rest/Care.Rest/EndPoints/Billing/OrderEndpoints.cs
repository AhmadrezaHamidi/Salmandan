using Care.Rest.EndPoints.Billing;

namespace Care.Rest.EndPoints;

/// <summary>Endpoints پرداخت امانی — وجه تا تأیید گزارش شیفت نزد پلتفرم می‌ماند</summary>
public class OrderEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup(BillingConstants.Routes.BaseRoute)
            .WithApiVersionSet()
            .WithTags(BillingConstants.Tag)
            .RequireAuthorization();

        // خانواده پرداخت می‌کند
        group.MapPostEndpoint<CreateOrderResponse>(BillingConstants.Routes.Create,  Post,        BillingConstants.Names.Create,  BillingConstants.Docs.Create.Summary,  BillingConstants.Docs.Create.Description) .RequireRoles(CareRoles.FamilyOrAdmin);
        group.MapPostEndpoint<bool>(               BillingConstants.Routes.Confirm, PostConfirm, BillingConstants.Names.Confirm, BillingConstants.Docs.Confirm.Summary, BillingConstants.Docs.Confirm.Description).RequireRoles(CareRoles.FamilyOrAdmin);

        // آزادسازی و عودت وجه تصمیم مالی مؤسسه/پلتفرم است
        group.MapPutEndpoint(BillingConstants.Routes.Release, PutRelease, BillingConstants.Names.Release, BillingConstants.Docs.Release.Summary, BillingConstants.Docs.Release.Description).RequireRoles(CareRoles.AgencyStaff);
        group.MapPutEndpoint(BillingConstants.Routes.Refund,  PutRefund,  BillingConstants.Names.Refund,  BillingConstants.Docs.Refund.Summary,  BillingConstants.Docs.Refund.Description) .RequireRoles(CareRoles.AgencyStaff);

        // فاکتور را هم خانواده (برای بیمه) و هم مؤسسه لازم دارد
        group.MapPostEndpoint<string>(BillingConstants.Routes.Invoice, PostInvoice, BillingConstants.Names.Invoice, BillingConstants.Docs.Invoice.Summary, BillingConstants.Docs.Invoice.Description).RequireRoles(CareRoles.FamilyOrAgency);

        group.MapGetEndpoint<OrderResult>(                BillingConstants.Routes.GetById,    Get,        BillingConstants.Names.GetById,    BillingConstants.Docs.GetById.Summary,    BillingConstants.Docs.GetById.Description);
        group.MapGetEndpoint<OrderResult>(                BillingConstants.Routes.GetByShift, GetByShift, BillingConstants.Names.GetByShift, BillingConstants.Docs.GetByShift.Summary, BillingConstants.Docs.GetByShift.Description);
        group.MapGetEndpoint<PagedResult<OrderListItem>>( BillingConstants.Routes.GetPaged,   Gets,       BillingConstants.Names.GetPaged,   BillingConstants.Docs.GetPaged.Summary,   BillingConstants.Docs.GetPaged.Description);
    }

    private static async Task<CreateOrderResponse> Post([FromBody] CreateCareOrderCommand cmd, IUserContext user, ICommandBus bus, CancellationToken ct)
        => await bus.Dispatch<CreateOrderResponse>(cmd with { PayerUserId = user.UserId }, ct);

    private static async Task<bool> PostConfirm([FromBody] ConfirmPaymentCommand cmd, ICommandBus bus, CancellationToken ct)
        => await bus.Dispatch<bool>(cmd, ct);

    private static async Task<bool> PutRelease(long id, ICommandBus bus, CancellationToken ct)
        => await bus.Dispatch<bool>(new ReleaseOrderCommand(id), ct);

    private static async Task<bool> PutRefund(long id, [FromBody] RefundOrderCommand cmd, ICommandBus bus, CancellationToken ct)
        => await bus.Dispatch<bool>(cmd with { OrderId = id }, ct);

    private static async Task<string> PostInvoice(long id, ICommandBus bus, CancellationToken ct)
        => await bus.Dispatch<string>(new IssueInvoiceCommand(id), ct);

    private static async Task<OrderResult?> Get(long id, IQueryBus bus, CancellationToken ct)
        => await bus.DispatchAsync<OrderResult?>(new GetOrderQuery(id), ct);

    private static async Task<OrderResult?> GetByShift(long shiftId, IQueryBus bus, CancellationToken ct)
        => await bus.DispatchAsync<OrderResult?>(new GetOrderByShiftQuery(shiftId), ct);

    private static async Task<PagedResult<OrderListItem>> Gets([AsParameters] GetOrdersQuery query, IQueryBus bus, CancellationToken ct)
        => await bus.DispatchAsync<PagedResult<OrderListItem>>(query, ct);
}
