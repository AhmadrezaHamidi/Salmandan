namespace Care.Rest.EndPoints.Shift;

public static class ShiftConstants
{
    public static class Routes
    {
        public const string BaseRoute    = ApiRoutes.Prefix + "/Shifts";
        public const string Request      = "";
        public const string GetById      = "/{id}";
        public const string GetPaged     = "";
        public const string Calendar     = "/Calendar";
        public const string CoverageRisk = "/CoverageRisk";
        public const string Assign       = "/{id}/Assign";
        public const string CheckIn      = "/{id}/CheckIn";
        public const string CheckOut     = "/{id}/CheckOut";
        public const string Complete     = "/{id}/Complete";
        public const string Cancel       = "/{id}/Cancel";
        public const string NoShow       = "/{id}/NoShow";
        public const string Replace      = "/{id}/Replace";
        public const string CarePlan     = "/{id}/CarePlan";
    }

    public static class Names
    {
        private const string P = "Shift_";
        public const string Request      = P + "Request";
        public const string GetById      = P + "GetById";
        public const string GetPaged     = P + "GetPaged";
        public const string Calendar     = P + "Calendar";
        public const string CoverageRisk = P + "CoverageRisk";
        public const string Assign       = P + "AssignCaregiver";
        public const string CheckIn      = P + "CheckIn";
        public const string CheckOut     = P + "CheckOut";
        public const string Complete     = P + "Complete";
        public const string Cancel       = P + "Cancel";
        public const string NoShow       = P + "MarkNoShow";
        public const string Replace      = P + "Replace";
        public const string CarePlan     = P + "CarePlan";
    }

    public const string Tag = "Shifts";

    public static class Docs
    {
        public static class Request      { public const string Summary = "درخواست شیفت";        public const string Description = "ثبت درخواست شیفت مراقبت؛ قیمت از تعرفه مصوب مؤسسه خوانده می‌شود"; }
        public static class GetById      { public const string Summary = "جزئیات شیفت";         public const string Description = "اطلاعات کامل شیفت با نام سالمند، مؤسسه و پرستار"; }
        public static class GetPaged     { public const string Summary = "لیست شیفت‌ها";         public const string Description = "لیست صفحه‌بندی‌شده با فیلتر سالمند، مؤسسه، پرستار، وضعیت و بازه تاریخ"; }
        public static class Calendar     { public const string Summary = "تقویم شیفت";          public const string Description = "شیفت‌های یک سالمند در بازه تاریخ"; }
        public static class CoverageRisk { public const string Summary = "ریسک پوشش شیفت";      public const string Description = "شیفت‌های بدون پرستار که تا N ساعت آینده شروع می‌شوند"; }
        public static class Assign       { public const string Summary = "تخصیص پرستار";        public const string Description = "تخصیص پرستار تأییدشده و بدون تداخل زمانی به شیفت"; }
        public static class CheckIn      { public const string Summary = "ثبت ورود";            public const string Description = "ثبت ورود پرستار با کنترل فاصله تا منزل سالمند (ژئوفنس)"; }
        public static class CheckOut     { public const string Summary = "ثبت خروج";            public const string Description = "ثبت خروج پرستار؛ پس از آن گزارش شیفت الزامی است"; }
        public static class Complete     { public const string Summary = "تکمیل شیفت";          public const string Description = "تکمیل شیفت و آماده‌سازی آزادسازی وجه"; }
        public static class Cancel       { public const string Summary = "لغو شیفت";            public const string Description = "لغو شیفت توسط خانواده، مؤسسه یا پلتفرم"; }
        public static class NoShow       { public const string Summary = "ثبت عدم حضور";        public const string Description = "ثبت عدم حضور پرستار — آغاز فرآیند تضمین پوشش شیفت"; }
        public static class Replace      { public const string Summary = "شیفت جایگزین";        public const string Description = "ساخت شیفت جایگزین با پرستار دیگر و همان مبلغ"; }
        public static class CarePlan     { public const string Summary = "پرونده مراقبت شیفت";  public const string Description = "داروها، بیماری‌ها، حساسیت‌ها و تماس اضطراری سالمند — فقط برای پرستارِ همین شیفت"; }
    }
}
