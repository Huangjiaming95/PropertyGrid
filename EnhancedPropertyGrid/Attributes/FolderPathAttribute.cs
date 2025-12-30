using System;

namespace EnhancedPropertyGrid.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class FolderPathAttribute : Attribute
    {
        public string Description { get; }

        public FolderPathAttribute(string description = "Select folder")
        {
            Description = description;
        }
    }
}

