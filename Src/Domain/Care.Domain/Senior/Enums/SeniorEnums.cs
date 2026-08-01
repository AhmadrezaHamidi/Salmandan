namespace Care.Domain.Senior.Enums;

/// <summary>سطح وابستگی سالمند بر مبنای ADL — تعیین‌کننده نوع خدمت و قیمت</summary>
public enum DependencyLevel
{
    /// <summary>مستقل — فقط پایش و همراهی</summary>
    Independent = 1,

    /// <summary>نیمه‌وابسته — نیازمند کمک در برخی امور روزمره</summary>
    PartiallyDependent = 2,

    /// <summary>وابسته — نیازمند کمک در اکثر امور</summary>
    Dependent = 3,

    /// <summary>کاملاً وابسته / بستری</summary>
    FullyDependent = 4
}

/// <summary>نسبت عضو خانواده با سالمند</summary>
public enum FamilyRelation
{
    Child = 1,
    Spouse = 2,
    Sibling = 3,
    Grandchild = 4,
    Other = 5
}

/// <summary>سطح دسترسی عضو خانواده به پرونده — کنترل حریم خصوصی داده سلامت</summary>
public enum FamilyAccessLevel
{
    /// <summary>مدیر پرونده — رزرو شیفت، پرداخت، مدیریت اعضا</summary>
    Manager = 1,

    /// <summary>ناظر — مشاهده گزارش‌ها و رزرو شیفت، بدون مدیریت اعضا</summary>
    Contributor = 2,

    /// <summary>فقط مشاهده گزارش‌ها</summary>
    Viewer = 3
}

/// <summary>وضعیت مصرف دارو</summary>
public enum MedicationFrequency
{
    OnceDaily = 1,
    TwiceDaily = 2,
    ThreeTimesDaily = 3,
    FourTimesDaily = 4,
    Weekly = 5,
    AsNeeded = 6
}
