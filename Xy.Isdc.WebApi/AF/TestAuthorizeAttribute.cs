using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace Xy.Isdc.WebApi.AF
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class TestAuthorizeAttribute : AuthorizeAttribute
    {

        public string Permission { get; set; }

        public TestAuthorizeAttribute(string permission)
        {
            Permission = permission;
        }

    }
}
