using System.Globalization;
using Irvuewin.Models.Unsplash;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Irvuewin.Helpers
{
    public static class JsonHelper
    {
        public static readonly JsonSerializerSettings? Settings = new()
            {
                MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
                DateParseHandling = DateParseHandling.None,
                NullValueHandling = NullValueHandling.Ignore,
                Formatting = Formatting.Indented,
                Converters =
                {
                    AssetTypeConverter.Singleton,
                    TypeEnumConverter.Singleton,
                    new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
                },
            };
    }

    public class AssetTypeConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(AssetType) || t == typeof(AssetType?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var value = serializer.Deserialize<string>(reader);
            if (value == "photo")
            {
                return AssetType.Photo;
            }

            throw new Exception("Cannot unmarshal type AssetType");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }

            var value = (AssetType)untypedValue;
            if (value == AssetType.Photo)
            {
                serializer.Serialize(writer, "photo");
                return;
            }

            throw new Exception("Cannot marshal type AssetType");
        }

        public static readonly AssetTypeConverter Singleton = new AssetTypeConverter();
    }

    public class TypeEnumConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(TypeEnum) || t == typeof(TypeEnum?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var value = serializer.Deserialize<string>(reader);
            if (value == "search")
            {
                return TypeEnum.Search;
            }

            throw new Exception("Cannot unmarshal type TypeEnum");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }

            var value = (TypeEnum)untypedValue;
            if (value == TypeEnum.Search)
            {
                serializer.Serialize(writer, "search");
                return;
            }

            throw new Exception("Cannot marshal type TypeEnum");
        }

        public static readonly TypeEnumConverter Singleton = new TypeEnumConverter();
    }
}