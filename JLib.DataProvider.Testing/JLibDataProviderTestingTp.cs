using JLib.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JLib.DataProvider.Testing;
public static class JLibDataProviderTestingTp
{
    public static ITypePackage Instance { get; } = TypePackage.Get(typeof(JLibDataProviderTestingTp).Assembly);
}
