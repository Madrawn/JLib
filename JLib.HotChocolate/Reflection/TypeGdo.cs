using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JLib.HotChocolate.Reflection;
public class TypeGdo
{
    public string Name => _src.Name;
    public string? Namespace => _src.Namespace;
    private readonly Type _src;

    public TypeGdo(Type src)
    {
        _src = src;
    }
}
