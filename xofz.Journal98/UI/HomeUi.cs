namespace xofz.Journal98.UI
{
    using System;
    using xofz.UI;

    public interface HomeUi : Ui
    {
        event Action NewKeyTapped;

        event Action SubmitKeyTapped;

        event Action<int> EntrySelected;

        MaterializedEnumerable<JournalEntry> Entries { get; set; }

        JournalEntry CurrentEntry { get; set; }

        string TotalTime { get; set; }

        bool ContentEditable { get; set; }
    }
}
