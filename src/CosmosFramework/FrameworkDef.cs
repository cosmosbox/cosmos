using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cosmos.Framework
{
    [AttributeUsage(AttributeTargets.Class)]
    class DependencyAttribute : Attribute
    {
        public Type DepType { get; set; }
    }

    class FrameworkDef
    {

    }
}
