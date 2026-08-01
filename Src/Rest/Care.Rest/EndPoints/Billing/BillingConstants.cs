namespace Care.Rest.EndPoints.Billing;

public static class BillingConstants
{
    public static class Routes
    {
        public const string BaseRoute  = ApiRoutes.Prefix + "/Orders";
        public const string Create     = "";
        public const string GetById    = "/{id}";
        public const string GetByShift = "/Shift/{shiftId}";
        public const string GetPaged   = "";
        public const string Confirm    = "/Confirm";
        public const string Release    = "/{id}/Release";
        public const string Refund     = "/{id}/Refund";
        public const string Invoice    = "/{id}/Invoice";
    }

    public static class Names
    {
        private const string P = "Order_";
        public const string Create     = P + "Create";
        public const string GetById    = P + "GetById";
        public const string GetByShift = P + "GetByShift";
        public const string GetPaged   = P + "GetPaged";
        public const string Confirm    = P + "ConfirmPayment";
        public const string Release    = P + "Release";
        public const string Refund     = P + "Refund";
        public const string Invoice    = P + "IssueInvoice";
    }

    public const string Tag = "Orders";

    public static class Docs
    {
        public static class Create     { public const string Summary = "ایجاد سفارش پرداخت";  public const string Description = "ایجاد سفارش شیفت و دریافت لینک درگاه پرداخت"; }
        public static class GetById    { public const string Summary = "جزئیات سفارش";        public const string Description = "اطلاعات کامل سفارش شامل سهم مؤسسه و کمیسیون پلتفرم"; }
        public static class GetByShift { public const string Summary = "سفارش یک شیفت";       public const string Description = "سفارش پرداخت مربوط به یک شیفت"; }
        public static class GetPaged   { public const string Summary = "لیست سفارش‌ها";        public const string Description = "لیست صفحه‌بندی‌شده با فیلتر سالمند، مؤسسه و وضعیت"; }
        public static class Confirm    { public const string Summary = "تأیید پرداخت";        public const string Description = "تأیید پرداخت پس از بازگشت از درگاه — وجه وارد امانت می‌شود"; }
        public static class Release    { public const string Summary = "آزادسازی وجه";        public const string Description = "آزادسازی وجه امانی به مؤسسه پس از تأیید گزارش شیفت"; }
        public static class Refund     { public const string Summary = "عودت وجه";            public const string Description = "عودت وجه به خانواده در صورت لغو یا عدم حضور"; }
        public static class Invoice    { public const string Summary = "صدور فاکتور";         public const string Description = "صدور فاکتور رسمی قابل ارائه به بیمه تکمیلی"; }
    }
}
