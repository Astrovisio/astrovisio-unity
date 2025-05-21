using MessagePack;


namespace Astrovisio
{
    [MessagePackObject]
    public class DataPack
    {
        [Key("columns")]
        public string[] Columns { get; set; }

        [Key("rows")]
        public double[][] Rows { get; set; }
    }

}
