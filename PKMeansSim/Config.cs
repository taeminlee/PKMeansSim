using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PKMeansSim
{
    public static class Config
    {
        public static int k = 5; // k-means cluster size
        public static int b = 4; // block, Mapper size
        public static int n = 300; // data point size

        public static double minX = 0;
        public static double maxX = 100;
        public static double minY = 0;
        public static double maxY = 100;

        public static DataSetGenerateMethods DataGenerateMethods = DataSetGenerateMethods.KBlock;
        public static CentroidGenerateMethods CentroidGenerateMethod = CentroidGenerateMethods.Random;
    }
}
