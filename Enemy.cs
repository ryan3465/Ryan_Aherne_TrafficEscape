
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
        

        public Image Visual { get; private set; }

        public bool isOffScreen { get; private set; } = false;

      
        private double velocityX;
        private double velocityY;
        public double speed = 6.0;
        private Random random = new Random();
        

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

        }

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
