using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

namespace OMRMaison
{
    public class Detection
    {
        //Retourne une liste des valeurs RGB de chaque pixel de la zone de l'image demandée
        private static Dictionary<int, List<byte>> getZonePixels(Bitmap bmp, Rectangle Zone, bool DisposeAfter)
        {
            // Create a new bitmap picture to work on it
            System.Drawing.Imaging.BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), System.Drawing.Imaging.ImageLockMode.ReadWrite, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            int scanline = bmpData.Stride; // stride (or scanline) is the widthof a pixels row in this picture
            IntPtr scan0 = bmpData.Scan0; // Scan0 indicate where is the first pixel in the memory
            Dictionary<int, List<byte>> pixelsList = new Dictionary<int, List<byte>>();

            unsafe
            {
                // "p" will be the pointer
                byte* p = (byte*)scan0.ToPointer();
                int nOffset = scanline - 3 * Zone.Width;
                byte r; // red
                byte g; // green
                byte b; // blue
                int y, x;
                int nPixel = 1;

                // Point de départ dans le tableau de pixel en fonction de la zone demandée et de la taille de l'image :
                p += 3 * (Zone.X + Zone.Y * bmp.Width) + Zone.Y;

                for (y = 0; y < Zone.Height; y++)
                {
                    for (x = 0; x < Zone.Width; x++)
                    {
                        pixelsList.Add(nPixel, new List<byte>());

                        b = *p;
                        pixelsList[nPixel].Add(b);
                        p++;
                        // Recupère la valeur, puis déplace le pointeur de 1
                        g = *p;
                        pixelsList[nPixel].Add(g);
                        p++;
                        
                        r = *p;
                        pixelsList[nPixel].Add(r);
                        p++;
                        
                        nPixel++;
                    }
                    p += nOffset;
                }
            }

            bmp.UnlockBits(bmpData);

            if (DisposeAfter) bmp.Dispose();

            return pixelsList;
        }

        //Retourne un pourcentage de pixels proches du noirs dans une zone selon un seuil donnant le pourcentage de couleur moins noire possible 
        public static float getNbPixelsNoirs(Bitmap src, Rectangle zone, int erreur, bool DisposeAfter)
        {
            Dictionary<int, List<byte>> listePixels = Detection.getZonePixels(src, zone, DisposeAfter);
            int couleurValide = 0;
            int nbPixels = 0;

            for (int i = 1; i <= listePixels.Count; i++)
            {
                couleurValide = 0;
                foreach (byte b in listePixels[i])
                {
                    Console.WriteLine("Pixel: " + b.ToString());
                    if (b <= erreur)
                    {
                        couleurValide++;
                    }
                }
                Console.WriteLine("CouleurValide: " + couleurValide);
                if (couleurValide == 3)
                {
                    nbPixels++;
                }
                Console.WriteLine("\nNombre de pixels: " + nbPixels);
            }

            int totalPixels = zone.Height * zone.Width;

            float result = (((float)nbPixels / (float)totalPixels) * 100);

            return result;
        }

        private static int getNbPixelsNoirs(List<List<byte>> listePixels, int erreur)
        {
            int couleurValide = 0;
            int nbPixels = 0;

            foreach (List<byte> lb in listePixels)
            {
                couleurValide = 0;
                foreach (byte b in lb)
                {   
                    if (b <= erreur)
                    {
                        couleurValide++;
                    }
                }
                if (couleurValide == 3)
                {
                    nbPixels++;
                }
            }
            return nbPixels;
        }

        private static int positionPremierPixelNoir(List<List<byte>> lignePixels, int erreur)
        {
            int couleurValide = 0;
            int posPixels = 0;

            foreach (List<byte> lb in lignePixels)
            {
                couleurValide = 0;
                foreach (byte b in lb)
                {
                    if (b <= erreur)
                    {
                        couleurValide++;
                    }
                }
                if (couleurValide == 3)
                {
                    return posPixels;
                }
                posPixels++;
            }

            return -1;
        }

        public static PixelsCircle getPixelsNoirsRond(Bitmap src, Rectangle zone, int erreur, bool DisposeAfter)
        {
            Dictionary<int, List<byte>> listePixels = Detection.getZonePixels(src, zone, DisposeAfter);

            int maxPixelsNoirsLigne = 0;
            int indexLigne1 = 0, indexLigneN = 0, indexLigneMilieu = 0;

            //Transfert de chaque ligne de pixels dans des listes
            List<List<List<byte>>> lignePixels = new List<List<List<byte>>>();
            int compteurListe = 0, c = 1;
            lignePixels.Add(new List<List<byte>>());

            for (int i = 1; i <= listePixels.Count; ++i)
            {
                lignePixels[compteurListe].Add(listePixels[i]);
                c++;
                if (c == (zone.Width + 1) && i != listePixels.Count)
                {
                    c = 1;
                    compteurListe++;
                    lignePixels.Add(new List<List<byte>>());
                }
            }

            //Récupération de la ligne du milieu du cercle par maxima
            for (int z = 0; z < lignePixels.Count; z++)
            {
                if (getNbPixelsNoirs(lignePixels[z], erreur) > maxPixelsNoirsLigne)
                {
                    maxPixelsNoirsLigne = getNbPixelsNoirs(lignePixels[z], erreur);
                    indexLigneMilieu = z;
                }
            }

            //Verification existance cercle vers le haut du diamètre
            for (int y = indexLigneMilieu; y >= 0; y--)
            {
                if ((positionPremierPixelNoir(lignePixels[y - 1], erreur) - positionPremierPixelNoir(lignePixels[y], erreur) > 5)
                    || (getNbPixelsNoirs(lignePixels[y], erreur) < getNbPixelsNoirs(lignePixels[y - 1], erreur)))
                {
                    Console.WriteLine("Cette zone ne contient pas de rond");
                    return null;
                }
                if (getNbPixelsNoirs(lignePixels[y], erreur) == 0)
                {
                    indexLigne1 = y + 1;
                    break;
                }
            }

            //Verification existance cercle vers le bas du diamètre
            for (int y = indexLigneMilieu; y < lignePixels.Count; y++)
            {
                if ((positionPremierPixelNoir(lignePixels[y + 1], erreur) - positionPremierPixelNoir(lignePixels[y], erreur) > 5)
                    || (getNbPixelsNoirs(lignePixels[y], erreur) < getNbPixelsNoirs(lignePixels[y + 1], erreur)))
                {
                    Console.WriteLine("Cette zone ne contient pas de rond");
                    return null;
                }
                if (getNbPixelsNoirs(lignePixels[y], erreur) == 0)
                {
                    indexLigneN = y - 1;
                    break;
                }
            }

            //Vérification Cercle finale
            if (getNbPixelsNoirs(lignePixels[indexLigne1], erreur) >= getNbPixelsNoirs(lignePixels[indexLigneMilieu], erreur) 
                || getNbPixelsNoirs(lignePixels[indexLigneN], erreur) >= getNbPixelsNoirs(lignePixels[indexLigneMilieu], erreur)
                || Math.Abs(getNbPixelsNoirs(lignePixels[indexLigne1], erreur) - getNbPixelsNoirs(lignePixels[indexLigneN], erreur)) > 5)
            {
                Console.WriteLine("Cette zone ne contient pas de rond");
                return null;
            }
            else
            {
                indexLigneMilieu = (indexLigne1 + indexLigneN) / 2;

                return new PixelsCircle(zone.X + positionPremierPixelNoir(lignePixels[indexLigneMilieu], erreur) + (maxPixelsNoirsLigne / 2),
                    zone.Y + indexLigneMilieu, maxPixelsNoirsLigne);
            }
        }

    }
}
