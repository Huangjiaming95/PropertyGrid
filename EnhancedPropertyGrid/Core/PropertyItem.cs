using System;
using System.ComponentModel;

namespace EnhancedPropertyGrid.Core
{
    internal class PropertyItem
    {
        public object Instance { get; }
        public PropertyDescriptor Descriptor { get; }

        public string Name => Descriptor.Name;
        public string DisplayName => Descriptor.DisplayName;
        public string Category => Descriptor.Category ?? string.Empty;
        public string Description => Descriptor.Description ?? string.Empty;
        public bool IsReadOnly => Descriptor.IsReadOnly;
        public Type PropertyType => Descriptor.PropertyType;
        public AttributeCollection Attributes => Descriptor.Attributes;
        public TypeConverter Converter => Descriptor.Converter;

        public PropertyItem(object instance, PropertyDescriptor descriptor)
        {
            Instance = instance ?? throw new ArgumentNullException(nameof(instance));
            Descriptor = descriptor ?? throw new ArgumentNullException(nameof(descriptor));
        }

        public object GetValue()
        {
            return Descriptor.GetValue(Instance);
        }

        public void SetValue(object value)
        {
            Descriptor.SetValue(Instance, value);
        }
        public string GetValidationErrors()
        {
            // 1. Data Annotations
            var val = GetValue();
            var results = new System.Collections.Generic.List<System.ComponentModel.DataAnnotations.ValidationResult>();
            var context = new System.ComponentModel.DataAnnotations.ValidationContext(Instance) { MemberName = Name };

            // Validate property attributes
            System.ComponentModel.DataAnnotations.Validator.TryValidateProperty(val, context, results);

            if (results.Count > 0)
            {
                return results[0].ErrorMessage;
            }

            // 2. IDataErrorInfo
            if (Instance is IDataErrorInfo info)
            {
                var error = info[Name];
                if (!string.IsNullOrEmpty(error)) return error;
            }

            return null;
        }
    }
}

