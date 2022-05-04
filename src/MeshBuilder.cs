// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MeshBuilder.cs" company="Helix Toolkit">
//   Copyright (c) 2014 Helix Toolkit contributors
// </copyright>
// <summary>
//   Builds MeshGeometry3D objects.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Primitives
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading;
    using System.Linq;
    using System.Windows;
    using System.Windows.Media;
    using System.Windows.Media.Media3D;
    using DoubleOrSingle = System.Double;

#pragma warning disable 0436
   public class MeshBuilder
    {

        public MeshBuilder(bool generateNormals = true, bool generateTexCoords = true, bool tangentSpace = false)
        {
            this.positions = new Point3DCollection();
            this.triangleIndices = new Int32Collection();
            if (generateNormals)
            {
                this.normals = new Vector3DCollection();
            }
            if (generateTexCoords)
            {
                this.textureCoordinates = new PointCollection();
            }
            if (tangentSpace)
            {
                this.tangents = new Vector3DCollection();
                this.bitangents = new Vector3DCollection();
            }
        }

        private Point3DCollection positions;
        
        public Point3DCollection Positions
        {
            get
            {
                return this.positions;
            }
        }
        
        private Int32Collection triangleIndices;
        
        public Int32Collection TriangleIndices
        {
            get
            {
                return this.triangleIndices;
            }
        }
        
        private Vector3DCollection normals;
        
        public Vector3DCollection Normals
        {
            get
            {
                return this.normals;
            }
            set
            {
                this.normals = value;
            }
        }
        
        private PointCollection textureCoordinates;
        
        public PointCollection TextureCoordinates
        {
            get
            {
                return this.textureCoordinates;
            }
            set
            {
                this.textureCoordinates = value;
            }
        }
        
        private Vector3DCollection tangents;
        
        public Vector3DCollection Tangents
        {
            get
            {
                return this.tangents;
            }
            set
            {
                this.tangents = value;
            }
        }
        
        private Vector3DCollection bitangents;
        
        public Vector3DCollection BiTangents
        {
            get
            {
                return this.bitangents;
            }
            set
            {
                this.bitangents = value;
            }
        }
        
        public bool HasNormals
        {
            get
            {
                return this.normals != null;
            }
        }
        
        public bool HasTexCoords
        {
            get
            {
                return this.textureCoordinates != null;
            }
        }
        
        public bool HasTangents
        {
            get
            {
                return this.tangents != null;
            }
        }
        
        public bool CreateNormals
        {
            get
            {
                return this.normals != null;
            }
            set
            {
                if (value && this.normals == null)
                {
                    this.normals = new Vector3DCollection();
                }
                if (!value)
                {
                    this.normals = null;
                }
            }
        }
        
        public bool CreateTextureCoordinates
        {
            get
            {
                return this.textureCoordinates != null;
            }
            set
            {
                if (value && this.textureCoordinates == null)
                {
                    this.textureCoordinates = new PointCollection();
                }
                if (!value)
                {
                    this.textureCoordinates = null;
                }
            }
        }
        
        public static IList<Point> GetCircle(int thetaDiv, bool closed = false)
        {
            IList<Point> circle = new PointCollection();
            var num = closed ? thetaDiv : thetaDiv - 1;
            for (var i = 0; i < thetaDiv; i++)
            {
                var theta = (DoubleOrSingle)Math.PI * 2 * ((DoubleOrSingle)i / num);
                circle.Add(new Point((DoubleOrSingle)Math.Cos(theta), -(DoubleOrSingle)Math.Sin(theta)));
            }
            IList<Point> result = new List<Point>();
            foreach (var point in circle)
            {
                result.Add(new Point(point.X, point.Y));
            }
            return result;
        }
        public void AddCone(Point3D origin, Vector3D direction,
            double baseRadius, double topRadius, double height,
            bool baseCap, bool topCap, int thetaDiv)
        {
            var pc = new PointCollection();
            var tc = new List<double>();
            if (baseCap)
            {
                pc.Add(new Point(0, 0));
                tc.Add(0);
            }

            pc.Add(new Point(0, (DoubleOrSingle)baseRadius));
            tc.Add(1);
            pc.Add(new Point((DoubleOrSingle)height, (DoubleOrSingle)topRadius));
            tc.Add(0);
            if (topCap)
            {
                pc.Add(new Point((DoubleOrSingle)height, 0));
                tc.Add(1);
            }

            this.AddRevolvedGeometry(pc, tc, origin, direction, thetaDiv);
        }
        public void AddRevolvedGeometry(IList<Point> points, IList<double> textureValues, Point3D origin, Vector3D direction, int thetaDiv)
        {
            direction.Normalize();

            var u = direction.FindAnyPerpendicular();
            var v = Vector3D.CrossProduct(direction, u);
            u.Normalize();
            v.Normalize();

            var circle = GetCircle(thetaDiv);

            var index0 = this.positions.Count;
            var n = points.Count;

            var totalNodes = (points.Count - 1) * 2 * thetaDiv;
            var rowNodes = (points.Count - 1) * 2;

            for (var i = 0; i < thetaDiv; i++)
            {
                var w = (v * circle[i].X) + (u * circle[i].Y);

                for (var j = 0; j + 1 < n; j++)
                {
                    var q1 = origin + (direction * points[j].X) + (w * points[j].Y);
                    var q2 = origin + (direction * points[j + 1].X) + (w * points[j + 1].Y);

                    this.positions.Add(q1);
                    this.positions.Add(q2);

                    if (this.normals != null)
                    {
                        var tx = points[j + 1].X - points[j].X;
                        var ty = points[j + 1].Y - points[j].Y;
                        var normal = (-direction * ty) + (w * tx);
                        normal.Normalize();
                        this.normals.Add(normal);
                        this.normals.Add(normal);
                    }

                    if (this.textureCoordinates != null)
                    {
                        this.textureCoordinates.Add(new Point((DoubleOrSingle)i / (thetaDiv - 1), textureValues == null ? (DoubleOrSingle)j / (n - 1) : (DoubleOrSingle)textureValues[j]));
                        this.textureCoordinates.Add(new Point((DoubleOrSingle)i / (thetaDiv - 1), textureValues == null ? (DoubleOrSingle)(j + 1) / (n - 1) : (DoubleOrSingle)textureValues[j + 1]));
                    }

                    var i0 = index0 + (i * rowNodes) + (j * 2);
                    var i1 = i0 + 1;
                    var i2 = index0 + ((((i + 1) * rowNodes) + (j * 2)) % totalNodes);
                    var i3 = i2 + 1;

                    this.triangleIndices.Add(i1);
                    this.triangleIndices.Add(i0);
                    this.triangleIndices.Add(i2);

                    this.triangleIndices.Add(i1);
                    this.triangleIndices.Add(i2);
                    this.triangleIndices.Add(i3);
                }
            }
        }

        public void AddSphere(Point3D center, double radius = 1, int thetaDiv = 32, int phiDiv = 32)
        {
            this.AddEllipsoid(center, radius, radius, radius, thetaDiv, phiDiv);
        }

        public void AddCylinder(Point3D p1, Point3D p2, double diameter, int thetaDiv)
        {
            var n = p2 - p1;
            var l = Math.Sqrt(n.X * n.X + n.Y * n.Y + n.Z * n.Z);
            n.Normalize();
            this.AddCone(p1, n, diameter / 2, diameter / 2, l, true, true, thetaDiv);
        }

        public void AddEllipsoid(Point3D center, double radiusx, double radiusy, double radiusz, int thetaDiv = 20, int phiDiv = 10)
        {
            var index0 = this.Positions.Count;
            var dt = 2 * (DoubleOrSingle)Math.PI / thetaDiv;
            var dp = (DoubleOrSingle)Math.PI / phiDiv;

            for (var pi = 0; pi <= phiDiv; pi++)
            {
                var phi = pi * dp;

                for (var ti = 0; ti <= thetaDiv; ti++)
                {
                    var theta = ti * dt;

                    var x = (DoubleOrSingle)Math.Cos(theta) * (DoubleOrSingle)Math.Sin(phi);
                    var y = (DoubleOrSingle)Math.Sin(theta) * (DoubleOrSingle)Math.Sin(phi);
                    var z = (DoubleOrSingle)Math.Cos(phi);

                    var p = new Point3D(center.X + (DoubleOrSingle)(radiusx * x), center.Y + (DoubleOrSingle)(radiusy * y), center.Z + (DoubleOrSingle)(radiusz * z));
                    this.positions.Add(p);

                    if (this.normals != null)
                    {
                        var n = new Vector3D(x, y, z);
                        this.normals.Add(n);
                    }

                    if (this.textureCoordinates != null)
                    {
                        var uv = new Point(theta / (2 * (DoubleOrSingle)Math.PI), phi / (DoubleOrSingle)Math.PI);
                        this.textureCoordinates.Add(uv);
                    }
                }
            }

            this.AddRectangularMeshTriangleIndices(index0, phiDiv + 1, thetaDiv + 1, true);
        }
        public void AddRectangularMeshTriangleIndices(int index0, int rows, int columns, bool isSpherical = false)
        {
            for (var i = 0; i < rows - 1; i++)
            {
                for (var j = 0; j < columns - 1; j++)
                {
                    var ij = (i * columns) + j;
                    if (!isSpherical || i > 0)
                    {
                        this.triangleIndices.Add(index0 + ij);
                        this.triangleIndices.Add(index0 + ij + 1 + columns);
                        this.triangleIndices.Add(index0 + ij + 1);
                    }

                    if (!isSpherical || i < rows - 2)
                    {
                        this.triangleIndices.Add(index0 + ij + 1 + columns);
                        this.triangleIndices.Add(index0 + ij);
                        this.triangleIndices.Add(index0 + ij + columns);
                    }
                }
            }
        }

        public void AddBox(Point3D center, double xlength, double ylength, double zlength)
        {
            var x = new Vector3D(1, 0, 0);
            var y = new Vector3D(0, 1, 0);
            var z = new Vector3D(0, 0, 1);
            this.AddCubeFace(center, x, z, xlength, ylength, zlength);
            this.AddCubeFace(center, -x, z, xlength, ylength, zlength);
            this.AddCubeFace(center, -y, z, ylength, xlength, zlength);
            this.AddCubeFace(center, y, z, ylength, xlength, zlength);
            this.AddCubeFace(center, z, y, zlength, xlength, ylength);
            this.AddCubeFace(center, -z, y, zlength, xlength, ylength);
        }
        public void AddTriangle(Point3D p0, Point3D p1, Point3D p2)
        {
            var uv0 = new Point(0, 0);
            var uv1 = new Point(1, 0);
            var uv2 = new Point(0, 1);
            this.AddTriangle(p0, p1, p2, uv0, uv1, uv2);
        }

        public void AddTriangle(Point3D p0, Point3D p1, Point3D p2, Point uv0, Point uv1, Point uv2)
        {
            var i0 = this.positions.Count;

            this.positions.Add(p0);
            this.positions.Add(p1);
            this.positions.Add(p2);

            if (this.textureCoordinates != null)
            {
                this.textureCoordinates.Add(uv0);
                this.textureCoordinates.Add(uv1);
                this.textureCoordinates.Add(uv2);
            }

            if (this.normals != null)
            {
                var p10 = p1 - p0;
                var p20 = p2 - p0;
                var w = Vector3D.CrossProduct(p10, p20);
                w.Normalize();
                this.normals.Add(w);
                this.normals.Add(w);
                this.normals.Add(w);
            }

            this.triangleIndices.Add(i0 + 0);
            this.triangleIndices.Add(i0 + 1);
            this.triangleIndices.Add(i0 + 2);
        }


        public void AddPyramid(Point3D center, Vector3D forward, Vector3D up, double sideLength, double height, bool closeBase = false)
        {
            var right = Vector3D.CrossProduct(forward, up);
            var n = forward * (DoubleOrSingle)sideLength / 2;
            up *= (DoubleOrSingle)height;
            right *= (DoubleOrSingle)sideLength / 2;

            var down = -up * 1f / 3;
            var realup = up * 2f / 3;

            var p1 = center - n - right + down;
            var p2 = center - n + right + down;
            var p3 = center + n + right + down;
            var p4 = center + n - right + down;
            var p5 = center + realup;

            this.AddTriangle(p1, p2, p5);
            this.AddTriangle(p2, p3, p5);
            this.AddTriangle(p3, p4, p5);
            this.AddTriangle(p4, p1, p5);
            if (closeBase)
            {
                this.AddTriangle(p1, p3, p2);
                this.AddTriangle(p3, p1, p4);
            }
        }

        public void AddCubeFace(Point3D center, Vector3D normal, Vector3D up, double dist, double width, double height)
        {
            var right = Vector3D.CrossProduct(normal, up);
            var n = normal * (DoubleOrSingle)dist / 2;
            up *= (DoubleOrSingle)height / 2;
            right *= (DoubleOrSingle)width / 2;
            var p1 = center + n - up - right;
            var p2 = center + n - up + right;
            var p3 = center + n + up + right;
            var p4 = center + n + up - right;

            var i0 = this.positions.Count;
            this.positions.Add(p1);
            this.positions.Add(p2);
            this.positions.Add(p3);
            this.positions.Add(p4);
            if (this.normals != null)
            {
                this.normals.Add(normal);
                this.normals.Add(normal);
                this.normals.Add(normal);
                this.normals.Add(normal);
            }

            if (this.textureCoordinates != null)
            {
                this.textureCoordinates.Add(new Point(1, 1));
                this.textureCoordinates.Add(new Point(0, 1));
                this.textureCoordinates.Add(new Point(0, 0));
                this.textureCoordinates.Add(new Point(1, 0));
            }

            this.triangleIndices.Add(i0 + 2);
            this.triangleIndices.Add(i0 + 1);
            this.triangleIndices.Add(i0 + 0);
            this.triangleIndices.Add(i0 + 0);
            this.triangleIndices.Add(i0 + 3);
            this.triangleIndices.Add(i0 + 2);
        }

        public MeshGeometry3D ToMesh(bool freeze = false)
        {
            if (this.triangleIndices.Count == 0)
            {
                var emptyGeometry = new MeshGeometry3D();
                if (freeze)
                {
                    emptyGeometry.Freeze();
                }

                return emptyGeometry;
            }

            if (this.normals != null && this.positions.Count != this.normals.Count)
            {
                throw new InvalidOperationException();
            }

            if (this.textureCoordinates != null && this.positions.Count != this.textureCoordinates.Count)
            {
                throw new InvalidOperationException();
            }

            var mg = new MeshGeometry3D
            {
                Positions = new Point3DCollection(this.positions),
                TriangleIndices = new Int32Collection(this.triangleIndices)
            };
            if (this.normals != null)
            {
                mg.Normals = new Vector3DCollection(this.normals);
            }

            if (this.textureCoordinates != null)
            {
                mg.TextureCoordinates = new PointCollection(this.textureCoordinates);
            }

            if (freeze)
            {
                mg.Freeze();
            }

            return mg;
        }
    }

    public static class Vector3DExtensions
    {
        public static Vector3D FindAnyPerpendicular(this Vector3D n)
        {
            n.Normalize();
            Vector3D u = Vector3D.CrossProduct(new Vector3D(0, 1, 0), n);
            if (u.LengthSquared < 1e-3)
            {
                u = Vector3D.CrossProduct(new Vector3D(1, 0, 0), n);
            }

            return u;
        }

        public static bool IsUndefined(this Vector3D v)
        {
            return double.IsNaN(v.X) && double.IsNaN(v.Y) && double.IsNaN(v.Z);
        }

        /// <summary>
        /// Convert a <see cref="Vector3D"/> to a <see cref="Point3D"/>.
        /// </summary>
        /// <param name="n">
        /// The input vector.
        /// </param>
        /// <returns>
        /// A point.
        /// </returns>
        public static Point3D ToPoint3D(this Vector3D n)
        {
            return new Point3D(n.X, n.Y, n.Z);
        }
    }
}
