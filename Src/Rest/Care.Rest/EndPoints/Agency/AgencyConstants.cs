namespace Care.Rest.EndPoints.Agency;

public static class AgencyConstants
{
    public static class Routes
    {
        public const string BaseRoute = ApiRoutes.Prefix + "/Agencies";
        public const string Register  = "";
        public const string GetById   = "/{id}";
        public const string GetPaged  = "";
        public const string Available = "/Available";
        public const string Approve   = "/{id}/Approve";
        public const string Reject    = "/{id}/Reject";
        public const string Suspend   = "/{id}/Suspend";
        public const string Renew     = "/{id}/License";
        public const string Update    = "/{id}";
        public const string Commission = "/{id}/Commission";
        public const string SetPrice  = "/{id}/Prices";
        public const string GetPrices = "/{id}/Prices";
        public const string Balance   = "/{id}/Balance";
    }

    public static class Names
    {
        private const string P = "Agency_";
        public const string Register   = P + "Register";
        public const string GetById    = P + "GetById";
        public const string GetPaged   = P + "GetPaged";
        public const string Available  = P + "Available";
        public const string Approve    = P + "Approve";
        public const string Reject     = P + "Reject";
        public const string Suspend    = P + "Suspend";
        public const string Renew      = P + "RenewLicense";
        public const string Update     = P + "Update";
        public const string Commission = P + "SetCommission";
        public const string SetPrice   = P + "SetPrice";
        public const string GetPrices  = P + "GetPrices";
        public const string Balance    = P + "Balance";
    }

    public const string Tag = "Agencies";

    public static class Docs
    {
        public static class Register   { public const string Summary = "ثبت مؤسسه";            public const string Description = "ثبت مؤسسه پرستاری جدید در انتظار تأیید ادمین"; }
        public static class GetById    { public const string Summary = "جزئیات مؤسسه";         public const string Description = "اطلاعات کامل یک مؤسسه پرستاری"; }
        public static class GetPaged   { public const string Summary = "لیست مؤسسات";          public const string Description = "لیست صفحه‌بندی‌شده مؤسسات با فیلتر شهر و وضعیت"; }
        public static class Available  { public const string Summary = "مؤسسات در دسترس";      public const string Description = "مؤسسات فعال با مجوز معتبر که برای این خدمت تعرفه دارند"; }
        public static class Approve    { public const string Summary = "تأیید مؤسسه";          public const string Description = "تأیید مؤسسه پس از بررسی مجوز وزارت بهداشت"; }
        public static class Reject     { public const string Summary = "رد مؤسسه";             public const string Description = "رد درخواست ثبت مؤسسه"; }
        public static class Suspend    { public const string Summary = "تعلیق مؤسسه";          public const string Description = "تعلیق مؤسسه به دلیل تخلف یا انقضای مجوز"; }
        public static class Renew      { public const string Summary = "تمدید مجوز";           public const string Description = "بروزرسانی شماره و تاریخ انقضای مجوز"; }
        public static class Update     { public const string Summary = "بروزرسانی مؤسسه";      public const string Description = "بروزرسانی اطلاعات تماس و آدرس"; }
        public static class Commission { public const string Summary = "تعیین کمیسیون";        public const string Description = "تعیین درصد کمیسیون پلتفرم برای مؤسسه"; }
        public static class SetPrice   { public const string Summary = "ثبت تعرفه";            public const string Description = "ثبت یا بروزرسانی قیمت یک نوع خدمت و شیفت"; }
        public static class GetPrices  { public const string Summary = "لیست تعرفه‌ها";         public const string Description = "تعرفه‌های مصوب مؤسسه"; }
        public static class Balance    { public const string Summary = "وضعیت مالی مؤسسه";     public const string Description = "مبالغ در امانت، آزادشده و کمیسیون پلتفرم"; }
    }
}
