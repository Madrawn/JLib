using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JLib.Reflection;

namespace JLib;
public static class JLibTypePackage
{
    public static ITypePackage Instance { get; } = TypePackage.Get(typeof(JLibTypePackage).Assembly);
}
