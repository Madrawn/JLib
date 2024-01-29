using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JLib.DataGeneration.Examples.Setup.Models;

namespace JLib.DataGeneration.Examples.SnapshotInfo;
public class CustomerSnapshotInfo
{
    public CustomerSnapshotInfo(CustomerEntity customer, IIdRegistry idRegistry)
    {
        Id = customer.Id.IdSnapshot(idRegistry);
        UserName = customer.UserName;
    }

    public string Type => "Customer";
    public IdSnapshotInformation Id { get; init; }
    public string UserName { get; set; }
}