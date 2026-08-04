namespace Care.Rest.EndPoints.Payroll;

public static class PayrollConstants
{
    public static class Routes
    {
        public const string BaseRoute = ApiRoutes.Prefix + "/Payouts";
        public const string Generate  = "/Generate";
        public const string Preview   = "/Preview";
        public const string Profit    = "/Profit";
        public const string GetById   = "/{id}";
        public const string GetPaged  = "";
        public const string MarkPaid  = "/{id}/Pay";
        public const string Cancel    = "/{id}/Cancel";
    }

    public static class Names
    {
        private const string P = "Payout_";
        public const string Generate = P + "GeneratePayroll";
        public const string Preview  = P + "PreviewPayroll";
        public const string Profit   = P + "AgencyProfit";
        public const string GetById  = P + "GetById";
        public const string GetPaged = P + "GetPaged";
        public const string MarkPaid = P + "MarkPaid";
        public const string Cancel   = P + "Cancel";
    }

    public const string Tag = "Payroll";

    public static class Docs
    {
        public static class Generate { public const string Summary = "صدور فیش حقوقی";      public const string Description = "محاسبه حقوق پرستاران از شیفت‌های تکمیل‌شده و قفل کردن آن‌ها تا پرداخت دوباره نشوند"; }
        public static class Preview  { public const string Summary = "پیش‌نمایش حقوق";      public const string Description = "چقدر باید پرداخت شود و بابت چند شیفت — بدون قفل کردن چیزی"; }
        public static class Profit   { public const string Summary = "سود و زیان مؤسسه";    public const string Description = "درآمد از خانواده منهای دستمزد پرستار و کمیسیون پلتفرم در یک بازه"; }
        public static class GetById  { public const string Summary = "جزئیات فیش";          public const string Description = "فیش حقوقی همراه ریز شیفت‌ها و ساعت کارکرد واقعی"; }
        public static class GetPaged { public const string Summary = "لیست فیش‌ها";          public const string Description = "لیست صفحه‌بندی‌شده با فیلتر مؤسسه، پرستار و وضعیت"; }
        public static class MarkPaid { public const string Summary = "ثبت واریز";           public const string Description = "ثبت پرداخت حقوق به پرستار با شماره پیگیری"; }
        public static class Cancel   { public const string Summary = "لغو فیش";             public const string Description = "لغو فیش پرداخت‌نشده — شیفت‌هایش دوباره قابل محاسبه می‌شوند"; }
    }
}
