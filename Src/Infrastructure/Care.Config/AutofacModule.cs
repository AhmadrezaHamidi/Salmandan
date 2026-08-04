using System.Data;
using AhmadBase.Config.Containers;
using AhmadBase.External.Service;
using AhmadBase.External.Service.Payment;
using AhmadBase.External.Service.Sms;
using AhmadBase.IOC;
using Autofac;
using Care.Application.Contract.Services;
using Care.Application.Validators;
using Care.Config.Extensions.Swagger;
using Care.Config.Middlewares;
using Care.Domain.Agency.Repositories;
using Care.Domain.Billing.Repositories;
using Care.Domain.Caregiver.Repositories;
using Care.Domain.Identity.Repositories;
using Care.Domain.Payroll.Repositories;
using Care.Domain.Report.Repositories;
using Care.Domain.Senior.Repositories;
using Care.Domain.Shift.Repositories;
using Care.Persistence.EF.Repositories;
using Care.Persistence.EF.Services;
using Care.Read.Dapper;
using Care.Read.Dapper.Agency;
using Care.Read.Dapper.Billing;
using Care.Read.Dapper.Caregiver;
using Care.Read.Dapper.Payroll;
using Care.Read.Dapper.Report;
using Care.Read.Dapper.Senior;
using Care.Read.Dapper.Shift;
using FluentValidation;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;
using AhmadSmsOptions = AhmadBase.External.Service.SmsOptions;
using PaymentOptions  = AhmadBase.External.Service.Payment.PaymentGatewayOptions;
using SmsContract     = AhmadBase.External.Service.ISmsService;

namespace Care.Config;

public class AutofacModule : Autofac.Module
{
    private readonly IConfiguration _configuration;
    private readonly bool           _isDevelopment;

    public AutofacModule(IConfiguration configuration)
    {
        _configuration = configuration;
        _isDevelopment = bool.TryParse(configuration["UseFakeServices"], out var b) && b;
    }

    protected override void Load(ContainerBuilder builder)
    {
        // ── ۱. ثبت خودکار فریم‌ورک ──────────────────────────────────────────
        Container.Setup(builder, _configuration)
            .RegisterDatabaseModels()
            .RegisterMainServices()
            .RegisterHandlers()
            .RegisterAuxiliaryServices();

        // ── ۲. Repository های سمت نوشتن (EF) ────────────────────────────────
        builder.RegisterType<UserRepository>()         .As<IUserRepository>()         .InstancePerLifetimeScope();
        builder.RegisterType<OtpRepository>()          .As<IOtpRepository>()          .InstancePerLifetimeScope();
        builder.RegisterType<RefreshTokenRepository>() .As<IRefreshTokenRepository>() .InstancePerLifetimeScope();
        builder.RegisterType<AgencyRepository>()       .As<IAgencyRepository>()       .InstancePerLifetimeScope();
        builder.RegisterType<CaregiverRepository>()    .As<ICaregiverRepository>()    .InstancePerLifetimeScope();
        builder.RegisterType<SeniorRepository>()       .As<ISeniorRepository>()       .InstancePerLifetimeScope();
        builder.RegisterType<ShiftRepository>()        .As<IShiftRepository>()        .InstancePerLifetimeScope();
        builder.RegisterType<ShiftReportRepository>()  .As<IShiftReportRepository>()  .InstancePerLifetimeScope();
        builder.RegisterType<CareOrderRepository>()    .As<ICareOrderRepository>()    .InstancePerLifetimeScope();
        builder.RegisterType<PayoutRepository>()       .As<IPayoutRepository>()       .InstancePerLifetimeScope();

        // ── ۳. اتصال Dapper برای سمت خواندن ─────────────────────────────────
        DapperTypeHandlers.Register();

        var connStr = _configuration.GetConnectionString("QueryConnection") ?? string.Empty;
        builder.Register<IDbConnection>(_ => new SqlConnection(connStr))
               .As<IDbConnection>()
               .InstancePerLifetimeScope();

        builder.RegisterType<AgencyReadRepository>()    .InstancePerLifetimeScope();
        builder.RegisterType<CaregiverReadRepository>() .InstancePerLifetimeScope();
        builder.RegisterType<SeniorReadRepository>()    .InstancePerLifetimeScope();
        builder.RegisterType<ShiftReadRepository>()     .InstancePerLifetimeScope();
        builder.RegisterType<ReportReadRepository>()    .InstancePerLifetimeScope();
        builder.RegisterType<BillingReadRepository>()   .InstancePerLifetimeScope();
        builder.RegisterType<PayrollReadRepository>()   .InstancePerLifetimeScope();

        // ── ۴. پیامک (با Circuit Breaker) ───────────────────────────────────
        builder.Register(c =>
        {
            var opts = c.Resolve<IConfiguration>().GetSection(AhmadSmsOptions.Section).Get<AhmadSmsOptions>() ?? new AhmadSmsOptions();
            return Microsoft.Extensions.Options.Options.Create(opts);
        }).As<IOptions<AhmadSmsOptions>>().SingleInstance();

        if (_isDevelopment)
            builder.RegisterType<FakeSmsSender>().As<SmsContract>().InstancePerLifetimeScope();
        else
            builder.RegisterType<SmsSender>().As<SmsContract>().InstancePerLifetimeScope();

        // ── ۵. درگاه پرداخت ─────────────────────────────────────────────────
        builder.Register(c =>
        {
            var opts = c.Resolve<IConfiguration>().GetSection(PaymentOptions.Section).Get<PaymentOptions>() ?? new PaymentOptions();
            return Microsoft.Extensions.Options.Options.Create(opts);
        }).As<IOptions<PaymentOptions>>().SingleInstance();

        builder.RegisterType<ZarinPalService>()       .As<IPaymentService>() .InstancePerLifetimeScope();
        builder.RegisterType<ZarinPalPaymentGateway>().As<IPaymentGateway>() .InstancePerLifetimeScope();

        // ── ۶. سرویس‌های پلتفرم ─────────────────────────────────────────────
        builder.RegisterType<JwtService>()         .As<IJwtService>()         .InstancePerLifetimeScope();
        builder.RegisterType<HealthRiskAnalyzer>() .As<IHealthRiskAnalyzer>() .InstancePerLifetimeScope();
        builder.RegisterType<FamilyNotifier>()     .As<IFamilyNotifier>()     .InstancePerLifetimeScope();
        builder.RegisterType<CareAccessGuard>()    .As<ICareAccessGuard>()    .InstancePerLifetimeScope();
        builder.RegisterType<FamilyNotifier>()     .As<IFamilyNotifier>()     .InstancePerLifetimeScope();

        // ── ۷. Middleware ها ────────────────────────────────────────────────
        builder.RegisterType<CorrelationIdStartupFilter>()       .As<IStartupFilter>().SingleInstance();
        builder.RegisterType<ValidationExceptionStartupFilter>() .As<IStartupFilter>().SingleInstance();

        // ── ۸. FluentValidation ─────────────────────────────────────────────
        builder.RegisterAssemblyTypes(typeof(ValidationBehavior<,>).Assembly)
               .AsClosedTypesOf(typeof(IValidator<>))
               .InstancePerLifetimeScope();

        // ترتیب ثبت = ترتیب اجرا؛ اعتبارسنجی بیرونی‌ترین لایه است تا زودتر fail کند
        builder.RegisterGeneric(typeof(ValidationBehavior<,>))
               .As(typeof(IPipelineBehavior<,>))
               .InstancePerLifetimeScope();

        // ── ۸b. انتشار رویدادهای دامین پس از هر Command ─────────────────────
        builder.RegisterGeneric(typeof(DomainEventDispatchBehavior<,>))
               .As(typeof(IPipelineBehavior<,>))
               .InstancePerLifetimeScope();

        // ── ۹. Swagger ──────────────────────────────────────────────────────
        builder.Register<IConfigureOptions<SwaggerGenOptions>>(_ =>
            new ConfigureOptions<SwaggerGenOptions>(opts =>
            {
                opts.SchemaGeneratorOptions.SchemaIdSelector = t => t.FullName ?? t.Name;
                opts.OperationFilterDescriptors.Add(new FilterDescriptor
                {
                    Type      = typeof(ErrorResponseOperationFilter),
                    Arguments = Array.Empty<object>()
                });
            }))
            .As<IConfigureOptions<SwaggerGenOptions>>()
            .SingleInstance();
    }
}
