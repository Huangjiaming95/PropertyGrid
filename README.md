# PropertyGrid
自定义属性表



## 使用指南

经过简化后，

EnhancedPropertyGrid 的使用变得更加直接：



```
// 实例化控件

var grid = new EnhancedPropertyGrid.EnhancedPropertyGrid();

grid.Dock = DockStyle.Fill;



// 设置要编辑的对象

grid.SelectedObject = myData;



// (可选) 设置是否按分类显示

grid.Categorized = true;



// (可选) 设置是否显示底部描述栏

grid.ShowDescription = true;
```