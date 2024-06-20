using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using YamlDotNet.Serialization;
using FluentRegex;
using System.Text.Json;
using NJsonSchema;
using System.Text.RegularExpressions;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("RegexRulesTests")]
namespace RegexRules;

public class CharacterClassPattern : ICharacterClass
{
    private string _type = "CharacterClass";
    private PatternValue _value;

    string? IPattern.Id { get => Id; set => Id = value; }
    IPatternValue IPattern.Value
    {
        get => Value;
        set => Value = (PatternValue)value;
    }

    IQuantifier? IPattern.Quantifiers
    {
        get => Quantifiers;
        set => Quantifiers = (Quantifier?)value;
    }

    string IPattern.Type
    {
        get => Type;
        set => Type = value;
    }

    [JsonPropertyName("Id")]
    [YamlMember(Alias = "Id")]
    public string? Id { get; set; } = Guid.NewGuid().ToString();

    [JsonPropertyName("Type")]
    [YamlMember(Alias = "Type")]
    public string Type
    {
        get => _type;
        set => _ = value;
    }

    [JsonPropertyName("Value")]
    [YamlMember(Alias = "Value")]
    public PatternValue Value
    {
        get
        {
            return _value;
        }
        set
        {
            var isValid = IsValidCharacterClassType(value) || IsValidCharacterClass(value);
            if (isValid == false)
            {
                throw new ArgumentException("Invalid CharacterClass.\n"
                                            + "Valid types are: "
                                            + string.Join(", ", GetValidCharacterClassTypes())
                                            + "\n"
                                            + "Valid CharacterClass literals are: "
                                            + string.Join(", ", GetValidCharacterClassLiterals()));
            }
            else
            {
                // Get the value of
                if (IsValidCharacterClass(value))
                {
                    _value = new PatternValue(value);
                }
                else
                {
                    var stringValue = GetCharacterClass(value);
                    _value = new PatternValue(stringValue!);
                }
            }
        }
    }

    [JsonPropertyName("Quantifiers")]
    [YamlMember(Alias = "Quantifiers")]
    public Quantifier? Quantifiers { get; set; }

    [JsonPropertyName("Message")]
    [YamlMember(Alias = "Message")]
    public string? Message { get; set; }

    [JsonPropertyName("Properties")]
    [YamlMember(Alias = "Properties")]
    public IPatternProperties? Properties { get; set; }

    [JsonIgnore]
    [YamlIgnore]
    public JsonSchema JsonSchema => JsonSchema.FromType(GetType());

    public CharacterClassPattern(string value, Quantifier? quantifiers = null)
    {
        Value = new PatternValue(value);
        Quantifiers = quantifiers;
    }

    public CharacterClassPattern(PatternValue value, Quantifier? quantifiers = null)
    {
        Value = value;
        Quantifiers = quantifiers;
    }

    public CharacterClassPattern()
    {
        Value = new PatternValue(string.Empty);
    }

    public CharacterClassPattern(string CharacterClassPatternObject)
    {
        if (CharacterClassPatternObject.StartsWith("\\"))
        {
            Value = new PatternValue(CharacterClassPatternObject);
        }
        else if (string.IsNullOrWhiteSpace(CharacterClassPatternObject))
        {
            Value = new PatternValue(string.Empty);
        }
        else if (IsJson(CharacterClassPatternObject))
        {
            DeserializeJson(CharacterClassPatternObject);
        }
        else if (IsYaml(CharacterClassPatternObject))
        {
            DeserializeYaml(CharacterClassPatternObject);
        }
    }

    public override string? ToString() => Value?.ToString() ?? string.Empty;

    internal bool IsJson(string patternObject) => ((IPattern)this).IsJson(patternObject);

    internal bool IsYaml(string patternObject) => ((IPattern)this).IsYaml(patternObject);

    internal void DeserializeYaml(string patternObject)
    {
        var deserializer = new Deserializer();
        var pattern = deserializer.Deserialize<Pattern>(patternObject);
        if (pattern != null)
        {
            Id = pattern.Id ?? Guid.NewGuid().ToString();
            Type = pattern.Type ?? "Literal";
            Value = pattern.Value ?? new PatternValue(string.Empty);
            Quantifiers = pattern.Quantifiers ?? null;
        }
    }

    internal void DeserializeJson(string patternObject)
    {
        var pattern = JsonSerializer.Deserialize<Pattern>(patternObject);
        if (pattern != null)
        {
            Id = pattern.Id;
            Type = pattern.Type;
            Value = (PatternValue)pattern.Value;
            Quantifiers = (Quantifier?)pattern.Quantifiers;
        }
    }

    public static string GetCharacterClass(string value)
    {
        // get the value of the appropriate CharacterClass from the static class CharacterClasses in FluentRegex
        string CharacterClass = default!;
        typeof(CharacterClasses).GetFields().ToList().ForEach(f =>
        {
            if (f.Name.ToLower() == value.ToLower())
            {
                CharacterClass = (string)f.GetValue(null)!;
            }
        });
        if (CharacterClass == default)
        {
            throw new ArgumentException("Invalid CharacterClass Type. Valid types are: " + string.Join(", ", GetValidCharacterClassTypes()));
        }
        return CharacterClass;
    }

    internal static bool IsValidCharacterClass(string literal)
    {
        return GetValidCharacterClassLiterals().Contains(literal);
    }

    private static List<string?> GetValidCharacterClassLiterals()
    {
        return typeof(CharacterClasses).GetFields().Select(f => f.GetValue(null))!.ToList<object>().ConvertAll<string?>(x => x?.ToString())!;
    }

    internal static bool IsValidCharacterClassType(string type)
    {
        return GetValidCharacterClassTypes().Contains(type);
    }

    internal static List<string?> GetValidCharacterClassTypes()
    {

        return typeof(CharacterClasses).GetFields().Select(f => f.Name).ToList()!;
    }

    internal static bool IsValidPatternType(string type)
    {
        return type == "Literal" || type == "CharacterClass" || type == "CharacterClass" || type == "Group";
    }
}