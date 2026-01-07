using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrafficEscape2
{
    internal class Player
    {
        // -------------------- PROPERTIES --------------------

        // Current X position of the player
        public double X { get; private set; }

        // Current Y position of the player
        public double Y { get; private set; }

        // Size of the player's car (used for collision detection and scaling)
        public double Size { get; private set; } = 90;

        // Visual representation of the player (Image displayed on canvas)
        public Image Visual { get; private set; }

        // Current rotation of the player's car in degrees
        public double Rotation
        {
            get
            {
                return Visual.Rotation;
            }
        }

        // -------------------- CONSTRUCTOR --------------------

        // Initializes a new player at the given X,Y coordinates
        // Creates the Image object for the player's car
        public Player(double x, double y)
        {
            X = x; // Starting X position
            Y = y; // Starting Y position

            // Create the player's car image
            Visual = new Image()
            {
                Source = "car2.png",     // Image file for player's car
                WidthRequest = Size * 0.75, // Scale width slightly
                HeightRequest = Size,    // Full size for height
            };
        }

        // -------------------- METHODS --------------------

        // Moves the player instantly to a new X,Y position
        // Could be extended to animate movement in the future
        public void MoveTo(double targetX, double targetY)
        {
            X = targetX;
            Y = targetY;
        }

        // Rotates the player's image to the given angle (in degrees)
        // Useful for animations or turning effects
        public void RotatePlayer(double angle)
        {
            Visual.Rotation = angle;
        }
    }
}