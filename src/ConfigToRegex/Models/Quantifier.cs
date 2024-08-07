using System.Runtime.Serialization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using NJsonSchema;
using YamlDotNet.Serialization;
using ConfigToRegex.Exceptions;

namespace ConfigToRegex;

/// <summary>
/// Represents a Regular Expression Quantifier as an object. This can be used on any <see cref="IPattern"/> object.
/// </summary>
[YamlSerializable]
public class Quantifier : IQuantifier
{
  /// <summary>
  /// The minimum number of times the pattern must match.
  /// </summary>
  [JsonPropertyName("Min")]
  [YamlMember(Alias = "Min", Description = "The minimum number of times the pattern must match.")]
  public int? Min { get; set; }

  /// <summary>
  /// The maximum number of times the pattern can match.
  /// </summary>
  [JsonPropertyName("Max")]
  [YamlMember(Alias = "Max", Description = "The maximum number of times the pattern can match.")]
  public int? Max { get; set; }

  /// <summary>
  /// The exact number of times the pattern must match.
  /// </summary>
  [JsonPropertyName("Exactly")]
  [YamlMember(Alias = "Exactly", Description = "The exact number of times the pattern must match.")]
  public int? Exactly { get; set; }

  /// <summary>
  /// Whether the quantifier is lazy.
  /// </summary>
  [JsonPropertyName("Lazy")]
  [YamlMember(Alias = "Lazy", Description = "Whether the quantifier is lazy.")]
  public bool? Lazy { get; set; }

  /// <summary>
  /// Whether the quantifier is greedy.
  /// </summary>
  [JsonPropertyName("Greedy")]
  [YamlMember(Alias = "Greedy", Description = "Whether the quantifier is greedy.")]
  public bool? Greedy { get; set; }

  /// <summary>
  /// The JSON Schema for the <see cref="Quantifier"/> object. This is a generated property and is not meant to be used directly.
  /// </summary>
  [JsonIgnore]
  [YamlIgnore]
  public JsonSchema JsonSchema => JsonSchema.FromType(GetType());

  public Quantifier()
  {
  }

  public Quantifier(int? min, int? max, int? exactly, bool? lazy, bool? greedy)
  {
    Min = min;
    Max = max;
    Exactly = exactly;
    Lazy = lazy;
    Greedy = greedy;
  }

  public Quantifier(string quantifierObject)
  {
    // deserialize the string using json or yaml

    if (!string.IsNullOrEmpty(quantifierObject) && quantifierObject.StartsWith('{') && quantifierObject.EndsWith('}'))
    {
      var deserializedValue = JsonSerializer.Deserialize<Quantifier>(quantifierObject);
      Min = deserializedValue!.Min;
      Max = deserializedValue!.Max;
      Exactly = deserializedValue!.Exactly;
      Lazy = deserializedValue!.Lazy;
      Greedy = deserializedValue!.Greedy;
    }
    else
    {
      // if this is yaml
      var deserializer = new Deserializer();
      var deserializedValue = deserializer.Deserialize<Quantifier>(quantifierObject);
      Min = deserializedValue!.Min;
      Max = deserializedValue!.Max;
      Exactly = deserializedValue!.Exactly;
      Lazy = deserializedValue!.Lazy;
      Greedy = deserializedValue!.Greedy;
    }
  }

  void IRegexSerializable.DeserializeJson(string jsonString)
  {
    DeserializeJson(jsonString);
  }

  void IRegexSerializable.DeserializeYaml(string yamlString)
  {
    DeserializeYaml(yamlString);
  }

  internal bool CanBeGreedy(string pattern)
  {
    return Greedy.HasValue
            && Greedy == true
            && !pattern.EndsWith('*')
            && !pattern.EndsWith('+')
            && !pattern.EndsWith('?')
            && !Exactly.HasValue
            && !Min.HasValue
            && !Max.HasValue;
  }

  string IRegexSerializable.ToRegex()
  {
    return ToRegex();
  }


  /// <summary>
  /// Serializes the object to a YAML string
  /// </summary>
  /// <returns><see cref="string"/> representing the object in YAML format</returns>
  public string SerializeYaml()
  {
    var serializer = new SerializerBuilder().Build();
    var yaml = serializer.Serialize(this);
    return yaml;
  }

  /// <summary>
  /// Serializes the object to a JSON string
  /// </summary>
  /// <returns><see cref="string"/> representing the object in JSON format</returns>
  public string SerializeJson()
  {
    var json = JsonSerializer.Serialize(this);
    return json;
  }

  /// <summary>
  /// Deserializes a YAML string to this instance of <see cref="Quantifier"/>
  /// </summary>
  /// <param name="yamlString"></param>
  public void DeserializeYaml(string yamlString)
  {
    var deserializer = new Deserializer();
    var pattern = deserializer.Deserialize<Quantifier>(yamlString);
    if (pattern != null)
    {
      Min = pattern.Min ?? null;
      Max = pattern.Max ?? null;
      Exactly = pattern.Exactly ?? null;
      Lazy = pattern.Lazy ?? null;
      Greedy = pattern.Greedy ?? null;
    }
  }

  /// <summary>
  /// Deserializes a JSON string to this instance of <see cref="Quantifier"/>
  /// </summary>
  /// <param name="jsonString"></param>
  /// <exception cref="Exception"></exception>
  public void DeserializeJson(string jsonString)
  {
    var pattern = JsonSerializer.Deserialize<Quantifier>(jsonString) ?? throw new InvalidJsonException("Invalid JSON");

    if (pattern != null)
    {
      Min = pattern.Min ?? null;
      Max = pattern.Max ?? null;
      Exactly = pattern.Exactly ?? null;
      Lazy = pattern.Lazy ?? null;
      Greedy = pattern.Greedy ?? null;
    }

  }

  public string ToRegex(string pattern = "")
  {
    // This takes the pattern as a parameter so that we can avoid invalid regex (I.E. **)
    StringBuilder sb = new();

    BuildQuantifierString(sb);

    ProcessQuantifierLaziness(pattern, sb);

    return sb.ToString();
  }

  private void ProcessQuantifierLaziness(string pattern, StringBuilder sb)
  {
    if (CanBeGreedy(pattern))
    {
      sb.Append('*');
    }

    if (Lazy.HasValue && Lazy == true && !pattern.EndsWith('?'))
    {
      // If this is appended when there are min/max declared, it can still be valid.
      // Example:
      // | ConfigToRegex |	Description |
      // | ---      |   ---         |
      // | A{2,9}? |	Two to nine As, as few as needed to allow the overall pattern to match (lazy) |
      sb.Append('?');
    }
  }

  private void BuildQuantifierString(StringBuilder sb)
  {
    if (Exactly.HasValue && Exactly > 0)
    {
      sb.Append($"{{{Exactly}}}");
    }
    else if (Min == 0 && !Max.HasValue)
    {
      sb.Append('*');
    }
    else if (Min == 1 && !Max.HasValue)
    {
      sb.Append('+');
    }
    else if (Min == 0 && Max == 1)
    {
      sb.Append('?');
    }
    else if (Min.HasValue && Max.HasValue && Max >= Min)
    {
      sb.Append($"{{{Min},{Max}}}");
    }
    else if (Min.HasValue && Max.HasValue && Max < Min)
    {
      throw new SerializationException("Max must be greater than or equal to Min.");
    }
  }
}
