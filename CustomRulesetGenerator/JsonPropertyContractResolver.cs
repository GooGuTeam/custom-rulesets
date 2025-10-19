// Copyright (c) 2025 GooGuTeam
// Licensed under the AGPL-3.0 Licence. See the LICENCE file in the repository root for full licence text.

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace CustomRulesetGenerator
{
    public class JsonPropertyContractResolver : DefaultContractResolver
    {
        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
        {
            return base.CreateProperties(type, memberSerialization)
                .Where(p => p.AttributeProvider.GetAttributes(typeof(JsonPropertyAttribute), true).Any())
                .Select(p =>
                {
                    var jsonProperty = (JsonPropertyAttribute)p.AttributeProvider.GetAttributes(typeof(JsonPropertyAttribute), true).First();
                    p.PropertyName = jsonProperty.PropertyName;
                    return p;
                })
                .ToList();
        }
    }
}