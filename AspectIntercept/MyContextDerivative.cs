using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AspectIntercept
{
    class MyContextDerivative : MyContextObject
    {
        public override string DoSomething(string s, int i, ref string refv, out int outv)
        {
           
            return base.DoSomething(s, i, ref refv, out outv);
        }
    }
}
