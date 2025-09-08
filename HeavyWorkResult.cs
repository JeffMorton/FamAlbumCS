using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class HeavyWorkResult
{
    //public int Skipped { get; set; }
    public int Processed { get; set; }

    public override string ToString()
    {
        return $"  {Processed}";
    }
}
