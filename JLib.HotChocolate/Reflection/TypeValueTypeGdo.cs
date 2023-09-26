using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace JLib.HotChocolate.Reflection;
public class TypeValueTypeGdo
{
    private readonly TypeValueType _src;
    public TypeGdo Self => new(_src.GetType());
    public TypeGdo Value => new(_src.Value);
    public TypeValueTypeGdo(TypeValueType src)
    {
        _src = src;
    }
}