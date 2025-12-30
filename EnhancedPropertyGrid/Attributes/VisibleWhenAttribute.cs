using System;

namespace EnhancedPropertyGrid.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class VisibleWhenAttribute : Attribute
    {
        public string PropertyName { get; }
        public object Value { get; }
        public bool Inverse { get; set; }

        public VisibleWhenAttribute(string propertyName, object value)
        {
            PropertyName = propertyName;
            Value = value;
        }
    }
}
