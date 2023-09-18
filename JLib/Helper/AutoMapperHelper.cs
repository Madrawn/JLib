using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using JLib.Attributes;

namespace JLib.Helper;
public static class AutoMapperHelper
{
    public static void AddProfiles(this IMapperConfigurationExpression builder, ITypeCache typeCache){
        builder.AddProfiles(typeCache.All<Types.AutoMapperProfileType>().Select(p => p.Create(typeCache)));
    }
}
