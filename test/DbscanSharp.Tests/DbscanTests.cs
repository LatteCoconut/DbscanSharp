using FluentAssertions;
namespace DbscanSharp.Tests;

public class DbscanTests
{
    // ---------- 帮助函数 ----------
    private static List<Dbscan.Point> MakeBlob(float cx, float cy, int n, float step)
    {
        var pts = new List<Dbscan.Point>(n);
        for (int i = 0; i < n; i++)
            pts.Add(new Dbscan.Point(cx + i * step, cy + i * step));
        return pts;
    }

    // ---------- 测试 1：两个密集簇 ----------
    [Fact]
    public void TwoDenseBlobs_ShouldReturnTwoClusters()
    {
        var points = new List<Dbscan.Point>();
        points.AddRange(MakeBlob(0f , 0f , 50, 0.01f));  // 簇 A
        points.AddRange(MakeBlob(5f , 5f , 50, 0.01f));  // 簇 B

        var algo   = new Dbscan(points, eps: 0.2f, minPts: 4);
        var labels = algo.Fit();

        // 断言：簇号 1 和 2 都出现
        labels.Should().Contain(1)
            .And.Contain(2);

        // 每个簇恰好 50 个点
        labels.Count(l => l == 1).Should().Be(50);
        labels.Count(l => l == 2).Should().Be(50);

        // 没有噪声
        labels.Should().NotContain(-1);
    }

    // ---------- 测试 2：全部噪声 ----------
    [Fact]
    public void SparsePoints_AllNoise()
    {
        var points = new List<Dbscan.Point>
        {
            new(0,0), new(10,10), new(20,20), new(30,30)
        };

        var algo   = new Dbscan(points, eps: 0.5f, minPts: 2);
        var labels = algo.Fit();

        labels.Should().AllBeEquivalentTo(-1);  // 全部是噪声
    }

    // ---------- 测试 3：单个簇 ----------
    [Fact]
    public void OneDenseBlob_SingleCluster()
    {
        var points = MakeBlob(1f, 1f, 100, 0.02f);

        var algo   = new Dbscan(points, eps: 0.3f, minPts: 3);
        var labels = algo.Fit();

        // 只存在簇号 1，且没有噪声
        labels.Distinct().Should().BeEquivalentTo(new[] { 1 });
        labels.Length.Should().Be(100);
    }
}