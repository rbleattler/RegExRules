using Xunit;
using RegexRules;

namespace RegexRulesTests;
public class PatternValueTests
{
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
}
