using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrafficEscape2
{
    public class Bullet
    {
        public double X { get; private set; }
        public double Y { get; private set; }
        public BoxView Visual { get; private set; }

        private double velocityX;
        private double velocityY;
        private double speed = 8.0;

        public Bullet(double x, double y, double directionX, double directionY)
        {
            X = x;
            Y = y;

            // Create visual representation
            Visual = new BoxView
            {
                WidthRequest = 6,
                HeightRequest = 20,
                Color = Colors.Yellow,
                CornerRadius = 3
            };
            velocityX = directionX * speed;
            velocityY = directionY * speed;
            double angle = Math.Atan2(directionY, directionX) * 180 / Math.PI;
            Visual.Rotation = angle + 90;
        }


        // Updates the bullet's position. Should be called every frame.
        // The bullet moves in a straight line at constant speed.
        public void Update()
        {
            X += velocityX;
            Y += velocityY;




        }


        // Checks if the bullet is still within the game boundaries.
        public bool IsOnScreen(double screenWidth, double screenHeight)
        {
            return X >= 0 && X <= screenWidth && Y >= 0 && Y <= screenHeight;
        }
    }
}
