using Dapper;
using System.Data;

namespace StargateAPI.Infrastructure.Data
{
    public class DateOnlyTypeHandler : SqlMapper.TypeHandler<DateOnly>
    {
        public override void SetValue(IDbDataParameter parameter, DateOnly value)
        {
            parameter.Value = value.ToDateTime(TimeOnly.MinValue);
            parameter.DbType = DbType.Date;
        }

        public override DateOnly Parse(object value)
        {
            return value switch
            {
                DateTime dateTime => DateOnly.FromDateTime(dateTime),
                string text => DateOnly.Parse(text),
                _ => throw new DataException($"Cannot convert {value.GetType().Name} to DateOnly.")
            };
        }
    }

    public class NullableDateOnlyTypeHandler : SqlMapper.TypeHandler<DateOnly?>
    {
        public override void SetValue(IDbDataParameter parameter, DateOnly? value)
        {
            parameter.Value = value.HasValue ? value.Value.ToDateTime(TimeOnly.MinValue) : DBNull.Value;
            parameter.DbType = DbType.Date;
        }

        public override DateOnly? Parse(object value)
        {
            if (value is DBNull)
            {
                return null;
            }

            return value switch
            {
                DateTime dateTime => DateOnly.FromDateTime(dateTime),
                string text when string.IsNullOrWhiteSpace(text) => null,
                string text => DateOnly.Parse(text),
                _ => throw new DataException($"Cannot convert {value.GetType().Name} to nullable DateOnly.")
            };
        }
    }
}
