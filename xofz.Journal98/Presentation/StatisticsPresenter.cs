namespace xofz.Journal98.Presentation
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using xofz.Framework;
    using xofz.Journal98.Framework;
    using xofz.Journal98.UI;
    using xofz.Presentation;
    using xofz.UI;

    public sealed class StatisticsPresenter : Presenter
    {
        public StatisticsPresenter(
            StatisticsUi ui,
            MethodWeb web) :
            base(ui, null)
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

            var w = this.web;
            w.Run<xofz.Framework.Timer>(
                t => t.Elapsed += this.timer_Elapsed,
                "StatisticsTimer");
            w.Run<Navigator>(n => n.RegisterPresenter(this));
        }

        public override void Start()
        {
            this.web.Run<xofz.Framework.Timer>(
                t => t.Start(1000), 
                "StatisticsTimer");
        }

        public override void Stop()
        {
            this.web.Run<xofz.Framework.Timer>(
                t => t.Stop(),
                "StatisticsTimer");
        }

        private void timer_Elapsed()
        {
            var w = this.web;
            var entries = w.Run<JournalEntriesHolder,
                MaterializedEnumerable<JournalEntry>>(holder => holder.Entries);

            var totalCount = entries.Count;
            UiHelpers.Write(this.ui, () => this.ui.TotalCount = totalCount.ToString());

            var now = DateTime.Now;

            var countThisMonth = MEHelpers.Count(
                entries,
                e => e.ModifiedTimestamp >=
                     new DateTime(now.Year, now.Month, 1));
            UiHelpers.Write(this.ui, () => this.ui.CountThisMonth = countThisMonth.ToString());

            var countThisYear = MEHelpers.Count(
                entries,
                e => e.ModifiedTimestamp >=
                     new DateTime(now.Year, 1, 1));
            UiHelpers.Write(this.ui, () => this.ui.CountThisYear = countThisYear.ToString());

            if (totalCount == 0)
            {
                var formattedZero = w.Run<TimeSpanFormatter, string>(
                    f => f.Format(TimeSpan.Zero));
                UiHelpers.Write(this.ui, () =>
                {
                    this.ui.AvgTime = formattedZero;
                    this.ui.AvgTimeThisMonth = formattedZero;
                    this.ui.AvgTimeThisYear = formattedZero;
                });
                return;
            }

            var totalTime = this.getTotalTime(entries);
            var avgTime = new TimeSpan(totalTime.Ticks / totalCount);
            var formattedAvg = w.Run<TimeSpanFormatter, string>(
                f => f.Format(avgTime));
            UiHelpers.Write(this.ui, () => this.ui.AvgTime
                = formattedAvg);

            var entriesThisMonth = new LinkedList<JournalEntry>(
                EnumerableHelpers.Where(
                    entries,
                    e => e.ModifiedTimestamp >= new DateTime(
                             now.Year, now.Month, 1)));
            if (entriesThisMonth.Count > 0)
            {
                var timeThisMonth = this.getTotalTime(entriesThisMonth);
                var avgTimeThisMonth = new TimeSpan(
                    timeThisMonth.Ticks / entriesThisMonth.Count);
                var formattedAvgMonth = w.Run<TimeSpanFormatter, string>(
                    f => f.Format(avgTimeThisMonth));
                UiHelpers.Write(this.ui, () => this.ui.AvgTimeThisMonth
                    = formattedAvgMonth);
            }

            var entriesThisYear = new LinkedList<JournalEntry>(
                EnumerableHelpers.Where(
                    entries,
                    e => e.ModifiedTimestamp >= new DateTime(
                             DateTime.Now.Year,
                             1,
                             1)));
            if (entriesThisYear.Count > 0)
            {
                var timeThisYear = this.getTotalTime(entriesThisYear);
                var avgTimeThisYear = new TimeSpan(
                    timeThisYear.Ticks / entriesThisYear.Count);
                var formattedAvgYear = w.Run<TimeSpanFormatter, string>(
                    f => f.Format(avgTimeThisYear));
                UiHelpers.Write(this.ui, () => this.ui.AvgTimeThisYear
                    = formattedAvgYear);
            }
        }

        private TimeSpan getTotalTime(IEnumerable<JournalEntry> entries)
        {
            return EnumerableHelpers.Aggregate(
                entries,
                TimeSpan.Zero,
                (current, e) =>
                {
                    if (e.ModifiedTimestamp == null || e.CreatedTimestamp == null)
                    {
                        return current.Add(TimeSpan.Zero);
                    }

                    return current.Add(
                        e.ModifiedTimestamp.Value
                        - e.CreatedTimestamp.Value);
                });
        }

        private int setupIf1;
        private readonly StatisticsUi ui;
        private readonly MethodWeb web;
    }
}
