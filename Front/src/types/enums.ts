export type UserRole = "Admin" | "Family" | "AgencyManager" | "Caregiver";
export type UserStatus = "Active" | "Inactive" | "Suspended";

export type AgencyStatus = "Pending" | "Active" | "Suspended" | "Rejected";
export type CareServiceType = "Companionship" | "PersonalCare" | "ClinicalNursing" | "Physiotherapy";

export type CaregiverLevel = "Companion" | "CareAssistant" | "PracticalNurse" | "RegisteredNurse";
export type CaregiverStatus = "Pending" | "Verified" | "Rejected" | "Suspended";
export type CaregiverSkillType =
  | "Alzheimer" | "Parkinson" | "Bedridden" | "Injection"
  | "Diabetes" | "Physiotherapy" | "PostStroke" | "Respiratory";

export type DependencyLevel = "Independent" | "PartiallyDependent" | "Dependent" | "FullyDependent";
export type FamilyRelation = "Child" | "Spouse" | "Sibling" | "Grandchild" | "Other";
export type FamilyAccessLevel = "Manager" | "Contributor" | "Viewer";
export type MedicationFrequency =
  | "OnceDaily" | "TwiceDaily" | "ThreeTimesDaily" | "FourTimesDaily" | "Weekly" | "AsNeeded";

export type ShiftKind = "Hourly" | "Daily" | "Night" | "FullDay" | "LiveIn";
export type ShiftStatus =
  | "Requested" | "Assigned" | "CheckedIn" | "CheckedOut" | "Completed" | "Cancelled" | "NoShow";
export type CancelActor = "Family" | "Agency" | "Platform";

export type ObservationScale = "Poor" | "Fair" | "Good";
export type MobilityLevel = "Bedridden" | "Assisted" | "WithAid" | "Independent";
export type IncidentType =
  | "Fall" | "MissedMedication" | "HealthDeterioration" | "Hospitalization" | "Agitation" | "Other";

export type OrderStatus = "Pending" | "Escrowed" | "Released" | "Refunded" | "Failed";
export type PayoutStatus = "Pending" | "Paid" | "Cancelled";

export type Gender = "Male" | "Female";

export const roleLabels: Record<UserRole, string> = {
  Admin: "مدیر سیستم",
  Family: "خانواده",
  AgencyManager: "مدیر آژانس",
  Caregiver: "پرستار",
};

export const shiftStatusLabels: Record<ShiftStatus, string> = {
  Requested: "درخواست‌شده",
  Assigned: "تخصیص‌یافته",
  CheckedIn: "شروع‌شده",
  CheckedOut: "پایان‌یافته",
  Completed: "تکمیل‌شده",
  Cancelled: "لغوشده",
  NoShow: "عدم‌حضور",
};

export const shiftStatusColor: Record<ShiftStatus, string> = {
  Requested: "gray",
  Assigned: "blue",
  CheckedIn: "amber",
  CheckedOut: "amber",
  Completed: "green",
  Cancelled: "red",
  NoShow: "red",
};

export const agencyStatusLabels: Record<AgencyStatus, string> = {
  Pending: "در انتظار تایید",
  Active: "فعال",
  Suspended: "معلق",
  Rejected: "ردشده",
};

export const caregiverStatusLabels: Record<CaregiverStatus, string> = {
  Pending: "در انتظار تایید",
  Verified: "تاییدشده",
  Rejected: "ردشده",
  Suspended: "معلق",
};

export const orderStatusLabels: Record<OrderStatus, string> = {
  Pending: "در انتظار پرداخت",
  Escrowed: "پرداخت‌شده (امانی)",
  Released: "تسویه‌شده",
  Refunded: "بازگشت‌داده‌شده",
  Failed: "ناموفق",
};

export const payoutStatusLabels: Record<PayoutStatus, string> = {
  Pending: "در انتظار پرداخت",
  Paid: "پرداخت‌شده",
  Cancelled: "لغوشده",
};

export const dependencyLevelLabels: Record<DependencyLevel, string> = {
  Independent: "مستقل",
  PartiallyDependent: "نیمه‌وابسته",
  Dependent: "وابسته",
  FullyDependent: "کاملاً وابسته",
};

export const careServiceTypeLabels: Record<CareServiceType, string> = {
  Companionship: "همراهی",
  PersonalCare: "مراقبت شخصی",
  ClinicalNursing: "پرستاری بالینی",
  Physiotherapy: "فیزیوتراپی",
};

export const shiftKindLabels: Record<ShiftKind, string> = {
  Hourly: "ساعتی",
  Daily: "روزانه",
  Night: "شبانه",
  FullDay: "تمام‌روز",
  LiveIn: "اقامتی",
};

// The backend deserializes enum fields in command/request bodies as their numeric
// ordinal (default System.Text.Json behavior), while query results return them as
// strings. These maps mirror the C# enum declarations exactly for write payloads.
export const genderOrdinal: Record<Gender, number> = { Male: 1, Female: 2 };

export const dependencyLevelOrdinal: Record<DependencyLevel, number> = {
  Independent: 1, PartiallyDependent: 2, Dependent: 3, FullyDependent: 4,
};

export const familyRelationOrdinal: Record<FamilyRelation, number> = {
  Child: 1, Spouse: 2, Sibling: 3, Grandchild: 4, Other: 5,
};

export const familyAccessLevelOrdinal: Record<FamilyAccessLevel, number> = {
  Manager: 1, Contributor: 2, Viewer: 3,
};

export const medicationFrequencyOrdinal: Record<MedicationFrequency, number> = {
  OnceDaily: 1, TwiceDaily: 2, ThreeTimesDaily: 3, FourTimesDaily: 4, Weekly: 5, AsNeeded: 6,
};

export const careServiceTypeOrdinal: Record<CareServiceType, number> = {
  Companionship: 1, PersonalCare: 2, ClinicalNursing: 3, Physiotherapy: 4,
};

export const shiftKindOrdinal: Record<ShiftKind, number> = {
  Hourly: 1, Daily: 2, Night: 3, FullDay: 4, LiveIn: 5,
};

export const cancelActorOrdinal: Record<CancelActor, number> = { Family: 1, Agency: 2, Platform: 3 };

export const caregiverLevelOrdinal: Record<CaregiverLevel, number> = {
  Companion: 1, CareAssistant: 2, PracticalNurse: 3, RegisteredNurse: 4,
};

export const caregiverSkillTypeOrdinal: Record<CaregiverSkillType, number> = {
  Alzheimer: 1, Parkinson: 2, Bedridden: 3, Injection: 4,
  Diabetes: 5, Physiotherapy: 6, PostStroke: 7, Respiratory: 8,
};
