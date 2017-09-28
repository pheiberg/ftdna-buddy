using System;
using FtdnaBuddy.Ftdna.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FtdnaBuddy.Ftdna
{
    public class LoginResultConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return true;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var result = new LoginResult();
            var jo = JObject.Load(reader);
            foreach(var property in jo)
            {
                switch(property.Key)
                {
                    case "lockedOut":
                        result.IsLockedOut = property.Value.Value<bool>();
                        break;
                    case "errorMessage":
                        result.ErrorMessage = property.Value.Value<string>();
                        break;
                    case "failedAccessAttempts":
                        result.FailedAccessAttempts = property.Value.Value<int>();
                        break;
                    case "returnUrl":
                        result.ReturnUrl = property.Value.Value<string>();
                        break;
                }
            }

            return result;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}