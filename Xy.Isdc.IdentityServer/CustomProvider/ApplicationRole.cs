using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Xy.Isdc.IdentityServer.CustomProvider
{
    public class ApplicationRole
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; }
    }
}
