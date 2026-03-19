namespace Tests.E2E.Visual
{
    /// <summary>
    /// xUnit class fixture shared across all tests in <c>VisualTests</c>.
    /// Owns the <see cref="VisualComparer"/>, collects results, and generates the HTML
    /// report when the last test in the class completes.
    ///
    /// Baseline images are read from (and new baselines written to) the <c>Baselines/</c>
    /// directory next to the test assembly output.  Commit that directory to keep baselines
    /// in source control — the MSBuild <c>CopyToOutputDirectory</c> entry in the csproj
    /// keeps the output copy in sync with the project.
    ///
    /// Diff images and the HTML report are written to <c>VisualResults/</c> in the output
    /// directory (gitignored).
    /// </summary>
    public class VisualTestContext : IAsyncDisposable
    {
        private readonly List<VisualTestResult> _results = new();
        private readonly string _reportPath;

        public VisualComparer Comparer { get; }

        public VisualTestContext()
        {
            var baseDir     = AppContext.BaseDirectory;
            var baselineDir = Path.Combine(baseDir, "Baselines");
            var resultsDir  = Path.Combine(baseDir, "VisualResults");

            _reportPath = Path.Combine(resultsDir, "report.html");
            Comparer    = new VisualComparer(baselineDir, resultsDir);
        }

        public void AddResult(VisualTestResult result)
        {
            lock (_results) _results.Add(result);
        }

        public async ValueTask DisposeAsync()
        {
            if (_results.Count == 0) return;

            VisualReport.Generate(_results, _reportPath);
            await Task.CompletedTask;
        }
    }
}
