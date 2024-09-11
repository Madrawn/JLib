using System.Linq.Expressions;
using System.Reflection;
using AutoMapper;

namespace JLib.AutoMapper;

/// <summary>
/// this automapper profile creates a map from <typeparamref name="T"/> to Dictionary&lt;string,string?> while adding all public properties of <typeparamref name="T"/> to the dictionary, even if they are null.<br/>
/// note: only public Properties are added, not Fields or any other members
/// </summary>
public class TypeToDictionaryProfile<T> : Profile
{
    /// <summary></summary>
    public TypeToDictionaryProfile()
    {
        var map = CreateMap<T, Dictionary<string, string?>>();


        var tDict = typeof(Dictionary<string, string?>);
        var tString = typeof(string);
        var t = typeof(T);

        var mAdd = tDict.GetMethod(nameof(Dictionary<string, string?>.Add), new[] { tString, tString })
                   ?? throw new("dictionary.add not found");

        var ctor = tDict.GetConstructor(Array.Empty<Type>())
                   ?? throw new("ctor not found");


        var parExSource= Expression.Parameter(typeof(T), "source");
        
        var valueAssign = Expression.ListInit(Expression.New(ctor),
            t.GetProperties().Cast<MemberInfo>().Concat(t.GetFields()).Select(memberInfo =>
            {
                var tMember = memberInfo switch
                {
                    PropertyInfo propInfo => propInfo.PropertyType,
                    FieldInfo fieldInfo => fieldInfo.FieldType,
                    _ => throw new("invalid member type")
                };

                Expression value = Expression.MakeMemberAccess(parExSource, memberInfo);
                if (tMember != typeof(string))
                {
                    var mToString = tMember.GetMethod(nameof(ToString), Array.Empty<Type>())
                        ?? throw new("toString not found");
                    value = Expression.Call(value, mToString);
                }

                return Expression.ElementInit(mAdd, Expression.Constant(memberInfo.Name), value);
            }));


        var res = Expression.Lambda<Func<T, Dictionary<string, string?>>>(valueAssign, parExSource);
        map.ConvertUsing(res);
    }
}
