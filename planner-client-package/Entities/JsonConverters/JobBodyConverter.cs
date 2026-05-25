using planner_client_package.Entities;
using planner_common_package;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

public class JobBodyConverter : JsonConverter<JobBody>
{
    public override JobBody Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        using var doc = JsonDocument.ParseValue(ref reader);

        var root = doc.RootElement;

        var type = root
            .GetProperty(Discriminator.TypeDiscriminatorPropertyName)
            .GetString();

        var json = root.GetRawText();

        return type switch
        {
            Discriminator.Task =>
                JsonSerializer.Deserialize<TaskBody>(json, options),

            Discriminator.Meeting =>
                JsonSerializer.Deserialize<MeetingBody>(json, options),

            Discriminator.Information =>
                JsonSerializer.Deserialize<InformationBody>(json, options),

            Discriminator.Reminder =>
                JsonSerializer.Deserialize<ReminderBody>(json, options),

            _ => throw new Exception($"Unknown type {type}")
        };
    }

    public override void Write(
        Utf8JsonWriter writer,
        JobBody value,
        JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        var discriminator = value switch
        {
            TaskBody => Discriminator.Task,
            MeetingBody => Discriminator.Meeting,
            InformationBody => Discriminator.Information,
            ReminderBody => Discriminator.Reminder,
            _ => throw new Exception($"Unknown type {value.GetType()}")
        };

        writer.WriteString(
            Discriminator.TypeDiscriminatorPropertyName,
            discriminator);

        var json = JsonSerializer.SerializeToElement(
            value,
            value.GetType(),
            options);

        foreach (var property in json.EnumerateObject())
        {
            property.WriteTo(writer);
        }

        writer.WriteEndObject();
    }
}