namespace xofz.Journal98
{
    using System;

    public class JournalEntry
    {
        public virtual DateTime? CreatedTimestamp { get; set; }

        public virtual DateTime? ModifiedTimestamp { get; set; }

        public virtual MaterializedEnumerable<string> Content { get; set; }
    }
}
