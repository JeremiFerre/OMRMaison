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
        private static List<Color> getZonePixels(Bitmap bmp, Rectangle Zone)
        {
            //Traitement
            List<Color> couleurs = new List<Color>();
            int x, y;

            for (y = Zone.Y; y < Zone.Height + Zone.Y; y++)
            {
                for (x = Zone.X; x < Zone.Width + Zone.X; x++)
                {
                    couleurs.Add(bmp.GetPixel(x, y));
                }
            }

            return couleurs;
        }

        /// <summary>
        /// Créer une matrice binaire avec des 1 représentant les pixels noirs, et des 0 pour les pixels blancs.
        /// </summary>
        /// <param name="bmp">Image sur laquelle calculer la matrice</param>
        /// <param name="Zone">Rectangle délimitant la zone à analyser</param>
        /// <param name="erreur">Taux de clarté autorisé [0-255]. Exemple: 40 autorisera les pixels dont les valeurs R,G et B sont inférieures à 40.</param>
        /// <returns>Matrice des pixels noirs détéctés.</returns>
        public static List<List<int>> getMatricePixelsNoirs(Bitmap bmp, Rectangle Zone, int erreur)
        {
            //Traitement
            List<List<int>> matricePixelsNoirs = new List<List<int>>();
            int x, y, curseurListeY = 0, curseurListeX = 0;

            for (y = Zone.Y; y < Zone.Height + Zone.Y; y++)
            {
                matricePixelsNoirs.Add(new List<int>());
                for (x = Zone.X; x < Zone.Width + Zone.X; x++)
                {
                    Color pixel = bmp.GetPixel(x, y);
                    // Si le pixel est considéré comme assez sombre, il est ajouté à la matrice en true
                    // Sinon il est false
                    if (pixel.R < erreur && pixel.G < erreur && pixel.B < erreur)
                    {
                        matricePixelsNoirs[curseurListeY].Add(1);
                    }
                    else
                    {
                        matricePixelsNoirs[curseurListeY].Add(0);
                    }
                    curseurListeX++;
                }
                curseurListeX = 0;
                curseurListeY++;
            }

            return matricePixelsNoirs;
        }

        //Retourne une liste des valeurs RGB de chaque pixel de la zone de l'image demandée
        private static Dictionary<int, List<byte>> getZonePixelsUnsafe(Bitmap bmp, Rectangle Zone, bool DisposeAfter)
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

        /// <summary>
        /// Calcul d'un pourcentage de pixels dans une zone d'image
        /// </summary>
        /// <param name="src">Image dans laquelle on analyse</param>
        /// <param name="zone">Rectangle représentant la zone de l'image à analyser</param>
        /// <param name="erreur">Taux de clarté autorisé [0-255]. Exemple: 40 autorisera les pixels dont les valeurs R,G et B sont inférieures à 40.</param>
        /// <returns>Pourcentage calculé dans cette zone de l'image.</returns>
        public static float getNbPixelsNoirs(Bitmap src, Rectangle zone, int erreur)
        {
            List<Color> listePixels = Detection.getZonePixels(src, zone);
            int nbPixels = 0;

            foreach (Color c in listePixels)
            {
                if (c.R < erreur && c.G < erreur && c.B < erreur)
                {
                    nbPixels++;
                }
            }

            int totalPixels = zone.Height * zone.Width;

            float result = (((float)nbPixels / (float)totalPixels) * 100);

            return result;
        }

        /// <summary>
        /// Calcul d'un pourcentage de pixels dans une zone d'image
        /// </summary>
        /// <param name="src">Image dans laquelle on analyse</param>
        /// <param name="zone">Rectangle représentant la zone de l'image à analyser</param>
        /// <param name="erreur">Taux de clarté autorisé [0-255]. Exemple: 40 autorisera les pixels dont les valeurs R,G et B sont inférieures à 40.</param>
        /// <param name="B">Autoriser la détection de couleur Blue.</param>
        /// <param name="G">Autoriser la détection de couleur Green.</param>
        /// <param name="R">Autoriser la détection de couleur Red.</param>
        /// <returns>Pourcentage calculé dans cette zone de l'image.</returns>
        public static float getNbPixelsNoirs(Bitmap src, Rectangle zone, int erreur, bool R, bool G, bool B)
        {
            List<Color> listePixels = Detection.getZonePixels(src, zone);
            int nbPixels = 0;
            int valeurR = (R) ? 255 : erreur, valeurG = (G) ? 255 : erreur, valeurB = (B) ? 255 : erreur;

            foreach (Color c in listePixels)
            {
                //R XOR G XOR B
                if (((R || G || B) && !((R && B) || (G && B) || (R && G) || (R && B && G))) 
                    && (c.R < valeurR && c.G < valeurG && c.B < valeurB))
                {
                    nbPixels++;
                }

                else if (R && G)
                {
                    if ((c.R < valeurR && c.G < erreur && c.B < valeurB) || (c.R < erreur && c.G < valeurG && c.B < valeurB))
                    {
                        nbPixels++;
                    }
                }
                else if (R && B)
                {
                    if ((c.R < valeurR && c.G < valeurG && c.B < erreur) || (c.R < erreur && c.G < valeurG && c.B < valeurB))
                    {
                        nbPixels++;
                    }
                }
                else if (G && B)
                {
                    if ((c.R < valeurR && c.G < valeurG && c.B < erreur) || (c.R < valeurR && c.G < erreur && c.B < valeurB))
                    {
                        nbPixels++;
                    }
                }
                else if (R && G && B)
                {
                    if ((c.R < valeurR && c.G < valeurG && c.B < erreur) || 
                        (c.R < valeurR && c.G < erreur && c.B < valeurB) ||
                        (c.R < erreur && c.G < valeurG && c.B < valeurB))
                    {
                        nbPixels++;
                    }
                }
                else if (c.R < valeurR && c.G < valeurG && c.B < valeurB)
                {
                    nbPixels++;
                }
            }

            int totalPixels = zone.Height * zone.Width;

            float result = (((float)nbPixels / (float)totalPixels) * 100);

            return result;
        }

        private static int getNbPixelsNoirs(List<Color> listePixels, int erreur)
        {
            int nbPixels = 0;

            foreach (Color c in listePixels)
            {
                if (c.R < erreur && c.G < erreur && c.B < erreur)
                {
                    nbPixels++;
                }
            }
            return nbPixels;
        }

        private static int positionPremierPixelNoir(List<Color> lignePixels, int erreur)
        {
            int posPixels = 0;

            foreach (Color c in lignePixels)
            {
                if (c.R < erreur && c.G < erreur && c.B < erreur)
                {
                    return posPixels;
                }
                posPixels++;
            }

            return -1;
        }

        /// <summary>
        /// Trouver le centre et le diamètre d'un disque noir dans une zone dont on sait que le rond est dominant.
        /// </summary>
        /// <param name="matrice">Matrice de pixels noirs et blancs représentant la zone à analyser</param>
        /// <param name="zone">Rectangle de la zone à analyser (doit correspondre à la matrice)</param>
        /// <param name="erreur">Taux de clarté autorisé [0-255]. Exemple: 40 autorisera les pixels dont les valeurs R,G et B sont inférieures à 40.</param>
        /// <returns>Instance du PixelsCircle trouvé.</returns>
        public static PixelsCircle getCircleFromFreeZone(List<List<int>> matrice, Rectangle zone, int erreur)
        {
            /** ---------------------------- Fonctionnelle ----------------------------------
             * Detection d'un rond par application d'une matrice de poids sur la zone,
             * puis en calculant le centre de gravité de la matrice avec la formule suivante:
             *  CentreRondX = Sigma(i=1, n) i*v_ij / M et CentreRondY = Sigma(j=1, n) j*v_ij / M
             *      avec M = Sigma(i,j) v_ij
             **/
            //Calcul de M
            int M = 0;
            foreach (List<int> lb in matrice)
            {
                foreach (int b in lb)
                {
                    M += b;
                }
            }

            //Calcul du centre du rond
            int xRond = 0, yRond = 0;
            for (int i = 1; i <= matrice.Count; i++)
            {
                for (int j = 1; j <= matrice[i-1].Count; j++)
                {
                    xRond += j * matrice[i-1][j-1];
                    yRond += i * matrice[i-1][j-1];
                }
            }
            xRond /= M;
            yRond /= M;

            //Calcul du diamètre
            int diametre = 0, nombreBlancConsecutifs = 0;
            float margeDeBlanc = matrice[yRond].Count * 0.05f;

            //Parcours depuis le centre du rond trouvé, vers la droite
            for (int x = xRond; x < matrice[yRond].Count; x++)
            {
                if (nombreBlancConsecutifs >= margeDeBlanc)
                {
                    break;
                }
                else if (matrice[yRond][x] == 1)
                {
                    nombreBlancConsecutifs = 0;
                    diametre++;
                }
                else
                {
                    nombreBlancConsecutifs++;
                }
            }

            //Parcours depuis le centre du rond trouvé, vers la gauche
            nombreBlancConsecutifs = 0;
            for (int x = xRond - 1; x >= 0; x--)
            {
                if (nombreBlancConsecutifs >= margeDeBlanc)
                {
                    break;
                }
                else if (matrice[yRond][x] == 1)
                {
                    nombreBlancConsecutifs = 0;
                    diametre++;
                }
                else
                {
                    nombreBlancConsecutifs++;
                }
            }

            return new PixelsCircle(xRond + zone.X, yRond + zone.Y, diametre);
        }
        
        /// <summary>
        /// Méthode non-optimale pour détecter un rond
        /// </summary>
        public static PixelsCircle getPixelsNoirsRond(Bitmap src, Rectangle zone, int erreur)
        {
            List<Color> listePixels = Detection.getZonePixels(src, zone);

            float margeErreurPixels = (1 * (float) src.Width) / 100;

            int maxPixelsNoirsLigne = 0;
            int indexLigne1 = 0, indexLigneN = 0, indexLigneMilieu = 0;

            //Transfert de chaque ligne de pixels dans des listes
            List<List<Color>> lignePixels = new List<List<Color>>();
            int compteurListe = 0, c = 0;
            lignePixels.Add(new List<Color>());

            foreach (Color col in listePixels)
            {
                lignePixels[compteurListe].Add(col);
                c++;
                if (c == zone.Width)
                {
                    c = 0;
                    compteurListe++;
                    lignePixels.Add(new List<Color>());
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

            if (indexLigneMilieu == 0)
            {
                Console.WriteLine("Cette zone ne contient pas de rond1");
                return null;
            }

            //Verification existance cercle vers le haut du diamètre
            for (int y = indexLigneMilieu; y >= 0; y--)
            {
                if ((y - 1) == 0)
                {
                    Console.WriteLine("Cette zone ne contient pas de rond2");
                    return null;
                }
                //Console.Write(positionPremierPixelNoir(lignePixels[y - 1], erreur) + ";" + positionPremierPixelNoir(lignePixels[y], erreur) + " ");
                if ((positionPremierPixelNoir(lignePixels[y - 1], erreur) - positionPremierPixelNoir(lignePixels[y], erreur) > margeErreurPixels))
                {
                    Console.WriteLine("Cette zone ne contient pas de rond3");
                    return null;
                }
                if (getNbPixelsNoirs(lignePixels[y - 1], erreur) - getNbPixelsNoirs(lignePixels[y], erreur) > margeErreurPixels)
                {
                    Console.WriteLine("Cette zone ne contient pas de rond3bis");
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
                if ((y + 1) == lignePixels.Count)
                {
                    Console.WriteLine("Cette zone ne contient pas de rond4");
                    return null;
                }

                if ((positionPremierPixelNoir(lignePixels[y + 1], erreur) - positionPremierPixelNoir(lignePixels[y], erreur) > margeErreurPixels))
                {
                    Console.WriteLine("Cette zone ne contient pas de rond5");
                    return null;
                }
                if (getNbPixelsNoirs(lignePixels[y + 1], erreur) - getNbPixelsNoirs(lignePixels[y], erreur) > margeErreurPixels)
                {
                    Console.WriteLine("Cette zone ne contient pas de rond5bis");
                    return null;
                }
                if (getNbPixelsNoirs(lignePixels[y], erreur) == 0)
                {
                    indexLigneN = y - 1;
                    break;
                }
            }

            //Vérification Cercle finale
            bool b1 = getNbPixelsNoirs(lignePixels[indexLigne1], erreur) >= getNbPixelsNoirs(lignePixels[indexLigneMilieu], erreur);
            bool b2 = getNbPixelsNoirs(lignePixels[indexLigneN], erreur) >= getNbPixelsNoirs(lignePixels[indexLigneMilieu], erreur);
            bool b3 = Math.Abs(getNbPixelsNoirs(lignePixels[indexLigne1], erreur) - getNbPixelsNoirs(lignePixels[indexLigneN], erreur)) > margeErreurPixels * 2;

            if (b1||b2||b3)
            {
                Console.WriteLine("Cette zone ne contient pas de rond6");
                return null;
            }
            else
            {
                indexLigneMilieu = (indexLigne1 + indexLigneN) / 2;

                return new PixelsCircle(zone.X + positionPremierPixelNoir(lignePixels[indexLigneMilieu], erreur) + (maxPixelsNoirsLigne / 2),
                    zone.Y + indexLigneMilieu + 1, maxPixelsNoirsLigne);
            }
        }

    }
}
