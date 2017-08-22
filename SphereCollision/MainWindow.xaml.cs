using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

/*
 * 0889529 TI3A WISKUNDE 7 Alternative Opdracht
 * 
 * Note: Puck placement and vector is random, so it can take a while for the two pucks to make contact, 
 * if it takes too long just close and reload the application for a more favorable lineup.
 */

namespace SphereCollision
{
    public partial class MainWindow : Window
    {
        Random rnd = new Random();
        List<Sphere> spheres = new List<Sphere>();
        Timer timer;

        public MainWindow()
        {
            InitializeComponent();
        }

        
        // Adds a new puck with a random radius at a random position to the screen
        private void AddSphere(Double xDirection, Double yDirection)
        {
            var s = new Sphere();
            s.Radius = 30;  //rnd.NextDouble() * 25 + 5;
            s.Mass = s.Radius * s.Radius; //base mass on size
            s.UIElement = new Ellipse()
            {
                Fill = Brushes.Maroon,
                Stroke = Brushes.Black,
                Width = 2 * s.Radius,
                Height = 2 * s.Radius,
                Margin = new Thickness(-s.Radius)//prevent pucks from leaving screen
            };
            Screen.Children.Add(s.UIElement);
            bool collision = false;
            do
            {
                s.Position = new Vector(
                    rnd.NextDouble() * (Screen.ActualWidth - 2 * s.Radius) + s.Radius,
                    rnd.NextDouble() * (Screen.ActualHeight - 2 * s.Radius) + s.Radius);

                //Don't put a puck atop of another one
                collision = false;
                foreach (var sphere in spheres)
                {
                    if (sphere.Intersects(s))
                    {
                        collision = true;
                        break;
                    }
                }
            } while (collision);

            /* //random direction and speed
            s.Velocity = new Vector(
                rnd.NextDouble() * 100,
                rnd.NextDouble() * 100);*/
            s.Velocity = new Vector(
                xDirection,
                yDirection);
            spheres.Add(s);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //Add one still puck
            AddSphere(0.1, 0.1);

            //Add x moving pucks
            for (int i = 0; i < 1; ++i)
            {
                AddSphere(50.0, 50.0);
            }

            //Start the update timer
            timer = new System.Timers.Timer(50);
            timer.Elapsed += timer_Elapsed;
            timer.Start();
        }

        void timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                Update(0.05);
            });
        }

        void Update(double seconds)
        {
            //Move each sphere
            foreach (var sphere in spheres)
            {
                sphere.Position += seconds * sphere.Velocity;
                //Collision with wall -> bounce of wall
                Double collisionLoss = 0.9;//10% loss of speed after wall-collision
                if ((sphere.Position.X < sphere.Radius && sphere.Velocity.X < 0) || (sphere.Position.X > Screen.ActualWidth - sphere.Radius && sphere.Velocity.X > 0))
                    sphere.Velocity = new Vector(-sphere.Velocity.X * collisionLoss, sphere.Velocity.Y * collisionLoss);
                if ((sphere.Position.Y < sphere.Radius && sphere.Velocity.Y < 0)|| (sphere.Position.Y > Screen.ActualHeight - sphere.Radius && sphere.Velocity.Y > 0))
                    sphere.Velocity = new Vector(sphere.Velocity.X * collisionLoss, -sphere.Velocity.Y * collisionLoss);
            }

            //Check each sphere for a collision
            for(int i = 0; i < spheres.Count; ++i)
            {
                var sphere1 = spheres[i];
                for(int j = i + 1; j < spheres.Count; ++j)
                {
                    var sphere2 = spheres[j];

                    if (sphere1.Intersects(sphere2))
                    {
                        //Positions
                        double p1x = sphere1.Position.X, p1y = sphere1.Position.Y;
                        double p2x = sphere2.Position.X, p2y = sphere2.Position.Y;
                        //Velocities
                        double v1x = sphere1.Velocity.X, v1y = sphere1.Velocity.Y;
                        double v2x = sphere2.Velocity.X, v2y = sphere2.Velocity.Y;
                        //Radii
                        double r1 = sphere1.Radius, r2 = sphere2.Radius;

                        //Move the spheres until they don't collide anymore. 
                        //Math -> |(p1 - t * v1) - (p2 - t * v2)| = r1 + r2, we're looking for the value of t.
                        var backTimeRoot = 0.5 * Math.Sqrt(4 * Math.Pow(p1x * (v1x - v2x) +
                            p2x * (-v1x + v2x) + (p1y - p2y) * (v1y - v2y), 2) -
                            4 * (p1x * p1x + p1y * p1y - 2 * p1x * p2x + p2x * p2x - 2 * p1y * p2y + p2y * p2y -
                            r1 * r1 - 2 * r1 * r2 - r2 * r2) * (v1x * v1x + v1y * v1y - 2 * v1x * v2x + v2x * v2x -
                            2 * v1y * v2y + v2y * v2y));
                        var backTimeSummand = p1x * v1x - p2x * v1x + p1y * v1y - p2y * v1y - p1x * v2x + p2x * v2x - p1y * v2y + p2y * v2y;
                        var backTimeDivisor = v1x * v1x + v1y * v1y - 2 * v1x * v2x + v2x * v2x - 2 * v1y * v2y + v2y * v2y;
                        var backTime = (backTimeSummand - backTimeRoot) / backTimeDivisor;
                        if (backTime < 0)
                            backTime = (backTimeSummand + backTimeRoot) / backTimeDivisor;
                        backTime += 0.001; //compensate for floating point errors

                        sphere1.Position -= sphere1.Velocity * backTime;
                        sphere2.Position -= sphere2.Velocity * backTime;

                        //Decompose v1 in parallel and orthogonal part
                        Vector collisionNormal = sphere1.Position - sphere2.Position;
                        collisionNormal *= 1 / collisionNormal.Length;
                        var v1Dot = Vector.Dot(collisionNormal, sphere1.Velocity);//scalar product
                        var v1Collide = collisionNormal * v1Dot;
                        var v1Remainder = sphere1.Velocity - v1Collide;

                        //Decompose v2 in parallel and orthogonal part
                        var v2Dot = Vector.Dot(collisionNormal, sphere2.Velocity);
                        var v2Collide = collisionNormal * v2Dot;
                        var v2Remainder = sphere2.Velocity - v2Collide;

                        //Calculate the collision, this is the physics part
                        var v1Length = v1Collide.Length * Math.Sign(v1Dot);
                        var v2Length = v2Collide.Length * Math.Sign(v2Dot);
                        //The formula we need is (Mass1 * Velocity1 + Mass2 * Velocity2) / (Mass1 + Mass2)
                        //This formula would normally half the total speed (Assuming mass of both pucks was equal), 
                        //We don't want any loss though so we do a times 2 on the formula
                        var commonVelocity = 2 * (sphere1.Mass * v1Length + sphere2.Mass * v2Length) / (sphere1.Mass + sphere2.Mass);
                        var v1LengthAfterCollision = commonVelocity - v1Length;
                        var v2LengthAfterCollision = commonVelocity - v2Length;
                        v1Collide = v1Collide * (v1LengthAfterCollision / v1Length);
                        v2Collide = v2Collide * (v2LengthAfterCollision / v2Length);

                        //Recombine the velocity
                        sphere1.Velocity = v1Collide + v1Remainder;
                        sphere2.Velocity = v2Collide + v2Remainder;

                        sphere1.Position += backTime * sphere1.Velocity;
                        sphere2.Position += backTime * sphere2.Velocity;
                    }
                }
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            timer.Stop();
        }
    }
}
