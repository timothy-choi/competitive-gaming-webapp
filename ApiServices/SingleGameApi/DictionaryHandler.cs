using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using Newtonsoft.Json;

public class DictionaryHandler : SqlMapper.TypeHandler<Dictionary<string, string>>
{
    public override void SetValue(IDbDataParameter parameter, Dictionary<string, string> value)
    {
        // Convert the dictionary to a JSON string
        parameter.Value = JsonConvert.SerializeObject(value);
        parameter.DbType = DbType.String;
    }

    public override Dictionary<string, string> Parse(object value)
    {
        if (value is string jsonString)
        {
            // Deserialize the JSON string to a dictionary
            return JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonString);
        }
        throw new InvalidCastException("Cannot convert value to Dictionary<string, string>.");
    }
}
