using System;

namespace EnhancedPropertyGrid.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class FilePathAttribute : Attribute
    {
        public string Filter { get; }
        public string Title { get; }
        public bool SaveDialog { get; }

        public FilePathAttribute(string filter = "All files|*.*", string title = "Select File", bool saveDialog = false)
        {
            Filter = filter;
            Title = title;
            SaveDialog = saveDialog;
        }
    }
}

