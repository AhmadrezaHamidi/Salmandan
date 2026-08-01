namespace Care.Rest.EndPoints.Caregiver;

public static class CaregiverConstants
{
    public static class Routes
    {
        public const string BaseRoute   = ApiRoutes.Prefix + "/Caregivers";
        public const string Register    = "/Agency/{agencyId}";
        public const string GetById     = "/{id}";
        public const string GetPaged    = "";
        public const string Available   = "/Available";
        public const string Documents   = "/{id}/Documents";
        public const string Verify      = "/{id}/Verify";
        public const string Reject      = "/{id}/Reject";
        public const string Suspend     = "/{id}/Suspend";
        public const string Update      = "/{id}";
        public const string AddSkill    = "/{id}/Skills";
        public const string RemoveSkill = "/{id}/Skills/{skillType}";
    }

    public static class Names
    {
        private const string P = "Caregiver_";
        public const string Register    = P + "Register";
        public const string GetById     = P + "GetById";
        public const string GetPaged    = P + "GetPaged";
        public const string Available   = P + "Available";
        public const string Documents   = P + "SubmitDocuments";
        public const string Verify      = P + "Verify";
        public const string Reject      = P + "Reject";
        public const string Suspend     = P + "Suspend";
        public const string Update      = P + "Update";
        public const string AddSkill    = P + "AddSkill";
        public const string RemoveSkill = P + "RemoveSkill";
    }

    public const string Tag = "Caregivers";

    public static class Docs
    {
        public static class Register    { public const string Summary = "ثبت پرستار";           public const string Description = "ثبت پرستار جدید توسط مؤسسه در انتظار احراز صلاحیت"; }
        public static class GetById     { public const string Summary = "جزئیات پرستار";        public const string Description = "اطلاعات کامل پرستار همراه وضعیت مدارک"; }
        public static class GetPaged    { public const string Summary = "لیست پرستاران";        public const string Description = "لیست صفحه‌بندی‌شده با فیلتر مؤسسه و وضعیت"; }
        public static class Available   { public const string Summary = "پرستاران آزاد";        public const string Description = "پرستاران تأییدشده بدون شیفت هم‌پوشان در بازه خواسته‌شده"; }
        public static class Documents   { public const string Summary = "ثبت مدارک";            public const string Description = "ثبت گواهی عدم سوءپیشینه، مدرک و بیمه مسئولیت"; }
        public static class Verify      { public const string Summary = "تأیید صلاحیت";         public const string Description = "تأیید صلاحیت پرستار — نیازمند گواهی عدم سوءپیشینه"; }
        public static class Reject      { public const string Summary = "رد صلاحیت";            public const string Description = "رد صلاحیت پرستار"; }
        public static class Suspend     { public const string Summary = "تعلیق پرستار";         public const string Description = "تعلیق پرستار"; }
        public static class Update      { public const string Summary = "بروزرسانی پرستار";     public const string Description = "بروزرسانی نام و سطح صلاحیت"; }
        public static class AddSkill    { public const string Summary = "افزودن مهارت";         public const string Description = "افزودن مهارت تخصصی به پرونده پرستار"; }
        public static class RemoveSkill { public const string Summary = "حذف مهارت";            public const string Description = "حذف مهارت از پرونده پرستار"; }
    }
}
