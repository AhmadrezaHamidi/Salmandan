namespace Care.Rest.EndPoints.Senior;

public static class SeniorConstants
{
    public static class Routes
    {
        public const string BaseRoute       = ApiRoutes.Prefix + "/Seniors";
        public const string Create          = "";
        public const string GetById         = "/{id}";
        public const string GetMine         = "/Mine";
        public const string Update          = "/{id}";
        public const string Dependency      = "/{id}/DependencyLevel";
        public const string EmergencyCard   = "/{id}/EmergencyCard";

        public const string FamilyCircle    = "/{id}/Family";
        public const string AddFamily       = "/{id}/Family";
        public const string RemoveFamily    = "/{id}/Family/{memberUserId}";
        public const string ChangeAccess    = "/{id}/Family/{memberUserId}/Access";

        public const string AddCondition    = "/{id}/Conditions";
        public const string RemoveCondition = "/{id}/Conditions/{conditionId}";
        public const string AddMedication   = "/{id}/Medications";
        public const string StopMedication  = "/{id}/Medications/{medicationId}";
        public const string AddAllergy      = "/{id}/Allergies";
        public const string AddContact      = "/{id}/EmergencyContacts";
    }

    public static class Names
    {
        private const string P = "Senior_";
        public const string Create          = P + "Create";
        public const string GetById         = P + "GetById";
        public const string GetMine         = P + "GetMine";
        public const string Update          = P + "Update";
        public const string Dependency      = P + "ChangeDependencyLevel";
        public const string EmergencyCard   = P + "EmergencyCard";
        public const string FamilyCircle    = P + "FamilyCircle";
        public const string AddFamily       = P + "AddFamilyMember";
        public const string RemoveFamily    = P + "RemoveFamilyMember";
        public const string ChangeAccess    = P + "ChangeFamilyAccess";
        public const string AddCondition    = P + "AddCondition";
        public const string RemoveCondition = P + "RemoveCondition";
        public const string AddMedication   = P + "AddMedication";
        public const string StopMedication  = P + "DiscontinueMedication";
        public const string AddAllergy      = P + "AddAllergy";
        public const string AddContact      = P + "AddEmergencyContact";
    }

    public const string Tag = "Seniors";

    public static class Docs
    {
        public static class Create          { public const string Summary = "ایجاد پرونده سالمند";   public const string Description = "ایجاد پرونده مراقبت؛ سازنده خودکار مدیر حلقه خانواده می‌شود"; }
        public static class GetById         { public const string Summary = "پرونده کامل سالمند";    public const string Description = "پرونده کامل شامل بیماری‌ها، داروها، حساسیت‌ها و حلقه خانواده"; }
        public static class GetMine         { public const string Summary = "سالمندان من";           public const string Description = "سالمندانی که کاربر جاری به آن‌ها دسترسی دارد"; }
        public static class Update          { public const string Summary = "بروزرسانی پرونده";      public const string Description = "بروزرسانی نام، آدرس و مختصات منزل"; }
        public static class Dependency      { public const string Summary = "تغییر سطح وابستگی";     public const string Description = "تغییر سطح وابستگی سالمند بر مبنای ADL"; }
        public static class EmergencyCard   { public const string Summary = "کارت اورژانس";          public const string Description = "خلاصه حیاتی پرونده برای ارائه به اورژانس یا بیمارستان"; }
        public static class FamilyCircle    { public const string Summary = "حلقه خانواده";          public const string Description = "لیست اعضای خانواده و سطح دسترسی هرکدام"; }
        public static class AddFamily       { public const string Summary = "افزودن عضو خانواده";    public const string Description = "افزودن عضو با شماره موبایل؛ حساب کاربری در صورت نبود ساخته می‌شود"; }
        public static class RemoveFamily    { public const string Summary = "حذف عضو خانواده";       public const string Description = "حذف عضو از حلقه خانواده"; }
        public static class ChangeAccess    { public const string Summary = "تغییر سطح دسترسی";      public const string Description = "تغییر سطح دسترسی عضو حلقه خانواده"; }
        public static class AddCondition    { public const string Summary = "افزودن بیماری";         public const string Description = "افزودن بیماری یا شرایط پزشکی به پرونده"; }
        public static class RemoveCondition { public const string Summary = "حذف بیماری";            public const string Description = "حذف بیماری از پرونده"; }
        public static class AddMedication   { public const string Summary = "افزودن دارو";           public const string Description = "افزودن داروی جاری به پرونده — مبنای چک‌لیست شیفت"; }
        public static class StopMedication  { public const string Summary = "قطع دارو";              public const string Description = "قطع مصرف دارو"; }
        public static class AddAllergy      { public const string Summary = "افزودن حساسیت";         public const string Description = "افزودن حساسیت دارویی یا غذایی"; }
        public static class AddContact      { public const string Summary = "افزودن تماس اضطراری";   public const string Description = "افزودن تماس اضطراری با اولویت"; }
    }
}
