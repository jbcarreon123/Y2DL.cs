﻿// From MiYanni on:
// https://github.com/aaubry/YamlDotNet/issues/30#issuecomment-538604539

using System.Reflection;
using System.Runtime.Serialization;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace Y2DL.Utils;

public class YamlStringEnumConverter : IYamlTypeConverter
{
    public bool Accepts(Type type)
    {
        return type.IsEnum;
    }

    public object ReadYaml(IParser parser, Type type)
    {
        var parsedEnum = parser.Expect<Scalar>();
        var serializableValues = type.GetMembers()
            .Select(m =>
                new KeyValuePair<string, MemberInfo>(
                    m.GetCustomAttributes<EnumMemberAttribute>(true).Select(ema => ema.Value).FirstOrDefault(), m))
            .Where(pa => !string.IsNullOrEmpty(pa.Key)).ToDictionary(pa => pa.Key, pa => pa.Value);
        if (!serializableValues.ContainsKey(parsedEnum.Value))
            throw new YamlException(parsedEnum.Start, parsedEnum.End,
                $"Value '{parsedEnum.Value}' not found in enum '{type.Name}'");

        return Enum.Parse(type, serializableValues[parsedEnum.Value].Name);
    }

    public void WriteYaml(IEmitter emitter, object value, Type type)
    {
        var enumMember = type.GetMember(value.ToString()).FirstOrDefault();
        var yamlValue =
            enumMember?.GetCustomAttributes<EnumMemberAttribute>(true).Select(ema => ema.Value).FirstOrDefault() ??
            value.ToString();
        emitter.Emit(new Scalar(yamlValue));
    }
}