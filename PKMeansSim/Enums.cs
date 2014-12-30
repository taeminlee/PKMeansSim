using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PKMeansSim
{
    public enum MR
    {
        MasterNode,
        DataNode
    }

    public enum MRState
    {
        MasterNodeInit,
        StartMRJob,
        CentroidSetGenerate,
        DataNodeInit,
        MapStart,
        MapEnd,
        CombineStart,
        CombineEnd,
        ShufflingStart,
        ShufflingEnd,
        ReduceStart,
        ReduceEnd,
        Finish,
        ReduceCombinePoints
    }

    public enum MapCode
    {
        L1Construct,
        L2InitMinDis,
        L3InitIndex,
        L4ForStart,
        L5ComputeDist,
        L6DistCheck,
        L7SetMinDis,
        L8SetIndex,
        L9ForEnd,
        L10IndexAsKey2,
        L11SetValue2,
        L12OutputKV2
    }

    public enum DataSetGenerateMethods
    {
        Uniform,
        KBlock
    }

    public enum CentroidGenerateMethods
    {
        Random,
        KMeansPlusPlus,
        KMeansBarBar
    }
}
