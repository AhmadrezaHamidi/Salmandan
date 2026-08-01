namespace Care.Rest.EndPoints.Report;

public static class ReportConstants
{
    public static class Routes
    {
        public const string BaseRoute   = ApiRoutes.Prefix + "/ShiftReports";
        public const string Submit      = "";
        public const string GetById     = "/{id}";
        public const string GetByShift  = "/Shift/{shiftId}";
        public const string GetBySenior = "/Senior/{seniorId}";
        public const string Timeline    = "/Senior/{seniorId}/Timeline";
        public const string HealthRisk  = "/Senior/{seniorId}/HealthRisk";
        public const string Medication  = "/{id}/Medications";
        public const string Incident    = "/{id}/Incident";
        public const string Approve     = "/{id}/Approve";
    }

    public static class Names
    {
        private const string P = "ShiftReport_";
        public const string Submit      = P + "Submit";
        public const string GetById     = P + "GetById";
        public const string GetByShift  = P + "GetByShift";
        public const string GetBySenior = P + "GetBySenior";
        public const string Timeline    = P + "HealthTimeline";
        public const string HealthRisk  = P + "HealthRisk";
        public const string Medication  = P + "RecordMedicationIntake";
        public const string Incident    = P + "FlagIncident";
        public const string Approve     = P + "Approve";
    }

    public const string Tag = "ShiftReports";

    public static class Docs
    {
        public static class Submit      { public const string Summary = "ثبت گزارش شیفت";       public const string Description = "ثبت گزارش پایان شیفت توسط پرستار؛ داروهای فعال خودکار به چک‌لیست اضافه می‌شوند"; }
        public static class GetById     { public const string Summary = "جزئیات گزارش";         public const string Description = "گزارش کامل شامل علائم حیاتی و وضعیت مصرف داروها"; }
        public static class GetByShift  { public const string Summary = "گزارش یک شیفت";        public const string Description = "گزارش ثبت‌شده برای یک شیفت مشخص"; }
        public static class GetBySenior { public const string Summary = "گزارش‌های سالمند";      public const string Description = "لیست صفحه‌بندی‌شده گزارش‌های یک سالمند"; }
        public static class Timeline    { public const string Summary = "روند سلامت";           public const string Description = "روند شاخص‌های سلامت در N گزارش اخیر — خوراک نمودار اپ خانواده"; }
        public static class HealthRisk  { public const string Summary = "تحلیل ریسک سلامت";     public const string Description = "امتیاز ریسک، عوامل مؤثر و خلاصه فارسی وضعیت سالمند"; }
        public static class Medication  { public const string Summary = "ثبت مصرف دارو";        public const string Description = "ثبت یا بروزرسانی وضعیت مصرف یک دارو در شیفت"; }
        public static class Incident    { public const string Summary = "ثبت حادثه";            public const string Description = "ثبت حادثه در شیفت — اطلاع فوری به خانواده"; }
        public static class Approve     { public const string Summary = "تأیید گزارش";          public const string Description = "تأیید گزارش توسط خانواده — پیش‌شرط آزادسازی وجه"; }
    }
}
