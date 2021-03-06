﻿using BruTile;
using SkiaSharp;
using System;
using System.Collections.Generic;
using VTMap.Common.Extensions;
using VTMap.Core;
using VTMap.Core.Enums;
using VTMap.Core.Interfaces;
using VTMap.Core.Primitives;
using VTMap.Core.Utilities;

namespace VTMap.Common.Primitives
{
    /// <summary>
    /// A VectorElement holds informations such as points and tags
    /// </summary>
    public class VectorElement : IVectorElement
    {
        List<Point> points = new List<Point>(512);
        List<int> index = new List<int>(64);
        TileClipper tileClipper;

        public VectorElement(TileClipper clipper, TileIndex tileIndex)
        {
            tileClipper = clipper;
            index.Add(0);

            TileIndex = tileIndex;
        }

        public VectorElement(TileClipper clipper, TileIndex index, string layer, string id) : this(clipper, index)
        {
            Layer = layer;
            Id = id;
        }

        public string Layer { get; set; }

        public string Id { get; set; }

        public TileIndex TileIndex { get; }

        public GeometryType Type { get; private set; }

        public TagsCollection Tags { get; } = new TagsCollection();

        public List<Point> Points { get => IsPoint ? new List<Point>(points) : new List<Point>(); }

        public bool IsPoint { get => Type == GeometryType.Point; }

        public bool IsLine { get => Type == GeometryType.LineString; }

        public bool IsPolygon { get => Type == GeometryType.Polygon; }

        public int Count { get => IsPoint ? points.Count : index.Count; }

        public void Add(Point point)
        {
            points.Add(point);
            index[index.Count - 1]++;
        }

        public void Add(float x, float y)
        {
            Add(new Point(x, y));
        }

        public void Add(IEnumerable<Point> points)
        {
            foreach (var point in points)
                Add(point);
        }

        public Point Get(int index)
        {
            if (index >= 0 && index < Count)
                return points[index];

            return Point.Empty;
        }

        public void Clear()
        {
            index.Clear();
            index.Add(0);
            points.Clear();
            Tags.Clear();
            Type = GeometryType.Unknown;
        }

        public void AddToPath(SKPath path)
        {
            int start = 0;

            for (int i = 0; i < index.Count; i++)
            {
                if (index[i] > 0)
                {
                    if (IsPolygon)
                        path.AddPoly(tileClipper.ReducePolygonPointsToClipRect(points.GetRange(start, index[i])).ToArray().ToSKPoints(), true);
                    else if (IsLine)
                    {
                        var lines = tileClipper.ReduceLinePointsToClipRect(points.GetRange(start, index[i]));
                        foreach (var line in lines)
                            path.AddPoly(line.ToArray().ToSKPoints(), false);
                    }
                }

                start += index[i];
            }
        }

        public SKPath CreatePath()
        {
            SKPath path = new SKPath();

            AddToPath(path);

            return path;
        }

        public void Scale(float factor)
        {
            if (factor == 1)
                return;

            for (int i = 0; i < points.Count; i++)
            {
                points[i] = new Point(points[i].X * factor, points[i].Y * factor);
            }
        }

        public void StartPoint()
        {
            SetOrCheckMode(GeometryType.Point);
        }

        public void StartLine()
        {
            SetOrCheckMode(GeometryType.LineString);

            if (index[index.Count - 1] > 0)
                index.Add(0);
        }

        public void StartPolygon()
        {
            SetOrCheckMode(GeometryType.Polygon);

            if (index[index.Count - 1] > 0)
                index.Add(0);
        }

        public void StartHole()
        {
            if (Type != GeometryType.Polygon)
                throw new ArgumentException($"Wrong type mode. Expected {GeometryType.Polygon}, but get {Type}.");

            if (index[index.Count - 1] > 0)
                index.Add(0);
        }

        void SetOrCheckMode(GeometryType type)
        {
            if (Type == type)
                return;

            if (Type != GeometryType.Unknown)
                throw new ArgumentException($"Wrong type mode. Expected {Type}, but get {type}.");

            Type = type;
        }
    }
}
