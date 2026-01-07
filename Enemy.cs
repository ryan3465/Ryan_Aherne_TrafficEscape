using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrafficEscape2
{
    internal class Enemy
    {
        // -------------------- PROPERTIES --------------------

        // Enemy's current X position
        public double X { get; private set; }

        // Enemy's current Y position
        public double Y { get; private set; }

        // Enemy size (used for collision detection and visual scaling)
        public double Size { get; private set; } = 90;

        // Visual representation of the enemy (Image displayed on canvas)
        public Image Visual { get; private set; }

        // True if the enemy has moved off-screen and should be removed
        public bool isOffScreen { get; private set; } = false;

        // -------------------- MOVEMENT --------------------

        // Current velocity in X and Y directions
        private double velocityX;
        private double velocityY;

        // Base speed of the enemy
        public double speed = 6.0;

        // Random number generator for selecting random car images
        private Random random = new Random();

        // -------------------- CONSTRUCTOR --------------------

        // Creates a new enemy at the specified X,Y position
        // Assigns a random car image and sets initial velocity
        public Enemy(double x, double y)
        {
            X = x; // Starting X
            Y = y; // Starting Y

            // Pick a random car type (1, 2, or 3)
            int whichCar = random.Next(1, 4);

            // Default image source
            String imgSrc = $"enemycar{whichCar}.jpg";
            Size = 90;

            // Assign image and adjust size based on which car
            if (whichCar == 1)
            {
                imgSrc = "enemycar.jpg";
                Size = 90;
            }
            else if (whichCar == 2)
            {
                imgSrc = $"enemycar{whichCar}.PNG";
                Size = 100;
            }
            else
            {
                imgSrc = $"enemycar{whichCar}.PNG";
                Size = 110;
            }

            // Create the Image object for display on canvas
            Visual = new Image()
            {
                Source = imgSrc,
                WidthRequest = Size * 0.75,  // Scale width slightly
                HeightRequest = Size,
            };

            // Initial movement: moving down the screen
            velocityX = 0;
            velocityY = speed;
        }

        // -------------------- UPDATE METHOD --------------------

        // Updates the enemy's position each frame
        // Handles movement and detects when enemy moves off-screen
        public void Update(double screenWidth, double screenHeight)
        {
            // Move the enemy along Y-axis
            Y += velocityY;

            // Prevent enemy from going above the top of the screen
            if (Y < Size / 2)
            {
                velocityY = -velocityY; // Bounce downward if necessary
                Y = Math.Clamp(Y, Size / 2, screenHeight - Size / 2); // Keep inside bounds
            }

            // Check if enemy has moved below the screen
            if (Y > screenHeight + Size)
            {
                isOffScreen = true; // Mark for removal
            }
        }

        // -------------------- MOVE TOWARDS TARGET --------------------

        // Adjusts enemy velocity to move toward a specific X,Y target
        public void MoveTowards(double targetX, double targetY)
        {
            // Compute distance vector from enemy to target
            double dx = targetX - X;
            double dy = targetY - Y;

            // Euclidean distance
            double distance = Math.Sqrt(dx * dx + dy * dy);

            // Normalize vector and multiply by speed
            if (distance > 0)
            {
                velocityX = (dx / distance) * speed;
                velocityY = (dy / distance) * speed;
            }
        }
    }
}
