using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JLib.Data;

public interface IPersistenceAccessor
{
    public void SaveChanges();
}