using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrafficEscape2
{
    internal class Player
    {
        public double X { get; private set; }
        public double Y { get; private set; }
        public double Size { get; private set; } = 90;

        public Image Visual { get; private set; }

        public double Rotation
        {
            get
            {
                return Visual.Rotation;
            }
        }

        public Player(double x, double y)
        {
            X = x;
            Y = y;


            Visual = new Image()
            {
                Source = "car2.png",
                WidthRequest = Size,
                HeightRequest = Size,

            };
        }


        // Moves the player to a new position instantly.
        // In a more advanced version, this could animate the movement.    
        public void MoveTo(double targetX, double targetY)
        {
            X = targetX;
            Y = targetY;
        }

        // Rotates the player to a specified angle in degrees.
        public void RotatePlayer(double angle)
        {
            Visual.Rotation = angle;
        }
    }
}

