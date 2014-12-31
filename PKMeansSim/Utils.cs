using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace PKMeansSim
{
    public static class Utils
    {
        public static Color GetPointColor(int id)
        {
            switch(id % 9)
            {
                case 0:
                    return Color.FromArgb(51, 102, 255);
                case 1 :
                    return Color.FromArgb(255, 51, 102);
                case 2 :
                    return Color.FromArgb(31, 255, 31);
                case 3:
                    return Color.FromArgb(245, 184, 0);
                case 4:
                    return Color.FromArgb(51, 204, 255);
                case 5:
                    return Color.FromArgb(204, 51, 255);
                case 6:
                    return Color.FromArgb(204, 255, 51);
                case 7:
                    return Color.FromArgb(184, 138, 0);
                case 8:
                    return Color.FromArgb(0, 46, 184);
            }
            return Color.Black;
        }

        public static Color GetCentroidColor(int id)
        {
            switch (id % 9)
            {
                case 0:
                    return Color.FromArgb(121, 162, 245);
                case 1:
                    return Color.FromArgb(255, 161, 162);
                case 2:
                    return Color.FromArgb(161, 255, 202);
                case 3:
                    return Color.FromArgb(255, 224, 80);
                case 4:
                    return Color.FromArgb(151, 244, 255);
                case 5:
                    return Color.FromArgb(244, 151, 255);
                case 6:
                    return Color.FromArgb(214, 255, 151);
                case 7:
                    return Color.FromArgb(204, 198, 90);
                case 8:
                    return Color.FromArgb(80, 126, 204);
            }
            return Color.Black;
        }
    }
}
