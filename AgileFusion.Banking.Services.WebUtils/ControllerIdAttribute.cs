using System;

namespace AgileFusion.Banking.Services.WebUtils
{
    /// <summary>
    /// Attribute for specifying id of controller
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class ControllerIdAttribute : Attribute
    {
        /// <summary>
        /// Gets string value of controller id
        /// </summary>
        public readonly string Value;
        /// <summary>
        /// Gets GUID value of controller id
        /// </summary>
        public readonly Guid Guid;
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="guid">Controller id</param>
        public ControllerIdAttribute(string guid)
        {
            if(!Guid.TryParse(guid, out Guid)) { throw new ArgumentException("Invalid guid", "guid"); }
            Value = Guid.ToString();
        }        
    }
}
