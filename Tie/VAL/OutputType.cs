using System;
using System.Collections.Generic;
using System.Text;

namespace Tie
{

    [Flags]
    enum OutputType
    {
        Nothing = 0,
        QuotationMark = 0x01,   //print "
        NullMark = 0x02,        //print null
        Valization = 0x4,       //
        Host = 0x08,            //print 12M, 12.3F instead of (decimal)12 and (float)12.3
        Typeof = 0x10,          //print typeof(..)
        Parentheses = 0x20,     //print {...}
        WellFormatted = 0x40    //well-formatted, indent and new line
    }

}
