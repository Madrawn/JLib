using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using JLib.Helper;
using Microsoft.Extensions.DependencyInjection;

namespace JLib.Data.Authorization;

public interface IAuthorizationInfo
{
    DataObjectType Target { get; }
}
