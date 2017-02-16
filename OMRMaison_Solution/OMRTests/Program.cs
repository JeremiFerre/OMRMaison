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
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;

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
        static void Main(string[] args)
        {
            // Program.AnalyseCopies("Doc.pdf", 60, 10);

            // Test de l'analyse de rond
            Bitmap b = (Bitmap) Image.FromFile("testRond.png");
            Rectangle r = new Rectangle(1320, 0, 333, 265);

            List<List<int>> matriceTest = Detection.getMatricePixelsNoirs(b, r, 10);

            Console.WriteLine(Detection.getCircleFromFreeZone(matriceTest, r, 10).ToString());
            Console.ReadKey();

            /*
            int taille = int.MaxValue - 1832911888; //Taille maximale d'un tableau c#...
            Console.WriteLine(taille);
            string[] tab = new string[taille];
            Console.ReadKey();*/
        }

        //Methode FONCTIONNELLE de conversion de pdf en images
          public static List<System.Drawing.Image> exportPdfToImages(string file)
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
                        img = rasterizer.GetPage(200, 200, i);
                        Bitmap b = new Bitmap(img, new System.Drawing.Size(1654, 2339));

                        output.Add(b);
                    }

                    rasterizer.Close();
                }

                return output;
            }


            //Methode chargée d'acquérir les coordonnées(X,Y) des marques OMR de lecture (Récursive #Pallez)
            /*
              public static PixelsCircle ReadingMarksCoord(System.Drawing.Image image, int x, int y, int width, int height)
             {
                 if (width + x >= image.Width && height + y >= image.Height)
                 {
                     Console.WriteLine("Pas de rond trouvé");
                     return null;
                 }

                 Rectangle coin = new Rectangle(x, y, width, height);
                 Bitmap b = new Bitmap(image);
                 PixelsCircle mark = Detection.getPixelsNoirsRond(b, coin, 10);

                 if (mark != null)
                 {
                     Console.WriteLine(mark.ToString());
                     return mark;
                 }
                 else if (width + x + 10 >= image.Width)
                 {
                     return ReadingMarksCoord(image, x, y, width, height + 10);
                 }
                 else if (height + y + 10 >= image.Height)
                 {
                     return ReadingMarksCoord(image, x, y, width + 10, height);
                 }
                 else
                     return ReadingMarksCoord(image, x, y, width + 10, height + 10);
             }
             */


            public static List<PixelsCircle> ReadingMarksCoord(System.Drawing.Image image)
            {

                Rectangle analyse = new Rectangle(0, 0, 100, 100);
                List<PixelsCircle> marks = new List<PixelsCircle>();
                Bitmap bImage = new Bitmap(image);

                while (analyse.Y < image.Height / 2 - 100)
                {
                    while (analyse.X < image.Width / 2 - 100)
                    {

                        PixelsCircle mark = Detection.getPixelsNoirsRond(bImage, analyse, 10);

                        if (mark != null)
                        {
                            marks.Add(mark);
                            break;
                        }

                        analyse.X += 50;
                    }

                    if (marks.Count == 1) break;
                    else
                    {
                        analyse.Y += 50;
                        analyse.X = 0;
                    }
                }

                analyse = new Rectangle(image.Width / 2, 0, 100, 100);

                while (analyse.Y < image.Height / 2 - 100)
                {
                    while (analyse.X < image.Width - 100)
                    {

                        PixelsCircle mark = Detection.getPixelsNoirsRond(bImage, analyse, 10);

                        if (mark != null)
                        {
                            marks.Add(mark);
                            break;
                        }

                        analyse.X += 50;
                    }

                    if (marks.Count == 2) break;
                    else
                    {
                        analyse.Y += 50;
                        analyse.X = image.Width / 2;
                    }
                }

                return marks;
            }


            //Methode chargée de recadrer la copie afin d'éviter les erreurs liés au décalage de l'image
            public static Bitmap recadrage(Bitmap bmp, List<PixelsCircle> circles)
            {
                if ((circles[0].y - circles[1].y == 0))
                {
                    return bmp;
                }

                double distHyp = Math.Sqrt((Math.Pow(circles[1].y - circles[0].y, 2) + Math.Pow(circles[1].x - circles[0].x, 2)));
                double rapport = ((float)(circles[0].y - circles[1].y)) / distHyp;
                double angle = Math.Asin(rapport) * (180 / Math.PI);

                Bitmap b = Program.rotation(bmp, (float)angle);

                //Redetection des ronds
                List<PixelsCircle> marques = Program.ReadingMarksCoord(b);

                Bitmap cp = b.Clone(new Rectangle(marques[0].x, marques[0].y, b.Width - marques[0].x, b.Height - marques[0].y), b.PixelFormat);
                Graphics g = Graphics.FromImage(cp);
                g.TranslateTransform(100, 100);

                return new Bitmap(1654, 2339, g);
            }

            public static Bitmap rotation(Bitmap b, float angle)
            {
                //create a new empty bitmap to hold rotated image
                Bitmap returnBitmap = new Bitmap(b.Width, b.Height);
                //make a graphics object from the empty bitmap
                using (Graphics g = Graphics.FromImage(returnBitmap))
                {
                    //move rotation point to center of image
                    g.TranslateTransform((float)b.Width / 2, (float)b.Height / 2);
                    //rotate
                    g.RotateTransform(angle);
                    //move image back
                    g.TranslateTransform(-(float)b.Width / 2, -(float)b.Height / 2);
                    //draw passed in image onto graphics object
                    g.DrawImage(b, new System.Drawing.Point(0, 0));
                }
                return returnBitmap;
            }

            //Methode permettant d'identifier l'etudiant
            public static string getIdEtudiant(Bitmap croppedCopiePage1, int seuilRemplissage, int nuancesGrisAdmise)
            {
                Rectangle zoneAnalyse = new Rectangle(290, 500, 40, 40);
                float pourcentageNoirCase;

                string retour = "";

                for (int x = 0; x < 7; x++)
                {
                    for (int i = 0; i < 9; i++)
                    {
                        pourcentageNoirCase = Detection.getNbPixelsNoirs(croppedCopiePage1, zoneAnalyse, nuancesGrisAdmise);
                        if (pourcentageNoirCase >= seuilRemplissage)
                        {
                            retour += i;
                            break;
                        }
                        zoneAnalyse.Y += 60;
                    }
                    zoneAnalyse.Y = 500;
                    zoneAnalyse.X += 80;
                }

                return retour;
            }

            //Methode permettant de récupérer le numéro de la copie
            public static Identifiant getNumCopie(Bitmap croppedCopiePage1, int seuilRemplissage, int nuancesGrisAdmise)
            {
                List<int> retour = new List<int>();
                Rectangle zoneAnalyse = new Rectangle(647, 110, 40, 40);
                float pourcentageNoirCase;
                string binaire = "";

                for (int x = 2; x >= 0; x--)
                {
                    if (x == 0 || x == 1)
                    {
                        binaire = "";
                    }

                    for (int y = 0; y < 9; y++)
                    {
                        if (x == 0 && y == 6)
                        {
                            binaire = "";
                        }

                        pourcentageNoirCase = Detection.getNbPixelsNoirs(croppedCopiePage1, zoneAnalyse, nuancesGrisAdmise);

                        if (pourcentageNoirCase >= seuilRemplissage)
                        {

                            binaire += "1";
                        }
                        else
                        {
                            binaire += "0";
                        }

                        zoneAnalyse.X += 40;

                        if (x == 0 && y == 5)
                        {
                            retour.Add(Convert.ToInt32(binaire, 2));
                        }
                    }

                    zoneAnalyse.X = 647;
                    zoneAnalyse.Y += 40;

                    if (x == 1 || x == 2)
                    {
                        retour.Add(Convert.ToInt32(binaire, 2));
                    }

                }
                return new Identifiant(retour[0], retour[1], retour[2]);
            }

            public static bool contientPage(int id, List<System.Drawing.Image> listePage, int seuilRemplissage, int nuancesGrisAdmise)
            {
                foreach (System.Drawing.Image page in listePage)
                {
                    Identifiant idPage = getNumCopie(new Bitmap(page), seuilRemplissage, nuancesGrisAdmise);
                    if (idPage.idPage == id)
                    {
                        return true;
                    }
                }
                return false;
            }
        

            public static bool AnalyseCopies(string file, int seuilRemplissage, int nuancesGrisAdmise)
            {

                List<System.Drawing.Image> copies = exportPdfToImages(file);
                List<System.Drawing.Image> copiesRecadrees = new List<System.Drawing.Image>();

                if (copies == null)
                {
                    Console.WriteLine("L'ouverture a échoué.");
                    return false;
                }

                foreach (System.Drawing.Image copie in copies)
                {
                    List<PixelsCircle> marks = ReadingMarksCoord(copie);
                    copiesRecadrees.Add((System.Drawing.Image)recadrage(new Bitmap(copie), marks));
                }
                
                return true;

            }
        }


    public class Identifiant
    {
        public int idSujet { get; set; }
        public int idCopie { get; set; }
        public int idPage { get; set; }

        public Identifiant(int idS, int idC, int idP)
        {
            idSujet = idS;
            idCopie = idC;
            idPage = idP;
        }


        public Bitmap drawID()
        {
            Bitmap image = new Bitmap(360, 120);
            Graphics g = Graphics.FromImage(image);
            Brush br = new SolidBrush(Color.Black);
            Pen p = new Pen(Color.Black, 2);
            p.Alignment = PenAlignment.Inset;


            string strIdS = intToBitString(idSujet, 9);
            string strIdC = intToBitString(idCopie, 12);
            string strIdP = intToBitString(idPage, 6);

            for (int i = 0; i < 9; i++)
            {
                if (strIdS[i] == '0')
                    g.DrawRectangle(p, 40 * i, 0, 40, 40);
                else
                {
                    g.DrawRectangle(p, 40 * i, 0, 40, 40);
                    g.FillRectangle(br, 40 * i, 0, 40, 40);
                }

            }

            if (strIdC[0] == '0')
                g.DrawRectangle(p, 240, 80, 40, 40);
            else
            {
                g.DrawRectangle(p, 240, 80, 40, 40);
                g.FillRectangle(br, 240, 80, 40, 40);
            }
            if (strIdC[1] == '0')
                g.DrawRectangle(p, 280, 80, 40, 40);
            else
            {
                g.DrawRectangle(p, 280, 80, 40, 40);
                g.FillRectangle(br, 280, 80, 40, 40);
            }
            if (strIdC[2] == '0')
                g.DrawRectangle(p, 320, 80, 40, 40);
            else
            {
                g.DrawRectangle(p, 320, 80, 40, 40);
                g.FillRectangle(br, 320, 80, 40, 40);
            }

            for (int i = 2; i < 12; i++)
            {
                if (strIdC[i] == '0')
                    g.DrawRectangle(p, 40 * (i - 3), 40, 40, 40);
                else
                {
                    g.DrawRectangle(p, 40 * (i - 3), 40, 40, 40);
                    g.FillRectangle(br, 40 * (i - 3), 40, 40, 40);
                }

            }

            for (int i = 0; i < 6; i++)
            {
                if (strIdP[i] == '0')
                    g.DrawRectangle(p, 40 * i, 80, 40, 40);
                else
                {
                    g.DrawRectangle(p, 40 * i, 80, 40, 40);
                    g.FillRectangle(br, 40 * i, 80, 40, 40);
                }

            }

            br.Dispose();
            p.Dispose();
            g.Dispose();

            return image;
        }

        public static Bitmap drawIdEtu()
        {
            Bitmap b = new Bitmap(656, 580);
            Graphics g = Graphics.FromImage(b);
            Brush br = new SolidBrush(Color.Black);
            Pen p = new Pen(Color.Black, 2);
            p.Alignment = PenAlignment.Inset;
            Font f = new Font("Verdana", 15);


            for (int i = 0; i < 10; i++)
            {
                String s = "";
                s += i;
                for (int j = 1; j < 9; j++)
                    g.DrawString(s, f, br, 80 * j, (i * 60) + 5);
            }

            for (int i = 1; i < 9; i++)
                for (int j = 0; j < 10; j++)
                    g.DrawRectangle(p, (i - 0.5f) * 80, j * 60, 40, 40);

            br.Dispose();
            p.Dispose();
            g.Dispose();
            f.Dispose();

            return b;
        }

        private static string intToBitString(int value, int nbBit)
        {
            string s = "";
            for (int i = nbBit - 1; i >= 0; i--)
            {
                if ((value - Math.Pow(2, i)) >= 0)
                {
                    s += "1";
                    value -= (int)Math.Pow(2, i);
                }
                else
                    s += "0";
            }
            return s;
        }
        
    }
}
