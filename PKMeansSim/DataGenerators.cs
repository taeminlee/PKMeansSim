using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PKMeansSim
{
    public class UniformGenerator : DataGenerator
    {
        public override List<Point> GeneratePoints(int pointNum)
        {
            List<Point> points = new List<Point>();

            Random rand = new Random();

            for (int i = 0; i < pointNum; i++)
            {
                double x = rand.NextDouble() * (Config.maxX - Config.minX) + Config.minX;
                double y = rand.NextDouble() * (Config.maxY - Config.minY) + Config.minY;

                points.Add(new Point(x, y));
            }

            return points;
        }
    }

    public class KBlockGenerator : DataGenerator
    {
        public override List<Point> GeneratePoints(int pointNum)
        {
            List<Point> points = new List<Point>();
            List<Point> grid = new List<Point>();

            Random rand = new Random();

            int clusterPointNum = pointNum/Config.k;

            for (int x = 1; x < Config.k + 2; x++)
            {
                for (int y = 1; y < Config.k + 2; y++)
                {
                    grid.Add(new Point(x * (Config.maxX - Config.minX) / (Config.k + 2) + Config.minX, y * (Config.maxY - Config.minY) / (Config.k + 2) + Config.minY));
                }
            }

            for (int k = 0; k < Config.k; k++)
            {
                Point center = grid[rand.Next(grid.Count)];
                grid.Remove(center);
                for (int n = 0; n < clusterPointNum; n++)
                {
                    double x = (center.X - (Config.maxX - Config.minX)/(Config.k + 2)/2) +
                               rand.NextDouble()*(Config.maxX - Config.minX)/(Config.k + 2);
                    double y = (center.Y - (Config.maxY - Config.minY) / (Config.k + 2) / 2) +
                               rand.NextDouble() * (Config.maxY - Config.minY) / (Config.k + 2);
                    points.Add(new Point(x, y));
                }
                
            }

            return points;
        }
    }
}
