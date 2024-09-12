namespace CompetitiveGamingApp.TimeSpanHandler;

using Dapper;
using System;
using System.Data;


public class TimeSpanHandler : SqlMapper.TypeHandler<TimeSpan>
{
    public override void SetValue(IDbDataParameter parameter, TimeSpan value)
    {
        // Convert TimeSpan to a format suitable for your database, e.g., a string representation
        parameter.Value = value.ToString("c"); // "c" for constant format (hh:mm:ss)
        parameter.DbType = DbType.String; // Ensure the parameter is treated as a string
    }

    public override TimeSpan Parse(object value)
    {
        if (value is TimeSpan timeSpan)
        {
            return timeSpan;
        }
        else if (value is string stringValue)
        {
            // Parse the string value to TimeSpan
            return TimeSpan.Parse(stringValue);
        }
        else
        {
            throw new InvalidCastException("Cannot convert value to TimeSpan.");
        }
    }
}