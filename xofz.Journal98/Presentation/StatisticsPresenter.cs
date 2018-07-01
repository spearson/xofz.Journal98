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
            MaterializedEnumerable<JournalEntry> entries = null;
            w.Run<JournalEntriesHolder>(holder =>
            {
                entries = holder.Entries;
            });

            if (entries == null)
            {
                return;
            }

            var totalCount = entries.Count;
            var tcString = totalCount.ToString();
            UiHelpers.Write(
                this.ui, 
                () => this.ui.TotalCount = tcString);

            var now = DateTime.Now;
            var countThisMonth = EnumerableHelpers.Count(
                    entries,
                    e => e.ModifiedTimestamp >=
                         new DateTime(now.Year, now.Month, 1))
                .ToString();
            UiHelpers.Write(
                this.ui, 
                () => this.ui.CountThisMonth = countThisMonth);

            var countThisYear = EnumerableHelpers.Count(
                    entries,
                    e => e.ModifiedTimestamp >=
                         new DateTime(now.Year, 1, 1))
                .ToString();
            UiHelpers.Write(
                this.ui, 
                () => this.ui.CountThisYear = countThisYear);

            if (totalCount == 0)
            {
                w.Run<TimeSpanFormatter>(formatter =>
                {
                    var formattedZero = formatter.Format(TimeSpan.Zero);
                    UiHelpers.Write(this.ui,
                        () =>
                        {
                            this.ui.AvgTime = formattedZero;
                            this.ui.AvgTimeThisMonth = formattedZero;
                            this.ui.AvgTimeThisYear = formattedZero;
                        });
                });
                return;
            }

            var totalTime = this.getTotalTime(entries);
            var avgTime = new TimeSpan(totalTime.Ticks / totalCount);
            w.Run<TimeSpanFormatter>(formatter =>
            {
                var formattedAverage = formatter.Format(avgTime);
                UiHelpers.Write(
                    this.ui,
                    () => this.ui.AvgTime = formattedAverage);
            });
            

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
                w.Run<TimeSpanFormatter>(formatter =>
                {
                    var formattedAverageThisMonth =
                        formatter.Format(avgTimeThisMonth);
                    UiHelpers.Write(
                        this.ui,
                        () => this.ui.AvgTimeThisMonth =
                            formattedAverageThisMonth);
                });
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
                w.Run<TimeSpanFormatter>(formatter =>
                {
                    var formattedAverageThisYear =
                        formatter.Format(avgTimeThisYear);
                    UiHelpers.Write(
                        this.ui,
                        () => this.ui.AvgTimeThisYear =
                            formattedAverageThisYear);
                });
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
