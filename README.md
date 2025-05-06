# DbscanSharp

🌐 Fast and lightweight **2D DBSCAN clustering** library based on **KD‑Tree** in pure C#.

🌏 基于 KD‑Tree 实现的高效轻量级二维 DBSCAN 聚类算法库，纯 C# 编写，可快速集成到各类项目中。

---

## ✨ Features / 功能特点

- ✅ 高性能 KD‑Tree 加速邻域查询
- ✅ 支持任意二维点集合
- ✅ 并行预处理（利用多核）
- ✅ 兼容 .NET Standard 2.0 和 .NET 8+
- ✅ 易于集成、可打包为 NuGet 包

---

## 📦 Installation / 安装方式

> 未来发布到 NuGet 后，可直接引用包（如下）。当前可通过克隆源码手动引用项目。

```bash
# NuGet 方式（待发布）
dotnet add package DbscanSharp

# 手动引用源码（推荐结构）
src/DbscanSharp/
```

## 🚀 Usage / 使用示例
```bash
using DbscanSharp;

var points = new List<Dbscan.Point>
{
    new(0.0f, 0.0f),
    new(0.05f, 0.03f),
    new(1.0f, 1.0f),
    new(5.0f, 5.0f),
};

var dbscan = new Dbscan(points, eps: 0.2f, minPts: 3);
int[] labels = dbscan.Fit();

for (int i = 0; i < points.Count; i++)
{
    Console.WriteLine($"Point {points[i]} → Cluster #{labels[i]}");
}
