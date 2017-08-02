namespace xofz.Journal98.Framework
{
    using System;

    public class TimeSpanFormatter
    {
        public virtual string Format(TimeSpan ts)
        {
            return ts.Days + "d "
                   + ts.Hours + "h "
                   + ts.Minutes + "m "
                   + ts.Seconds + "s";
        }
    }
}
