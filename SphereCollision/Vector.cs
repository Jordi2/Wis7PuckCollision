using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SphereCollision
{
    struct Vector
    {
        public double X { get; set; }
        public double Y { get; set; }

        public Vector(double x, double y) : this()
        {
            X = x;
            Y = y;
        }

        public static Vector operator +(Vector a, Vector b)
        {
            return new Vector(a.X + b.X, a.Y + b.Y); 
        }

        public static Vector operator -(Vector a, Vector b)
        {
            return new Vector(a.X - b.X, a.Y - b.Y);
        }

        public static Vector operator*(double s, Vector v)
        {
            return new Vector(s * v.X, s * v.Y);
        }

        public static Vector operator *(Vector v, double s)
        {
            return s * v;
        }

        public static double Dot(Vector a, Vector b)
        {
            return a.X * b.X + a.Y * b.Y;
        }

        public double Length
        {
            get{ return Math.Sqrt(X * X + Y * Y); }
        }
    }
}
