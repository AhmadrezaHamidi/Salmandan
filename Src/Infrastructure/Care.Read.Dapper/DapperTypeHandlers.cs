namespace Care.Read.Dapper;

/// <summary>
/// Dapper نوع‌های DateOnly و TimeOnly را به‌صورت پیش‌فرض نگاشت نمی‌کند؛
/// SqlClient ستون date را DateTime و ستون time را TimeSpan برمی‌گرداند.
/// این handler ها آن شکاف را پر می‌کنند.
/// </summary>
public static class DapperTypeHandlers
{
    private static bool _registered;

    public static void Register()
    {
        if (_registered) return;

        SqlMapper.AddTypeHandler(new DateOnlyTypeHandler());
        SqlMapper.AddTypeHandler(new TimeOnlyTypeHandler());
        _registered = true;
    }

    private sealed class DateOnlyTypeHandler : SqlMapper.TypeHandler<DateOnly>
    {
        public override DateOnly Parse(object value) => value switch
        {
            DateTime dt  => DateOnly.FromDateTime(dt),
            DateOnly d   => d,
            string s     => DateOnly.Parse(s),
            _            => throw new InvalidCastException($"نگاشت {value.GetType()} به DateOnly ممکن نیست")
        };

        public override void SetValue(IDbDataParameter parameter, DateOnly value)
        {
            parameter.DbType = DbType.Date;
            parameter.Value  = value.ToDateTime(TimeOnly.MinValue);
        }
    }

    private sealed class TimeOnlyTypeHandler : SqlMapper.TypeHandler<TimeOnly>
    {
        public override TimeOnly Parse(object value) => value switch
        {
            TimeSpan ts  => TimeOnly.FromTimeSpan(ts),
            TimeOnly t   => t,
            DateTime dt  => TimeOnly.FromDateTime(dt),
            string s     => TimeOnly.Parse(s),
            _            => throw new InvalidCastException($"نگاشت {value.GetType()} به TimeOnly ممکن نیست")
        };

        public override void SetValue(IDbDataParameter parameter, TimeOnly value)
        {
            parameter.DbType = DbType.Time;
            parameter.Value  = value.ToTimeSpan();
        }
    }
}
