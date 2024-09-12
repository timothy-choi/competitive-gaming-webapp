namespace CompetitiveGamingApp.TypeHandlers;

using Dapper;
using System.Data;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;



public class JsonTypeHandler<T> : SqlMapper.TypeHandler<List<T>>
{
    public override void SetValue(IDbDataParameter parameter, List<T> value)
    {
        parameter.Value = JsonConvert.SerializeObject(value);
    }

    public override List<T> Parse(object value)
    {
        return JsonConvert.DeserializeObject<List<T>>(value.ToString());
    }
}