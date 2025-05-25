using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace render.Models
{
    public class Class1
    {
        public float x { get; set; }
        public float y { get; set; }
        public float z { get; set; }
        public double intensity { get; set; }

        public Class1(float x, float y, float z, double intensity)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.intensity = intensity;
        }
    }

}
