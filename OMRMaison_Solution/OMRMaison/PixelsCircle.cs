using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMRMaison
{
    public class PixelsCircle
    {
        public int x { get; set; }
        public int y { get; set; }
        public int diametre { get; set; }

        public PixelsCircle(int x, int y, int diam)
        {
            this.x = x;
            this.y = y;
            this.diametre = diam;
        }
    }
}
