using System.Text.Json.Serialization;

namespace WebApi.Entities;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Role
{
    Admin,
    User
}