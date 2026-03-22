using ImageMagick;

namespace Tests.E2E.Visual
{
    /// <summary>
    /// Compares a Playwright screenshot against a committed baseline image using Magick.NET.
    ///
    /// If no baseline exists the test fails with instructions on how to create one.
    ///
    /// Set the environment variable <c>UPDATE_VISUAL_BASELINES=true</c> to create or overwrite
    /// baselines with the current screenshots (useful after intentional UI changes).
    /// Copy updated files from the output <c>Baselines/</c> directory back into the project to commit them.
    /// </summary>
    public class VisualComparer
    {
        private readonly string _baselineDir;
        private readonly string _resultsDir;

        /// <summary>
        /// Maximum fraction of differing pixels before a test is considered failed.
        /// Defaults to 0.001 (0.1%).
        /// </summary>
        public double Threshold { get; set; } = 0.001;

        private static readonly bool UpdateBaselines =
            string.Equals(Environment.GetEnvironmentVariable("UPDATE_VISUAL_BASELINES"), "true",
                StringComparison.OrdinalIgnoreCase);

        public VisualComparer(string baselineDir, string resultsDir)
        {
            _baselineDir = baselineDir;
            _resultsDir  = resultsDir;

            Directory.CreateDirectory(_baselineDir);
            Directory.CreateDirectory(_resultsDir);
        }

        /// <summary>
        /// Compares <paramref name="screenshot"/> against the stored baseline for <paramref name="name"/>.
        /// </summary>
        public VisualTestResult Compare(string name, byte[] screenshot)
        {
            var baselinePath = Path.Combine(_baselineDir, $"{name}.png");
            var actualPath   = Path.Combine(_resultsDir,  $"{name}_actual.png");
            var diffPath     = Path.Combine(_resultsDir,  $"{name}_diff.png");

            File.WriteAllBytes(actualPath, screenshot);

            if (!File.Exists(baselinePath))
            {
                if (!UpdateBaselines)
                {
                    return new VisualTestResult(
                        name, VisualTestStatus.Failed, 0, baselinePath, actualPath, null,
                        $"No baseline found for '{name}'. " +
                        $"To create one, run: UPDATE_VISUAL_BASELINES=true dotnet test Tests.E2E --filter \"FullyQualifiedName~VisualTests\" " +
                        $"then copy Tests.E2E/bin/.../Baselines/{name}.png into Tests.E2E/Tests/Visual/Baselines/ and commit it.");
                }

                File.Copy(actualPath, baselinePath, overwrite: true);
                return new VisualTestResult(
                    name, VisualTestStatus.NewBaseline, 0, baselinePath, actualPath, null);
            }

            if (UpdateBaselines)
            {
                File.Copy(actualPath, baselinePath, overwrite: true);
                return new VisualTestResult(
                    name, VisualTestStatus.NewBaseline, 0, baselinePath, actualPath, null);
            }

            using var baseline = new MagickImage(baselinePath);
            using var actual   = new MagickImage(screenshot);

            // Normalise dimensions so a viewport change doesn't mask real failures
            if (baseline.Width != actual.Width || baseline.Height != actual.Height)
                actual.Resize(new MagickGeometry(baseline.Width, baseline.Height)
                    { IgnoreAspectRatio = true });

            // Count differing pixels
            var differentPixels = baseline.Compare(actual, ErrorMetric.Absolute);
            var diffFraction    = differentPixels / (double)(baseline.Width * baseline.Height);

            if (diffFraction > Threshold)
            {
                WriteDiffImage(baseline, actual, diffPath);
                return new VisualTestResult(
                    name, VisualTestStatus.Failed, diffFraction, baselinePath, actualPath, diffPath);
            }

            return new VisualTestResult(
                name, VisualTestStatus.Passed, diffFraction, baselinePath, actualPath, null);
        }

        /// <summary>
        /// Generates a diff image highlighting changed pixels.
        /// Uses the Difference composite operator then normalises contrast for visibility.
        /// </summary>
        private static void WriteDiffImage(MagickImage baseline, MagickImage actual, string diffPath)
        {
            try
            {
                using var diff = baseline.Clone();
                diff.Composite(actual, CompositeOperator.Difference);
                diff.Normalize(); // Stretch contrast so subtle changes are visible
                diff.Write(diffPath);
            }
            catch
            {
                // Diff image generation is best-effort; never fail the test over it
            }
        }
    }
}
