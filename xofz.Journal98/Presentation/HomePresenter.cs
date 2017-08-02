namespace xofz.Journal98.Presentation
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using xofz.Framework;
    using xofz.Framework.Materialization;
    using xofz.Journal98.Framework;
    using xofz.Journal98.UI;
    using xofz.Presentation;
    using xofz.UI;

    public sealed class HomePresenter : Presenter
    {
        public HomePresenter(
            HomeUi ui,
            MethodWeb web) 
            : base(ui, null)
        {
            this.ui = ui;
            this.web = web;
        }

        public void Setup()
        {
            if (Interlocked.CompareExchange(ref this.setupIf1, 1, 0) == 1)
            {
                return;
            }

            this.ui.NewKeyTapped += this.ui_NewKeyTapped;
            this.ui.SubmitKeyTapped += this.ui_SubmitKeyTapped;
            this.ui.EntrySelected += this.ui_EntrySelected;
            var w = this.web;
            w.Run<Navigator>(n => n.RegisterPresenter(this));
            w.Run<xofz.Framework.Timer>(
                t => t.Elapsed += this.timer_Elapsed,
                "HomeTimer");
        }

        public override void Start()
        {
            var w = this.web;
            var entries = w.Run<
                JournalEntryLoader,
                IEnumerable<JournalEntry>>(
                loader => loader.Load());
            this.setAllEntries(
                EnumerableHelpers.OrderByDescending(entries,
                e => e.ModifiedTimestamp));
            w.Run<EventRaiser>(er => er.Raise(this.ui, "EntrySelected", 0));
            w.Run<xofz.Framework.Timer>(t => t.Start(1000), "HomeTimer");
        }

        public override void Stop()
        {
            var w = this.web;
            w.Run<xofz.Framework.Timer>(t => t.Stop(), "HomeTimer");
        }

        private void setAllEntries(OrderedMaterializedEnumerable<JournalEntry> allEntries)
        {
            this.allEntries = allEntries;
            this.web.Run<JournalEntriesHolder>(holder => holder.Entries
                = new OrderedMaterializedEnumerable<JournalEntry>(allEntries));
            UiHelpers.Write(this.ui, () => this.ui.Entries = allEntries);
        }

        private void ui_NewKeyTapped()
        {
            var w = this.web;
            var editable = UiHelpers.Read(this.ui, () => this.ui.ContentEditable);
            var current = UiHelpers.Read(this.ui, () => this.ui.CurrentEntry);
            if (editable && current.Content.Count > 0)
            {
                var discardChanges = w.Run<Messenger, Response>(m => UiHelpers.Read(
                    m.Subscriber,
                    () => m.Question("Discard current changes?")));
                if (discardChanges == Response.No)
                {
                    return;
                }
            }

            this.setCurrentEntry(
                new JournalEntry { CreatedTimestamp = DateTime.Now });
            UiHelpers.Write(this.ui, () =>
            {
                this.ui.CurrentEntry = this.currentEntry;
                this.ui.ContentEditable = true;
            });
        }

        private void ui_SubmitKeyTapped()
        {
            var ce = this.currentEntry;
            ce.ModifiedTimestamp = DateTime.Now;
            ce.Content = UiHelpers.Read(this.ui, () => this.ui.CurrentEntry.Content);

            var w = this.web;
            w.Run<JournalEntrySaver>(saver => saver.Save(ce));

            UiHelpers.Write(this.ui, () => this.ui.CurrentEntry = ce);
            this.Start();
        }

        private void ui_EntrySelected(int entryIndex)
        {
            if (this.allEntries.Count - 1 < entryIndex)
            {
                return;
            }

            this.setCurrentEntry(this.allEntries[entryIndex]);
        }

        private void timer_Elapsed()
        {
            if (this.currentEntry?.CreatedTimestamp == null)
            {
                return;
            }

            if (this.currentEntry?.ModifiedTimestamp == null)
            {
                return;
            }

            var editable = this.currentEntry.CreatedTimestamp.Value.Date 
                == DateTime.Today;
            UiHelpers.Write(
                this.ui,
                () => this.ui.ContentEditable = editable);
        }

        private void setCurrentEntry(JournalEntry currentEntry)
        {
            if (currentEntry.CreatedTimestamp == null)
            {
                return;
            }

            this.currentEntry = currentEntry;
            var w = this.web;
            var editable = currentEntry.CreatedTimestamp.Value.Date == DateTime.Today;
            var totalTime = TimeSpan.Zero;
            if (currentEntry.ModifiedTimestamp != null)
            {
                totalTime = currentEntry.ModifiedTimestamp.Value 
                    - currentEntry.CreatedTimestamp.Value;
            }

            var formattedString = w.Run<TimeSpanFormatter, string>(
                f => f.Format(totalTime));
            UiHelpers.Write(this.ui, () => this.ui.TotalTime =
                formattedString);

            UiHelpers.Write(this.ui, () =>
            {
                this.ui.CurrentEntry = currentEntry;
                this.ui.ContentEditable = editable;
            });
        }

        private int setupIf1;
        private OrderedMaterializedEnumerable<JournalEntry> allEntries;
        private JournalEntry currentEntry;
        private readonly HomeUi ui;
        private readonly MethodWeb web;
    }
}
