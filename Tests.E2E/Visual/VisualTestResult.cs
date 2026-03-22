namespace Tests.E2E.Visual
{
    public enum VisualTestStatus
    {
        Passed,
        Failed,
        NewBaseline
    }

    public class VisualTestResult
    {
        public string Name { get; }
        public VisualTestStatus Status { get; }

        /// <summary>Fraction of pixels that differ (0–1). Zero for new baselines.</summary>
        public double DiffFraction { get; }

        public string BaselinePath { get; }
        public string ActualPath { get; }

        /// <summary>Null when the test passed or a new baseline was created.</summary>
        public string? DiffImagePath { get; }

        /// <summary>Human-readable failure reason. Null when the test passed.</summary>
        public string? Message { get; }

        public VisualTestResult(
            string name,
            VisualTestStatus status,
            double diffFraction,
            string baselinePath,
            string actualPath,
            string? diffImagePath,
            string? message = null)
        {
            Name          = name;
            Status        = status;
            DiffFraction  = diffFraction;
            BaselinePath  = baselinePath;
            ActualPath    = actualPath;
            DiffImagePath = diffImagePath;
            Message       = message;
        }
    }
}
