using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KdTree;
using KdTree.Math;
using System;

namespace DbscanSharp;

/// <summary>
/// 2‑D DBSCAN clustering with KD‑Tree acceleration.
/// Thread‑safe for concurrent reads after construction.
/// </summary>
public sealed class Dbscan
{
    private readonly IReadOnlyList<Point> _points;
    private readonly float _eps;
    private readonly int _minPts;
    private readonly int[] _labels;
    private readonly KdTree<float, int> _tree;

    /// <param name="points">Collection of (x,y) points.</param>
    /// <param name="eps">Neighborhood radius ε.</param>
    /// <param name="minPts">Minimum points in ε‑neighborhood to be a core point.</param>
    public Dbscan(IReadOnlyList<Point> points, float eps, int minPts)
    {
        if (eps <= 0)         throw new ArgumentOutOfRangeException(nameof(eps));
        if (minPts <= 0)      throw new ArgumentOutOfRangeException(nameof(minPts));
        if (points is null)   throw new ArgumentNullException(nameof(points));

        _points = points;
        _eps    = eps;
        _minPts = minPts;
        _labels = new int[points.Count];
        _tree   = new KdTree<float, int>(2, new FloatMath());

        for (var i = 0; i < points.Count; i++)
        {
            _tree.Add(new[] { points[i].X, points[i].Y }, i);
        }
    }

    /// <summary>
    /// Performs clustering and returns the label array.
    /// 0 = unvisited (only during running), -1 = noise, &gt;0 = cluster id.
    /// </summary>
    public int[] Fit()
    {
        // 1. Parallel pre‑compute neighborhoods (space‑for‑time trade‑off)
        var neighborhoods = new int[_points.Count][];
        Parallel.For(0, _points.Count, i =>
        {
            neighborhoods[i] = Query(_points[i]);
        });

        // 2. Core loop (single‑threaded for simplicity & correctness)
        var clusterId = 0;
        for (var i = 0; i < _points.Count; i++)
        {
            if (_labels[i] != 0) continue;

            var neighbors = neighborhoods[i];
            if (neighbors.Length < _minPts)
            {
                _labels[i] = -1;   // noise
            }
            else
            {
                clusterId++;
                ExpandCluster(neighbors, clusterId, neighborhoods);
            }
        }

        return _labels;
    }

    /* ---------- private helpers ---------- */

    private void ExpandCluster(int[] neighbors, int clusterId, int[][] neighborhoods)
    {
        var visited = new bool[_points.Count];
        var queue   = new Queue<int>(neighbors);

        while (queue.Count > 0)
        {
            var idx = queue.Dequeue();
            if (visited[idx]) continue;
            visited[idx]   = true;
            _labels[idx]   = clusterId;

            var nbrs = neighborhoods[idx];
            if (nbrs.Length >= _minPts)
            {
                foreach (var nIdx in nbrs)
                {
                    if (!visited[nIdx]) queue.Enqueue(nIdx);
                }
            }
        }
    }

    private int[] Query(Point p)
    {
        var nodes = _tree.RadialSearch(new[] { p.X, p.Y }, _eps);
        return nodes.Select(n => n.Value).ToArray();
    }

    /* ---------- public helper struct ---------- */

    public readonly record struct Point(float X, float Y);
}
