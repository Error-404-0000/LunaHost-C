using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Attributes
{
    /// <summary>
    /// Indicates that a property should be automatically injected with a dependency by a dependency injection
    /// </summary>
    /// <remarks>This attribute is typically applied to properties in classes that are managed by a dependency
    /// injection container. The container uses this attribute to identify properties that require dependency
    /// injection.</remarks>
    [AttributeUsage(AttributeTargets.Property|AttributeTargets.Parameter)]
    public class DInjectAttribute:Attribute;
}
