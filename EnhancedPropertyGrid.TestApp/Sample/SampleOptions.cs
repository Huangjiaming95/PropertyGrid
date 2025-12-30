using EnhancedPropertyGrid.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace EnhancedPropertyGrid.TestApp.Sample
{
    public enum Mode
    {
        Fast,
        Normal,
        Slow
    }

    public class Plugin
    {
        [Category("基本")]
        [DisplayName("名称")]
        public string Name { get; set; }

        [Category("基本")]
        [DisplayName("启用")]
        public bool Enabled { get; set; }

        public override string ToString() => $"{Name} ({(Enabled ? "On" : "Off")})";
    }

    public class SampleOptions
    {
        [Category("基本")]
        [DisplayName("名称")]
        [Description("显示名称")]
        public string Name { get; set; }

        [Category("基本")]
        [DisplayName("启用")]
        [Description("是否启用功能")]
        public bool Enabled { get; set; }

        [Category("路径")]
        [DisplayName("配置文件")]
        [FilePath("Config files|*.json;*.yaml;*.yml|All files|*.*", title: "选择配置文件")]
        public string ConfigFile { get; set; }

        [Category("路径")]
        [DisplayName("输出目录")]
        [FolderPath("选择输出目录")]
        public string OutputFolder { get; set; }

        [Category("选择")]
        [DisplayName("模式")]
        public Mode RunMode { get; set; }

        [Category("选择")]
        [DisplayName("星期")]
        public DayOfWeek Day { get; set; }

        [Category("选择")]
        [DisplayName("颜色名")]
        [TypeConverter(typeof(ColorNameConverter))]
        public string ColorName { get; set; }

        [Category("数据")]
        [DisplayName("数量")]
        [Range(1, 100, ErrorMessage = "数量必须在 1 到 100 之间")]
        public int Count { get; set; }

        [Category("数据")]
        [DisplayName("阈值")]
        [VisibleWhen("Enabled", true)]
        public double Threshold { get; set; }

        [Category("集合")]
        [DisplayName("标签")]
        public List<string> Tags { get; set; }

        [Category("集合")]
        [DisplayName("插件")]
        public List<Plugin> Plugins { get; set; }

        public static SampleOptions CreateDemo()
        {
            return new SampleOptions
            {
                Name = "示例任务",
                Enabled = true,
                ConfigFile = string.Empty,
                OutputFolder = string.Empty,
                RunMode = Mode.Normal,
                Day = DayOfWeek.Monday,
                ColorName = "Red",
                Count = 10,
                Threshold = 0.75,
                Tags = new List<string> { "alpha", "beta", "release" },
                Plugins = new List<Plugin>
                {
                    new Plugin { Name = "Auth", Enabled = true },
                    new Plugin { Name = "Cache", Enabled = false }
                }
            };
        }
    }
}

