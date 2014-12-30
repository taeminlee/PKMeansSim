using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PKMeansSim
{
    public class MapCodeEventArgs : EventArgs
    {
        private MapCode mapCode;
        private Centroid centroid;

        public MapCode MapCode
        {
            get { return mapCode; }
        }

        public Centroid Centroid
        {
            get { return centroid; }
        }

        public MapCodeEventArgs(MapCode mapCode, Centroid centroid)
        {
            this.mapCode = mapCode;
            this.centroid = centroid;
        }
    }
}
