using ApprovalTests;
using ApprovalTests.Reporters;

namespace MegaPricer.CharacterizationTests;

[UseReporter(typeof(DiffReporter))]
public class CsvFileComparisonTests
{
  [Fact]
  public void TestCsvFileIsAsExpected()
  {
    // Generate the new CSV file (includes a timestamp)
    string newPath = GenerateCsvFile();

    // Read the file into memory and remove the first line (the timestamp)
    string[] allLines = File.ReadAllLines(newPath);
    string[] allLinesExceptFirst = new string[allLines.Length - 1];
    Array.Copy(allLines, 1, allLinesExceptFirst, 0, allLines.Length - 1);
    string modifiedContent = string.Join(Environment.NewLine, allLinesExceptFirst);

    // Verify the modified content
    Approvals.Verify(modifiedContent);
  }

  public string GenerateCsvFile()
  {
    // Your logic to generate the CSV file
    string path = "Orders.csv";

    // MANUALLY COPY Orders.csv to bin/debug/version/Orders.csv

    return path;
  }
}
