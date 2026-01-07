using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrafficEscape2
{

    public class PowerUp
    {
        public Image Visual { get; }
        public double X { get; set; }
        public double Y { get; set; }
        public bool IsActive { get; set; } = true;

        public PowerUp(double x, double y)
        {
            X = x;
            Y = y;

            Visual = new Image
            {
                Source = "bullet.png",
                Aspect = Aspect.AspectFit,
                WidthRequest = 40,
                HeightRequest = 40
            };
        }
    }
}

