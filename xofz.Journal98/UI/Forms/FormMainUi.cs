namespace xofz.Journal98.UI.Forms
{
    using System;
    using System.Threading;
    using System.Windows.Forms;
    using xofz.UI;
    using xofz.UI.Forms;

    public partial class FormMainUi : FormUi, MainUi, HomeUi
    {
        public FormMainUi(
            Materializer materializer)
        {
            this.materializer = materializer;
            this.InitializeComponent();
        }

        public virtual StatisticsUi StatisticsUi => this.statisticsUi;

        public event Action ShutdownRequested;

        public event Action NewKeyTapped;

        public event Action SubmitKeyTapped;

        public event Action<int> EntrySelected;

        string HomeUi.TotalTime
        {
            get => this.totalTimeLabel.Text;

            set => this.totalTimeLabel.Text = value;
        }

        JournalEntry HomeUi.CurrentEntry
        {
            get => new JournalEntry
            {
                Content = this.materializer.Materialize(this.contentTextBox.Lines)
            };

            set
            {
                this.createdTextBox.Text = value.CreatedTimestamp?
                    .ToString("yyyy/MM/dd hh:mm:ss tt");
                this.modifiedTextBox.Text = value.ModifiedTimestamp?
                    .ToString("yyyy/MM/dd hh:mm:ss tt");
                this.contentTextBox.Lines = MEHelpers.ToArray(value.Content);
            }
        }

        bool HomeUi.ContentEditable
        {
            get => !this.contentTextBox.ReadOnly;

            set
            {
                this.contentTextBox.ReadOnly = !value;
                this.submitKey.Enabled = value;
            }
        }

        MaterializedEnumerable<JournalEntry> HomeUi.Entries
        {
            get => MaterializedEnumerable.Empty<JournalEntry>();

            set
            {
                this.entriesGrid.Rows.Clear();
                foreach (var entry in value)
                {
                    var summary = MEHelpers.FirstOrDefault(entry.Content);
                    summary = summary?.Substring(
                        0,
                        summary.Length > 50
                            ? 50
                            : summary.Length);

                    this.entriesGrid.Rows.Add(
                        entry.CreatedTimestamp?.ToString("yyyy/MM/dd hh:mm:ss tt"),
                        entry.ModifiedTimestamp?.ToString("yyyy/MM/dd hh:mm:ss tt"),
                        summary + "...");
                }
            }
        }

        private void newKey_Click(object sender, EventArgs e)
        {
            new Thread(() => this.NewKeyTapped?.Invoke()).Start();
        }

        private void submitKey_Click(object sender, EventArgs e)
        {
            new Thread(() => this.SubmitKeyTapped?.Invoke()).Start();
        }

        private void searchResultsViewer_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex > -1 && e.RowIndex < this.entriesGrid.RowCount - 1)
            {
                new Thread(() => this.EntrySelected?.Invoke(e.RowIndex)).Start();
            }
        }

        private void this_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            new Thread(() => this.ShutdownRequested?.Invoke()).Start();
        }

        private readonly Materializer materializer;
    }
}
