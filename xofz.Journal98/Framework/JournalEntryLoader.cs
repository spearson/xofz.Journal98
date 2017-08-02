namespace xofz.Journal98.Framework
{
    using System.Collections.Generic;

    public interface JournalEntryLoader
    {
        IEnumerable<JournalEntry> Load();
    }
}
