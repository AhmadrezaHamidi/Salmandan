# سالمندان — پلتفرم مراقبت از سالمندان

بک‌اند پلتفرم مراقبت از سالمندان با معماری DDD + CQRS روی .NET 8 — هم‌ساختار با `DentV2`.

**مدل کسب‌وکار:** Aggregator مؤسسات پرستاری + اپ خانواده.
پلتفرم خودش اعزام‌کننده نیرو نیست؛ عرضه از مؤسسات دارای مجوز وزارت بهداشت تأمین می‌شود
و پلتفرم لایه‌ی اعتماد، هماهنگی و هوش را می‌سازد.

---

## ساختار پروژه

```
Src/
├── Domain/
│   ├── Care.Domain            ← Aggregate ها، Args، Enums، Exceptions، Repository interfaces
│   └── Care.Event             ← Domain Event ها
├── Application/
│   ├── Care.Application.Contract        ← Command ها + قراردادهای سرویس
│   ├── Care.Application                 ← Command Handler ها
│   ├── Care.Application.Query.Contract  ← Query ها + Result ها
│   ├── Care.Application.Query           ← Query Handler ها
│   ├── Care.Application.Validators      ← FluentValidation + ValidationBehavior
│   └── Care.Event.Handler               ← Event Handler ها
├── Infrastructure/
│   ├── Care.Persistence.EF    ← CareDbContext، Configurations، Repositories، JWT، درگاه پرداخت
│   ├── Care.Read.Dapper       ← Read Repository ها (سمت Query)
│   └── Care.Config            ← AutofacModule، Middlewares، Swagger
├── Rest/Care.Rest             ← Minimal API Endpoints
└── Host/
    ├── Care.Option
    └── Care.ServiceHost       ← نقطه شروع اپلیکیشن
Test/Api.E2E.Tests.ps1         ← تست end-to-end روی API واقعی
```

> نام اسمبلی‌ها باید از الگوی `{CompanyName}.{ContextName}.{Layer}` پیروی کند
> (`Salmandan.Care.Application` و…) چون `AhmadBase.Config` اسمبلی‌ها را با همین قرارداد
> reflection می‌کند. تغییر `CompanyName` یا `ContextName` در `appsettings.json`
> بدون تغییر `AssemblyName` باعث خطای زمان اجرا می‌شود.

---

## Aggregate ها

| Aggregate | مسئولیت |
|---|---|
| `User` | احراز هویت OTP، نقش‌ها (Admin / Family / AgencyManager / Caregiver) |
| `CareAgency` | مؤسسه پرستاری، مجوز وزارت بهداشت، تعرفه خدمات، کمیسیون |
| `CaregiverProfile` | پرستار، احراز صلاحیت، مهارت‌ها، شمارنده عدم حضور |
| `SeniorProfile` | پرونده مراقبت + حلقه خانواده + داروها/بیماری‌ها/حساسیت‌ها |
| `CareShift` | چرخه حیات شیفت: Requested → Assigned → CheckedIn → CheckedOut → Completed |
| `ShiftReport` | گزارش ساختاریافته پایان شیفت + امتیاز ریسک |
| `CareOrder` | پرداخت امانی (Escrow): Pending → Escrowed → Released / Refunded |

### قواعد دامین که در تست پوشش داده شده‌اند

- **احراز صلاحیت پرستار** بدون گواهی عدم سوءپیشینه ممکن نیست.
- **چک‌این ژئوفنس**: ثبت ورود فقط در شعاع ۲۰۰ متری منزل سالمند.
- **قیمت شیفت** از تعرفه مصوب مؤسسه خوانده می‌شود، نه از ورودی کاربر.
- **تکمیل شیفت** پیش از ثبت گزارش پایان شیفت مجاز نیست.
- **تخصیص هم‌پوشان** پرستار در یک بازه زمانی رد می‌شود.
- **حلقه خانواده**: آخرین مدیر پرونده قابل حذف یا تنزل نیست.
- **دسترسی داده سلامت**: فقط اعضای حلقه خانواده پرونده را می‌بینند.
- **عدم حضور**: ۳ بار → تعلیق خودکار پرستار.
- **شیفت جایگزین** با همان مبلغ ساخته می‌شود و به شیفت اصلی لینک است.

---

## Endpoint ها

| گروه | مسیر پایه |
|---|---|
| احراز هویت | `POST /api/v1/Auth/{SendOtp, VerifyOtp, Refresh, Logout}` · `PUT /Auth/Profile` |
| مؤسسات | `/api/v1/Agencies` — ثبت، تأیید، تعلیق، تمدید مجوز، تعرفه، وضعیت مالی |
| پرستاران | `/api/v1/Caregivers` — ثبت، مدارک، احراز صلاحیت، مهارت، پرستاران آزاد |
| سالمندان | `/api/v1/Seniors` — پرونده، حلقه خانواده، دارو، بیماری، حساسیت، کارت اورژانس |
| شیفت‌ها | `/api/v1/Shifts` — درخواست، تخصیص، چک‌این/اوت، تکمیل، لغو، عدم حضور، جایگزین، ریسک پوشش |
| گزارش شیفت | `/api/v1/ShiftReports` — ثبت، مصرف دارو، حادثه، تأیید، تایم‌لاین، تحلیل ریسک |
| پرداخت | `/api/v1/Orders` — ایجاد، تأیید پرداخت، آزادسازی، عودت، فاکتور |

مستندات کامل روی Swagger: `http://localhost:5100/swagger`

---

## اجرا

```bash
dotnet build Salmandan.sln
```

```bash
dotnet ef database update --project Src/Infrastructure/Care.Persistence.EF --startup-project Src/Host/Care.ServiceHost
```

```bash
dotnet run --project Src/Host/Care.ServiceHost --urls http://localhost:5100
```

در حالت `UseFakeServices: true` پیامک ارسال نمی‌شود و کد OTP در پاسخ
`SendOtp` با پیشوند `SMS_UNAVAILABLE:` برمی‌گردد.

## تست

```bash
pwsh Test/Api.E2E.Tests.ps1
```

۵۹ سناریوی end-to-end روی API واقعی و دیتابیس واقعی؛ شامل edge case ها
(کد OTP اشتباه، ورودی نامعتبر، چک‌این خارج از محدوده، دسترسی غیرمجاز، تداخل زمانی).

---

## نکات فنی

- **Dapper و `DateOnly`/`TimeOnly`**: Dapper این نوع‌ها را به‌صورت پیش‌فرض نگاشت نمی‌کند.
  `DapperTypeHandlers.Register()` در `AutofacModule` این شکاف را پر می‌کند.
- **خطای اعتبارسنجی**: `ValidationBehavior` خطاها را به `InputValidationException`
  (زیرمجموعه `BusinessException`) تبدیل می‌کند تا ProblemDetails فریم‌ورک پاسخ ۴۲۲
  با پیام فارسی بدهد؛ `ValidationException` خام به ۵۰۰ تبدیل می‌شد.
- **شناسه‌ها**: Aggregate Root ها از HiLo و موجودیت‌های فرزند از `max(id)+1` می‌گیرند
  (HiLo فقط برای Aggregate Root تعریف شده است).

## کارهای باقی‌مانده

- کنترل نقش روی endpoint ها (فعلاً فقط `RequireAuthorization`؛ مثلاً تأیید مؤسسه باید Admin-only باشد)
- اتصال Event Handler ها به پیامک و اعلان واقعی (فعلاً فقط لاگ)
- اشتراک خانواده و پنل SaaS مؤسسات
- همدم صوتی فارسی و تحلیل ریسک مبتنی بر یادگیری ماشین
