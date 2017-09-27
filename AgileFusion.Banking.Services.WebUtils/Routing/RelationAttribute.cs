using System;

namespace AgileFusion.Banking.Services.WebUtils.Routing
{
    [AttributeUsage(AttributeTargets.Method)]
    public class RelationAttribute : Attribute
    {
        public readonly string Name;
        public RelationAttribute(string name)
        {
            Name = name;
        }
    }
}
