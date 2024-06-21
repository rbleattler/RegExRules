using Xunit;
using RegexRules;

namespace RegexRulesTests;
public class PatternValueTests : RegexRuleTestCore
{

  public string[] AllTestFiles => GetAllTestFiles(directory: ExampleFilesDirectory, searchPattern: "patternValue*.yml") ?? Array.Empty<string>();

  [Fact]
  public void AllPatternValue_ConstructValidObjects_FromValidYaml()
  {
    for (var i = 0; i < AllTestFiles.Length; i++)
    {
      var PatternValue = new PatternValue(ReadFileAsString(AllTestFiles[i]));
      // Console.WriteLine($"Test file: {AllTestFiles[i]}");
      Assert.NotNull(PatternValue);
    }
  }

  [Fact]
  public void DefaultConstructor_SetsValueToEmptyString()
  {
    var patternValue = new PatternValue();
    Assert.Equal(string.Empty, patternValue.Value);
  }

  [Fact]
  public void DynamicConstructor_SetsValueToPassedValue()
  {
    var value = "test";
    var patternValue = new PatternValue(value);
    Assert.Equal(value, patternValue.Value);
  }

  [Fact]
  public void StringConstructor_SetsValueToPassedValue()
  {
    var value = "test";
    var patternValue = new PatternValue(value);
    Assert.Equal(value, patternValue.Value);
  }
  [Fact]
  public void StringConstructor_SetsValueToPassedValue_FromYaml()
  {
    var value = "Value: test";
    var patternValue = new PatternValue(value);
    Assert.Equal("test", patternValue.Value);
  }

  [Fact]
  public void ToString_ReturnsStringValueOfValue()
  {
    var value = "test";
    var patternValue = new PatternValue(value);
    Assert.Equal(value, patternValue.ToString());
  }

  [Fact]
  public void ToRegex_WhenValueIsPatternValue_ReturnsExpectedResult()
  {
    // Arrange
    var innerPatternValue = new PatternValue("inner value");
    var patternValue = new PatternValue(innerPatternValue);

    // Act
    var result = patternValue.ToRegex();

    // Assert
    Assert.Equal("inner value", result);
  }

  [Fact]
  public void ToRegex_WhenValueIsString_ReturnsExpectedResult()
  {
    // Arrange
    var patternValue = new PatternValue("test value");

    // Act
    var result = patternValue.ToRegex();

    // Assert
    Assert.Equal("test value", result);
  }

  [Fact]
  public void ToRegex_WhenValueIsNull_ReturnsExpectedResult()
  {
    // Arrange
    var patternValue = new PatternValue(string.Empty);

    // Act
    var result = patternValue.ToRegex();

    // Assert
    Assert.Equal(string.Empty, result);
  }
}
