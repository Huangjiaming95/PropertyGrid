using System;
using System.ComponentModel;
using System.Globalization;

namespace EnhancedPropertyGrid.TestApp.Sample
{
    public class ColorNameConverter : StringConverter
    {
        private static readonly string[] Colors = new[] { "Red", "Green", "Blue", "Orange", "Purple" };

        public override bool GetStandardValuesSupported(ITypeDescriptorContext context) => true;

        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context) => true;

        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
            => new StandardValuesCollection(Colors);
    }
}

