using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OMRMaison;
using Ghostscript.NET.Rasterizer;
using Ghostscript.NET;
using GhostscriptSharp.Settings;
using System.IO;
using System.Reflection;

/// This file is part of Yermangderrff - OMRMaison.

/// OMRMaison is free software: you can redistribute it and/or modify
/// it under the terms of the GNU General Public License as published by
/// the Free Software Foundation, either version 3 of the License, or
/// (at your option) any later version.

/// OMRMaison is distributed in the hope that it will be useful,
/// but WITHOUT ANY WARRANTY; without even the implied warranty of
/// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
/// GNU General Public License for more details.

/// You should have received a copy of the GNU General Public License
/// along with OMRMaison.  If not, see <http://www.gnu.org/licenses/>.

namespace OMRTests
{
    public class Program
    {


        public static void Main(string[] args)
        {
            /*try
            {
                List<Image> imagesPdf = Program.exportPdfToImages("HelloWorld.pdf", 72);
                Bitmap b = new Bitmap("testRond.PNG");

                //Rectangle r1 = new Rectangle(23, 5, 40, 40);
                //Rectangle r2 = new Rectangle(514, 15, 44, 44);
                //Rectangle r3 = new Rectangle(20, 355, 43, 48);
                //Rectangle r4 = new Rectangle(513, 360, 43, 44);

                Program.ReadingMarksCoord(imagesPdf[0], 0, 0, 100, 100);
                Program.ReadingMarksCoord(imagesPdf[0], 1529, 0, 100, 100);
                Program.ReadingMarksCoord(imagesPdf[0], 0, 2200, 200, 139);
                Program.ReadingMarksCoord(imagesPdf[0], 1529, 2000, 100, 100);

                //PixelsCircle pc1;
                //PixelsCircle pc2;
                //PixelsCircle pc3;
                //PixelsCircle pc4;

                //Console.WriteLine("Zone haut-gauche: ");
                //if ((pc1 = Detection.getPixelsNoirsRond(b, r1, 10, false)) != null)
                //    Console.WriteLine(pc1.ToString());

                //Console.WriteLine("Zone haut-droite: ");
                //if ((pc2 = Detection.getPixelsNoirsRond(b, r2, 10, false)) != null)
                //    Console.WriteLine(pc2.ToString());

                //Console.WriteLine("Zone bas-gauche: ");
                //if ((pc3 = Detection.getPixelsNoirsRond(b, r3, 10, false)) != null)
                //    Console.WriteLine(pc3.ToString());

                //Console.WriteLine("Zone bas-droite: ");
                //if ((pc4 = Detection.getPixelsNoirsRond(b, r4, 10, true)) != null)
                //    Console.WriteLine(pc4.ToString());

                
                //Console.WriteLine(imagesPdf.Count.ToString());

                //Program.ReadingMarksCoord(imagesPdf[0]);

                Console.ReadKey();
            }
            catch (ArgumentException ae)
            {
                Console.WriteLine(ae.ToString());
                Console.ReadKey();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                Console.ReadKey();
            }*/
        }
    }
}
