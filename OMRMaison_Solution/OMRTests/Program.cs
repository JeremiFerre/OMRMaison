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
        public static List<System.Drawing.Image> exportPdfToImages(string file, int dpi)
        {
            string path = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            List<System.Drawing.Image> output = new List<System.Drawing.Image>();
            System.Drawing.Image img;

            GhostscriptRasterizer rasterizer = null;
            GhostscriptVersionInfo vesion = new GhostscriptVersionInfo(new Version(0, 0, 0), path + @"\gsdll32.dll", string.Empty, Ghostscript.NET.GhostscriptLicense.GPL);

            using (rasterizer = new GhostscriptRasterizer())
            {
                rasterizer.Open(file, vesion, false);

                for (int i = 1; i <= rasterizer.PageCount; i++)
                {
                    //string pageFilePath = Path.Combine(outputPath, Path.GetFileNameWithoutExtension(file) + "-p" + i.ToString() + ".bmp");

                    img = rasterizer.GetPage(dpi, dpi, i);

                    output.Add(img);
                }

                rasterizer.Close();
            }

            return output;
        }

        public static List<PixelsCircle> ReadingMarksCoord(System.Drawing.Image image)
        {
            List<PixelsCircle> marks = new List<PixelsCircle>();
            Rectangle coinHautGauche = new Rectangle(0, 0, 100, 100);
            Bitmap b = new Bitmap(image);
            marks.Add(Detection.getPixelsNoirsRond(b, coinHautGauche, 10, true));

            Console.WriteLine(marks[0].ToString());

            return null;
        }

        public static void Main(string[] args)
        {
            try
            {
                Bitmap b = new Bitmap("testRond.PNG");
                Rectangle r1 = new Rectangle(23, 5, 40, 40);
                Rectangle r2 = new Rectangle(514, 15, 44, 44);
                Rectangle r3 = new Rectangle(20, 355, 43, 48);
                Rectangle r4 = new Rectangle(513, 360, 43, 44);

                PixelsCircle pc1;
                PixelsCircle pc2;
                PixelsCircle pc3;
                PixelsCircle pc4;

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

                List<System.Drawing.Image> imagesPdf = Program.exportPdfToImages("HelloWorld.pdf", 50);
                Console.WriteLine(imagesPdf.Count.ToString());

                Program.ReadingMarksCoord(imagesPdf[0]);

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
            }
        }
    }
}
