
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrafficEscape2
{
    internal class Enemy
    {
        public double X { get; private set; }
        public double Y { get; private set; }
        public double Size { get; private set; } = 90;
        // public BoxView Visual { get; private set; }

        public Image Visual { get; private set; }

        public bool isOffScreen { get; private set; } = false;

      
        private double velocityX;
        private double velocityY;
        private double speed = 6.0;
        private Random random = new Random();
        private DateTime lastDirectionChange;
        private int directionChangeInterval = 2000; // Change direction every 2 seconds

        // Creates a new enemy at the specified position.
        // The enemy will have a random initial direction.
        public Enemy(double x, double y)
        {
            X = x;
            Y = y;
            
           
            int whichCar = random.Next(1, 4);
            
            String imgSrc = $"enemycar{whichCar}.jpg";
            Size = 90;

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


            Visual = new Image()
            {
                Source = imgSrc,
                WidthRequest = Size *0.75,
                HeightRequest = Size,
            };

            
            velocityX = 0;
            velocityY = speed;
        }

        // Updates the enemy's position. Should be called every frame.
        // The enemy moves in its current direction and periodically changes direction.
        public void Update(double screenWidth, double screenHeight)
        {
            // Move in current direction
            
            Y += velocityY;
            

            if (Y < Size / 2 )
            {
               
                velocityY = -velocityY;
                Y = Math.Clamp(Y, Size / 2, screenHeight - Size / 2);
            }

            if (Y > screenHeight + Size)
            {
                isOffScreen = true;   
            }

            // Periodically change direction for more interesting movement
            // if ((DateTime.Now - lastDirectionChange).TotalMilliseconds > directionChangeInterval)
            // {
            //    ChangeDirection();
            //    lastDirectionChange = DateTime.Now;
            //   }
        }

        // Changes the enemy's direction to a new random direction.
       // private void ChangeDirection()
       // {
            // Generate random angle
         //   double angle = random.NextDouble() * 2 * Math.PI;

            // Convert to velocity components
         //   velocityX = Math.Cos(angle) * speed;
         //   velocityY = Math.Sin(angle) * speed;
       // }


        // Alternative update method: Makes the enemy move towards a target (like the player).
        // Maybe different types of enemies could use this behaviour.
        // This is not currently used but demonstrates how to create homing enemies.
        public void MoveTowards(double targetX, double targetY)
        {
            double dx = targetX - X;
            double dy = targetY - Y;
            double distance = Math.Sqrt(dx * dx + dy * dy);

            if (distance > 0)
            {
                velocityX = (dx / distance) * speed;
                velocityY = (dy / distance) * speed;
            }
        }
    
}
}
