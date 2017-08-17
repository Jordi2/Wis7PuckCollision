using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace SphereCollision
{
    class Sphere
    {
        private Vector position;
        public Vector Position {
            get { return position; }
            set
            {
                position = value;
                if (UIElement != null)
                {
                    Canvas.SetTop(UIElement, position.Y);
                    Canvas.SetLeft(UIElement, position.X);
                }
            }
        }
        public Vector Velocity { get; set; }
        public double Mass { get; set; }
        public double Radius { get; set; }

        public Ellipse UIElement { get; set; }

        public bool Intersects(Sphere other)
        {
            double d;
            return Intersects(other, out d);
        }

        public bool Intersects(Sphere other, out double intersectionDistance)
        {
            intersectionDistance = (Position - other.Position).Length - Radius - other.Radius;
            return intersectionDistance < 0;
        }
    }
}
