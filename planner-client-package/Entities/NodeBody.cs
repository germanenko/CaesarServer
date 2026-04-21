using planner_client_package.Interface;
using planner_common_package;
using planner_common_package.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace planner_client_package.Entities
{
    [JsonPolymorphic(TypeDiscriminatorPropertyName = Discriminator.TypeDiscriminatorPropertyName)]
    [JsonDerivedType(typeof(BoardBody), Discriminator.Board)]
    [JsonDerivedType(typeof(ColumnBody), Discriminator.Column)]
    [JsonDerivedType(typeof(JobBody), Discriminator.Job)]
    [JsonDerivedType(typeof(ChatBody), Discriminator.Chat)]
    [JsonDerivedType(typeof(MessageBody), Discriminator.ChatMessage)]
    public class NodeBody : ISyncable, IBody
    {
        public Guid Id { get; set; }
        public NodeType Type { get; set; }
        public SyncKind SyncKind { get; set; }
        public string Name { get; set; }
        public string Props { get; set; }
        public Guid UpdatedBy { get; set; }
        public DateTime UpdatedAt { get; set; }
        public long Version { get; set; }
        public long ScopeVersion { get; set; }
        public NodeLinkBody Link { get; set; }
        public AccessRuleBody AccessRule { get; set; }
        public IEnumerable<NodeBody> Childs { get; set; }

        public override bool Equals(object obj)
        {
            if (obj.GetType() == typeof(NodeBody))
            {
                var other = (NodeBody)obj;
                return Id == other.Id
                    && Name == other.Name
                    && Props == other.Props;
            }

            return base.Equals(obj);
        }
    }

    public static class NodeBodyExtensions
    {
        public static T ApplyNodeMetadata<T>(this T target, NodeBody source) where T : NodeBody
        {
            if (source == null) return target;

            target.Link = source.Link;
            target.AccessRule = source.AccessRule;
            target.Version = source.Version;
            target.ScopeVersion = source.ScopeVersion;
            target.SyncKind = source.SyncKind;

            return target;
        }

        public static IEnumerable<IBody> Extract<T>(this T target) where T : NodeBody
        {
            List<IBody> result = new List<IBody> { target };

            var accessRule = target.AccessRule;
            var subject = accessRule?.AccessSubject;

            result.AddIfNotNull(accessRule);
            result.AddIfNotNull(subject);

            if (subject is UserAccessSubjectBody user)
            {
                result.AddIfNotNull(user.Profile);
            }

            result.AddIfNotNull(target.Link);
            result.AddRangeIfNotNull(target.Childs);

            if (target.SyncKind == SyncKind.Scope)
            {
                result.Add(new ScopeVersionBody(target.Id, target.ScopeVersion));
            }

            return result;
        }

        public static void AddIfNotNull<T>(this List<T> list, T item) where T : class
        {
            if (item != null) list.Add(item);
        }

        public static void AddRangeIfNotNull<T>(this List<T> list, IEnumerable<T> items)
        {
            if (items != null) list.AddRange(items);
        }
    }
}
