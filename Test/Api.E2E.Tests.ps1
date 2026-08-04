$ErrorActionPreference = "Stop"
$base = "http://localhost:5100/api/v1"
$pass = 0; $fail = 0
function Step($name, $block) {
    try { $r = & $block; Write-Host "PASS  $name" -ForegroundColor Green; $script:pass++; return $r }
    catch {
        $msg = $_.Exception.Message
        try { $sr = New-Object IO.StreamReader($_.Exception.Response.GetResponseStream()); $msg = $sr.ReadToEnd() } catch {}
        Write-Host "FAIL  $name -> $msg" -ForegroundColor Red; $script:fail++; return $null
    }
}
function Login($phone) {
    $otp = Invoke-RestMethod "$base/Auth/SendOtp" -Method Post -ContentType "application/json; charset=utf-8" -Body (@{phoneNumber=$phone} | ConvertTo-Json)
    $code = $otp.message.Split(':')[1]
    $login = Invoke-RestMethod "$base/Auth/VerifyOtp" -Method Post -ContentType "application/json; charset=utf-8" -Body (@{phoneNumber=$phone;code=$code} | ConvertTo-Json)
    return $login
}
function Hdr($token) { return @{ Authorization = "Bearer $token" } }

$rand  = Get-Random -Minimum 1000 -Maximum 9999
$today = (Get-Date).ToString("yyyy-MM-dd")

Write-Host "`n=== 1. Identity / OTP ===" -ForegroundColor Cyan
$family = Step "ورود عضو خانواده با OTP" { Login "0912000$rand" }
$owner  = Step "ورود مدیر مؤسسه با OTP"  { Login "0913000$rand" }
$nurse  = Step "ورود پرستار با OTP"       { Login "0914000$rand" }
$admin  = Step "ورود ادمین پلتفرم (شماره از تنظیمات)" { Login "09120000000" }
Step "نقش ادمین از تنظیمات آمد نه از ورودی کلاینت" { if ($admin.role -ne "Admin") { throw "role=$($admin.role)" }; $true }
Step "کاربر عادی نقش Family می‌گیرد" { if ($family.role -ne "Family") { throw "role=$($family.role)" }; $true }
Step "توکن دریافت شد" { if (-not $family.accessToken) { throw "no token" }; $true }
Step "تمدید توکن (Refresh)" {
    Invoke-RestMethod "$base/Auth/Refresh" -Method Post -ContentType "application/json; charset=utf-8" -Body (@{refreshToken=$family.refreshToken} | ConvertTo-Json)
}
Step "کد OTP اشتباه رد می‌شود" {
    try { Invoke-RestMethod "$base/Auth/VerifyOtp" -Method Post -ContentType "application/json; charset=utf-8" -Body (@{phoneNumber="0912000$rand";code="00000"} | ConvertTo-Json); throw "SHOULD_FAIL" }
    catch { if ($_.Exception.Message -eq "SHOULD_FAIL") { throw "کد اشتباه پذیرفته شد" }; $true }
}
Step "شماره موبایل نامعتبر رد می‌شود (validator)" {
    try { Invoke-RestMethod "$base/Auth/SendOtp" -Method Post -ContentType "application/json; charset=utf-8" -Body (@{phoneNumber="123"} | ConvertTo-Json); throw "SHOULD_FAIL" }
    catch { if ($_.Exception.Message -eq "SHOULD_FAIL") { throw "شماره نامعتبر پذیرفته شد" }; $true }
}

Write-Host "`n=== 2. Agency (مؤسسه) ===" -ForegroundColor Cyan
$agencyId = Step "ثبت مؤسسه پرستاری" {
    Invoke-RestMethod "$base/Agencies" -Method Post -Headers (Hdr $owner.accessToken) -ContentType "application/json; charset=utf-8" -Body (@{
        name="مؤسسه مراقبت مهر $rand"; licenseNumber="LIC-$rand"; licenseExpiryDate="2027-12-31"
        city="تهران"; district="سعادت‌آباد"; address="خیابان نمونه"; phoneNumber="02144000000"
    } | ConvertTo-Json)
}
Step "تأیید مؤسسه توسط ادمین" { Invoke-RestMethod "$base/Agencies/$agencyId/Approve" -Method Put -Headers (Hdr $admin.accessToken) }
Step "تأیید دوباره رد می‌شود (قاعده دامین)" {
    try { Invoke-RestMethod "$base/Agencies/$agencyId/Approve" -Method Put -Headers (Hdr $admin.accessToken); throw "SHOULD_FAIL" }
    catch { if ($_.Exception.Message -eq "SHOULD_FAIL") { throw "تأیید تکراری پذیرفته شد" }; $true }
}
# ثبت مؤسسه، ثبت‌کننده را به AgencyManager ارتقا می‌دهد؛ توکن قبلی هنوز Family است
$owner = Step "ورود مجدد مالک پس از ارتقای نقش به مدیر مؤسسه" { Login "0913000$rand" }
Step "نقش مالک به AgencyManager تغییر کرد" { if ($owner.role -ne "AgencyManager") { throw "role=$($owner.role)" }; $true }
Step "ثبت تعرفه خدمت (PersonalCare/Daily)" {
    Invoke-RestMethod "$base/Agencies/$agencyId/Prices" -Method Post -Headers (Hdr $owner.accessToken) -ContentType "application/json; charset=utf-8" -Body (@{
        serviceType=2; shiftKind=2; price=3500000
    } | ConvertTo-Json)
}
Step "خواندن تعرفه‌ها" {
    $p = Invoke-RestMethod "$base/Agencies/$agencyId/Prices" -Headers (Hdr $owner.accessToken)
    if ($p.Count -lt 1) { throw "تعرفه ثبت نشد" }; $p
}
Step "جزئیات مؤسسه (Dapper read)" {
    $a = Invoke-RestMethod "$base/Agencies/$agencyId" -Headers (Hdr $owner.accessToken)
    if ($a.status -ne "Active") { throw "status=$($a.status)" }
    if (-not $a.hasValidLicense) { throw "مجوز نامعتبر" }; $a
}
Step "لیست مؤسسات در دسترس شهر تهران" {
    $l = Invoke-RestMethod "$base/Agencies/Available?city=تهران&serviceType=2" -Headers (Hdr $family.accessToken)
    if ($l.Count -lt 1) { throw "مؤسسه‌ای برنگشت" }; $l
}

Write-Host "`n=== 3. Caregiver (پرستار) ===" -ForegroundColor Cyan
$nurseNid = "10" + $rand.ToString() + "0000"
$caregiverId = Step "ثبت پرستار توسط مؤسسه" {
    Invoke-RestMethod "$base/Caregivers/Agency/$agencyId" -Method Post -Headers (Hdr $owner.accessToken) -ContentType "application/json; charset=utf-8" -Body (@{
        phoneNumber="0914000$rand"; fullName="زهرا کریمی"; nationalId=$nurseNid; gender=2; birthYear=1370; level=3
    } | ConvertTo-Json)
}
# ثبت پرستار، حساب او را با نقش Caregiver می‌سازد؛ باید دوباره وارد شود
$nurse = Step "ورود مجدد پرستار پس از ثبت توسط مؤسسه" { Login "0914000$rand" }
Step "نقش پرستار Caregiver شد" { if ($nurse.role -ne "Caregiver") { throw "role=$($nurse.role)" }; $true }
Step "تأیید بدون سوءپیشینه رد می‌شود (قاعده اعتماد)" {
    try { Invoke-RestMethod "$base/Caregivers/$caregiverId/Verify" -Method Put -Headers (Hdr $owner.accessToken); throw "SHOULD_FAIL" }
    catch { if ($_.Exception.Message -eq "SHOULD_FAIL") { throw "بدون سوءپیشینه تأیید شد!" }; $true }
}
Step "ثبت مدارک پرستار" {
    Invoke-RestMethod "$base/Caregivers/$caregiverId/Documents" -Method Put -Headers (Hdr $owner.accessToken) -ContentType "application/json; charset=utf-8" -Body (@{
        hasCriminalRecordCheck=$true; hasCertificate=$true; hasLiabilityInsurance=$true
    } | ConvertTo-Json)
}
Step "تأیید صلاحیت پرستار" { Invoke-RestMethod "$base/Caregivers/$caregiverId/Verify" -Method Put -Headers (Hdr $owner.accessToken) }
Step "افزودن مهارت آلزایمر" {
    Invoke-RestMethod "$base/Caregivers/$caregiverId/Skills" -Method Post -Headers (Hdr $owner.accessToken) -ContentType "application/json; charset=utf-8" -Body (@{skillType=1; yearsOfExperience=5} | ConvertTo-Json)
}
Step "جزئیات پرستار (Dapper read)" {
    $c = Invoke-RestMethod "$base/Caregivers/$caregiverId" -Headers (Hdr $owner.accessToken)
    if ($c.status -ne "Verified") { throw "status=$($c.status)" }; $c
}

Write-Host "`n=== 4. Senior (پرونده مراقبت) ===" -ForegroundColor Cyan
$seniorId = Step "ایجاد پرونده سالمند" {
    Invoke-RestMethod "$base/Seniors" -Method Post -Headers (Hdr $family.accessToken) -ContentType "application/json; charset=utf-8" -Body (@{
        fullName="اکبر رضایی"; nationalId="0012345678"; birthDate="1945-03-21"; gender=1; dependencyLevel=2
        city="تهران"; district="سعادت‌آباد"; address="بلوار فرحزادی پلاک ۱۲"
        latitude=35.7712; longitude=51.3672; timeZoneId="Asia/Tehran"; notes="سابقه سکته خفیف"
    } | ConvertTo-Json)
}
Step "افزودن بیماری" {
    Invoke-RestMethod "$base/Seniors/$seniorId/Conditions" -Method Post -Headers (Hdr $family.accessToken) -ContentType "application/json; charset=utf-8" -Body (@{
        title="دیابت نوع ۲"; description="کنترل‌شده با دارو"; diagnosedAt="2015-06-01"; isChronic=$true
    } | ConvertTo-Json)
}
$medOk = Step "افزودن دارو" {
    Invoke-RestMethod "$base/Seniors/$seniorId/Medications" -Method Post -Headers (Hdr $family.accessToken) -ContentType "application/json; charset=utf-8" -Body (@{
        name="متفورمین"; dosage="۵۰۰ میلی‌گرم"; frequency=2; instruction="بعد از غذا"; startDate="2024-01-01"
    } | ConvertTo-Json)
}
Step "دارو تکراری رد می‌شود" {
    try { Invoke-RestMethod "$base/Seniors/$seniorId/Medications" -Method Post -Headers (Hdr $family.accessToken) -ContentType "application/json; charset=utf-8" -Body (@{
        name="متفورمین"; dosage="۵۰۰"; frequency=2; startDate="2024-01-01" } | ConvertTo-Json); throw "SHOULD_FAIL" }
    catch { if ($_.Exception.Message -eq "SHOULD_FAIL") { throw "داروی تکراری پذیرفته شد" }; $true }
}
Step "افزودن حساسیت" {
    Invoke-RestMethod "$base/Seniors/$seniorId/Allergies" -Method Post -Headers (Hdr $family.accessToken) -ContentType "application/json; charset=utf-8" -Body (@{title="پنی‌سیلین"; reaction="کهیر"} | ConvertTo-Json)
}
Step "افزودن تماس اضطراری" {
    Invoke-RestMethod "$base/Seniors/$seniorId/EmergencyContacts" -Method Post -Headers (Hdr $family.accessToken) -ContentType "application/json; charset=utf-8" -Body (@{
        fullName="دکتر احمدی"; phoneNumber="09121112233"; relation="پزشک معالج"; priority=1
    } | ConvertTo-Json)
}
Step "افزودن فرزند دوم به حلقه خانواده (مقیم خارج)" {
    Invoke-RestMethod "$base/Seniors/$seniorId/Family" -Method Post -Headers (Hdr $family.accessToken) -ContentType "application/json; charset=utf-8" -Body (@{
        phoneNumber="0915000$rand"; relation=1; accessLevel=3; timeZoneId="Europe/Berlin"
    } | ConvertTo-Json)
}
Step "پرونده کامل سالمند" {
    $s = Invoke-RestMethod "$base/Seniors/$seniorId" -Headers (Hdr $family.accessToken)
    if ($s.medications.Count -lt 1) { throw "دارو ثبت نشد" }
    if ($s.familyMembers.Count -ne 2) { throw "اعضای خانواده=$($s.familyMembers.Count)" }
    if ($s.age -lt 70) { throw "سن اشتباه=$($s.age)" }; $s
}
Step "کارت اورژانس" {
    $e = Invoke-RestMethod "$base/Seniors/$seniorId/EmergencyCard" -Headers (Hdr $family.accessToken)
    if ($e.allergies.Count -lt 1) { throw "حساسیت در کارت نیست" }; $e
}
Step "کاربر بیگانه به پرونده دسترسی ندارد" {
    try { Invoke-RestMethod "$base/Seniors/$seniorId" -Headers (Hdr $nurse.accessToken); throw "SHOULD_FAIL" }
    catch { if ($_.Exception.Message -eq "SHOULD_FAIL") { throw "نشت داده سلامت!" }; $true }
}

Write-Host "`n=== 5. Shift (شیفت مراقبت) ===" -ForegroundColor Cyan
$shiftId = Step "ثبت درخواست شیفت" {
    Invoke-RestMethod "$base/Shifts" -Method Post -Headers (Hdr $family.accessToken) -ContentType "application/json; charset=utf-8" -Body (@{
        seniorId=$seniorId; agencyId=$agencyId; serviceType=2; shiftKind=2
        date=$today; startTime="08:00:00"; endTime="20:00:00"; familyNote="لطفاً داروی صبح یادآوری شود"
    } | ConvertTo-Json)
}
Step "قیمت از تعرفه مؤسسه خوانده شد (نه ورودی کاربر)" {
    $s = Invoke-RestMethod "$base/Shifts/$shiftId" -Headers (Hdr $family.accessToken)
    if ($s.price -ne 3500000) { throw "price=$($s.price)" }
    if ($s.status -ne "Requested") { throw "status=$($s.status)" }; $s
}
Step "پرستاران آزاد مؤسسه" {
    $a = Invoke-RestMethod "$base/Caregivers/Available?agencyId=$agencyId&date=$today&startTime=08:00:00&endTime=20:00:00" -Headers (Hdr $owner.accessToken)
    if ($a.Count -lt 1) { throw "پرستار آزادی برنگشت" }; $a
}
Step "تخصیص پرستار به شیفت" {
    Invoke-RestMethod "$base/Shifts/$shiftId/Assign" -Method Put -Headers (Hdr $owner.accessToken) -ContentType "application/json; charset=utf-8" -Body (@{caregiverId=$caregiverId} | ConvertTo-Json)
}
Step "چک‌این خارج از محدوده ژئوفنس رد می‌شود" {
    try { Invoke-RestMethod "$base/Shifts/$shiftId/CheckIn" -Method Put -Headers (Hdr $nurse.accessToken) -ContentType "application/json; charset=utf-8" -Body (@{latitude=35.6892; longitude=51.3890} | ConvertTo-Json); throw "SHOULD_FAIL" }
    catch { if ($_.Exception.Message -eq "SHOULD_FAIL") { throw "چک‌این از راه دور پذیرفته شد!" }; $true }
}
Step "چک‌این در محل منزل سالمند" {
    Invoke-RestMethod "$base/Shifts/$shiftId/CheckIn" -Method Put -Headers (Hdr $nurse.accessToken) -ContentType "application/json; charset=utf-8" -Body (@{latitude=35.7713; longitude=51.3673} | ConvertTo-Json)
}
Step "تکمیل شیفت قبل از گزارش رد می‌شود" {
    try { Invoke-RestMethod "$base/Shifts/$shiftId/Complete" -Method Put -Headers (Hdr $owner.accessToken); throw "SHOULD_FAIL" }
    catch { if ($_.Exception.Message -eq "SHOULD_FAIL") { throw "بدون گزارش تکمیل شد!" }; $true }
}
Step "چک‌اوت پرستار" { Invoke-RestMethod "$base/Shifts/$shiftId/CheckOut" -Method Put -Headers (Hdr $nurse.accessToken) }

Write-Host "`n=== 6. ShiftReport (گزارش شیفت) ===" -ForegroundColor Cyan
$reportId = Step "ثبت گزارش پایان شیفت" {
    Invoke-RestMethod "$base/ShiftReports" -Method Post -Headers (Hdr $nurse.accessToken) -ContentType "application/json; charset=utf-8" -Body (@{
        shiftId=$shiftId; mood=2; appetite=1; sleepQuality=1; mobility=2; painLevel=6
        bloodPressureSystolic=165; bloodPressureDiastolic=95; pulseRate=88; temperature=37.2
        bloodSugar=140; oxygenSaturation=95; notes="امروز اشتها کم بود و دو بار سرگیجه داشت"
    } | ConvertTo-Json)
}
Step "داروهای فعال خودکار به چک‌لیست اضافه شدند" {
    $r = Invoke-RestMethod "$base/ShiftReports/$reportId" -Headers (Hdr $family.accessToken)
    if ($r.medicationIntakes.Count -lt 1) { throw "چک‌لیست دارو خالی است" }; $r
}
Step "علائم حیاتی نامعتبر رد می‌شود (validator)" {
    try { Invoke-RestMethod "$base/ShiftReports" -Method Post -Headers (Hdr $nurse.accessToken) -ContentType "application/json; charset=utf-8" -Body (@{
        shiftId=$shiftId; mood=2; appetite=2; sleepQuality=2; mobility=2; painLevel=99 } | ConvertTo-Json); throw "SHOULD_FAIL" }
    catch { if ($_.Exception.Message -eq "SHOULD_FAIL") { throw "painLevel=99 پذیرفته شد" }; $true }
}
Step "ثبت مصرف دارو" {
    $s = Invoke-RestMethod "$base/Seniors/$seniorId" -Headers (Hdr $family.accessToken)
    $medId = $s.medications[0].id
    Invoke-RestMethod "$base/ShiftReports/$reportId/Medications" -Method Put -Headers (Hdr $nurse.accessToken) -ContentType "application/json; charset=utf-8" -Body (@{
        medicationId=$medId; wasTaken=$true; reason=$null
    } | ConvertTo-Json)
}
Step "ثبت حادثه در شیفت" {
    Invoke-RestMethod "$base/ShiftReports/$reportId/Incident" -Method Put -Headers (Hdr $nurse.accessToken) -ContentType "application/json; charset=utf-8" -Body (@{
        incidentKind=1; description="سقوط خفیف هنگام برخاستن از تخت، بدون آسیب"
    } | ConvertTo-Json)
}
Step "تأیید گزارش توسط خانواده" { Invoke-RestMethod "$base/ShiftReports/$reportId/Approve" -Method Put -Headers (Hdr $family.accessToken) }
Step "تایم‌لاین سلامت" {
    $t = Invoke-RestMethod "$base/ShiftReports/Senior/$seniorId/Timeline?count=14" -Headers (Hdr $family.accessToken)
    if ($t.Count -lt 1) { throw "تایم‌لاین خالی" }; $t
}
$risk = Step "تحلیل ریسک سلامت (خلاصه فارسی)" {
    $r = Invoke-RestMethod "$base/ShiftReports/Senior/$seniorId/HealthRisk" -Headers (Hdr $family.accessToken)
    if (-not $r.summary) { throw "خلاصه خالی" }
    if ($r.riskScore -le 0) { throw "riskScore=$($r.riskScore)" }; $r
}

Write-Host "`n=== 7. Shift Complete + Billing (پرداخت امانی) ===" -ForegroundColor Cyan
Step "تکمیل شیفت" { Invoke-RestMethod "$base/Shifts/$shiftId/Complete" -Method Put -Headers (Hdr $owner.accessToken) }
Step "وضعیت شیفت Completed شد" {
    $s = Invoke-RestMethod "$base/Shifts/$shiftId" -Headers (Hdr $family.accessToken)
    if ($s.status -ne "Completed") { throw "status=$($s.status)" }; $s
}
Step "تقویم شیفت‌های سالمند" {
    $c = Invoke-RestMethod "$base/Shifts/Calendar?seniorId=$seniorId&from=$today&to=$today" -Headers (Hdr $family.accessToken)
    if ($c.Count -lt 1) { throw "تقویم خالی" }; $c
}
Step "لیست صفحه‌بندی‌شده شیفت‌ها" {
    $p = Invoke-RestMethod "$base/Shifts?page=1&pageSize=10&seniorId=$seniorId" -Headers (Hdr $family.accessToken)
    if ($p.totalCount -lt 1) { throw "totalCount=$($p.totalCount)" }; $p
}
Step "داشبورد ریسک پوشش شیفت" {
    Invoke-RestMethod "$base/Shifts/CoverageRisk?withinHours=24" -Headers (Hdr $owner.accessToken)
}
Step "وضعیت مالی مؤسسه" {
    Invoke-RestMethod "$base/Agencies/$agencyId/Balance" -Headers (Hdr $owner.accessToken)
}

Write-Host "`n=== 8. سناریوی عدم حضور و شیفت جایگزین ===" -ForegroundColor Cyan
$shift2 = Step "ثبت شیفت دوم" {
    Invoke-RestMethod "$base/Shifts" -Method Post -Headers (Hdr $family.accessToken) -ContentType "application/json; charset=utf-8" -Body (@{
        seniorId=$seniorId; agencyId=$agencyId; serviceType=2; shiftKind=2
        date=$today; startTime="21:00:00"; endTime="23:00:00"
    } | ConvertTo-Json)
}
Step "تخصیص پرستار به شیفت دوم" {
    Invoke-RestMethod "$base/Shifts/$shift2/Assign" -Method Put -Headers (Hdr $owner.accessToken) -ContentType "application/json; charset=utf-8" -Body (@{caregiverId=$caregiverId} | ConvertTo-Json)
}
Step "ثبت عدم حضور پرستار" { Invoke-RestMethod "$base/Shifts/$shift2/NoShow" -Method Put -Headers (Hdr $owner.accessToken) }
$shift3 = Step "ساخت شیفت جایگزین (تضمین پوشش)" {
    Invoke-RestMethod "$base/Shifts/$shift2/Replace" -Method Post -Headers (Hdr $owner.accessToken) -ContentType "application/json; charset=utf-8" -Body (@{replacementCaregiverId=$caregiverId} | ConvertTo-Json)
}
Step "شیفت جایگزین به شیفت اصلی لینک است و پرستار دارد" {
    $s3 = Invoke-RestMethod "$base/Shifts/$shift3" -Headers (Hdr $family.accessToken)
    if ($s3.replacesShiftId -ne $shift2) { throw "replacesShiftId=$($s3.replacesShiftId)" }
    if ($s3.status -ne "Assigned") { throw "status=$($s3.status)" }
    if ($s3.price -ne 3500000) { throw "مبلغ جایگزین تغییر کرد" }; $s3
}

Write-Host "`n=== 9. تداخل زمانی پرستار ===" -ForegroundColor Cyan
$shift4 = Step "ثبت شیفت هم‌پوشان" {
    Invoke-RestMethod "$base/Shifts" -Method Post -Headers (Hdr $family.accessToken) -ContentType "application/json; charset=utf-8" -Body (@{
        seniorId=$seniorId; agencyId=$agencyId; serviceType=2; shiftKind=2
        date=$today; startTime="22:00:00"; endTime="23:30:00"
    } | ConvertTo-Json)
}
Step "تخصیص پرستار مشغول رد می‌شود" {
    try { Invoke-RestMethod "$base/Shifts/$shift4/Assign" -Method Put -Headers (Hdr $owner.accessToken) -ContentType "application/json; charset=utf-8" -Body (@{caregiverId=$caregiverId} | ConvertTo-Json); throw "SHOULD_FAIL" }
    catch { if ($_.Exception.Message -eq "SHOULD_FAIL") { throw "تخصیص هم‌پوشان پذیرفته شد!" }; $true }
}

Write-Host "`n=== 10. کنترل نقش (Authorization) ===" -ForegroundColor Cyan
function Denied($name, $expected, $block) {
    try { & $block; Write-Host "FAIL  $name -> اجازه داده شد!" -ForegroundColor Red; $script:fail++ }
    catch {
        $code = [int]$_.Exception.Response.StatusCode
        if ($code -eq $expected) { Write-Host "PASS  $name ($code)" -ForegroundColor Green; $script:pass++ }
        else { Write-Host "FAIL  $name -> انتظار $expected، دریافت $code" -ForegroundColor Red; $script:fail++ }
    }
}
Denied "بدون توکن ۴۰۱ می‌گیرد" 401 { Invoke-RestMethod "$base/Seniors/Mine" }
Denied "خانواده نمی‌تواند مؤسسه تأیید کند" 403 { Invoke-RestMethod "$base/Agencies/$agencyId/Approve" -Method Put -Headers (Hdr $family.accessToken) }
Denied "مدیر مؤسسه نمی‌تواند کمیسیون تعیین کند" 403 { Invoke-RestMethod "$base/Agencies/$agencyId/Commission" -Method Put -Headers (Hdr $owner.accessToken) -ContentType "application/json; charset=utf-8" -Body (@{commissionPercent=1} | ConvertTo-Json) }
Denied "پرستار نمی‌تواند پرونده سالمند بسازد" 403 { Invoke-RestMethod "$base/Seniors" -Method Post -Headers (Hdr $nurse.accessToken) -ContentType "application/json; charset=utf-8" -Body (@{fullName="x";birthDate="1950-01-01";gender=1;dependencyLevel=1;city="تهران";address="x"} | ConvertTo-Json) }
Denied "پرستار نمی‌تواند گزارش خودش را تأیید کند" 403 { Invoke-RestMethod "$base/ShiftReports/$reportId/Approve" -Method Put -Headers (Hdr $nurse.accessToken) }
Denied "خانواده نمی‌تواند چک‌این بزند" 403 { Invoke-RestMethod "$base/Shifts/$shiftId/CheckIn" -Method Put -Headers (Hdr $family.accessToken) -ContentType "application/json; charset=utf-8" -Body (@{latitude=35.7712;longitude=51.3672} | ConvertTo-Json) }
Denied "خانواده نمی‌تواند فهرست پرستاران را ببیند" 403 { Invoke-RestMethod "$base/Caregivers?page=1&pageSize=5" -Headers (Hdr $family.accessToken) }
Denied "پرستار نمی‌تواند شیفت تخصیص دهد" 403 { Invoke-RestMethod "$base/Shifts/$shiftId/Assign" -Method Put -Headers (Hdr $nurse.accessToken) -ContentType "application/json; charset=utf-8" -Body (@{caregiverId=$caregiverId} | ConvertTo-Json) }
Denied "خانواده نمی‌تواند وضعیت مالی مؤسسه را ببیند" 403 { Invoke-RestMethod "$base/Agencies/$agencyId/Balance" -Headers (Hdr $family.accessToken) }
Write-Host "`n────────────────────────────────" -ForegroundColor Cyan
Write-Host "PASS: $pass   FAIL: $fail" -ForegroundColor $(if($fail -eq 0){"Green"}else{"Red"})
if ($risk) { Write-Host "`nتحلیل ریسک: [$($risk.riskLevel)/$($risk.riskScore)] $($risk.summary)" -ForegroundColor Yellow }
exit $fail
