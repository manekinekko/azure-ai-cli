//
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE.md file in the project root for full license information.
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using YamlDotNet.RepresentationModel;

namespace Azure.AI.Details.Common.CLI.TestFramework
{
    public class YamlTagHelpers
    {
        public static FileInfo? FindDefaultTagsFile(DirectoryInfo directory)
        {
            var found = directory.GetFiles(YamlTestFramework.YamlDefaultTagsFileName);
            return found.Length == 1
                ? found[0]
                : directory.Parent != null
                    ? FindDefaultTagsFile(directory.Parent)
                    : null;
        }

        public static Dictionary<string, List<string>> FindAndGetDefaultTags(DirectoryInfo directory)
        {
            var defaultsFile = FindDefaultTagsFile(directory)?.FullName;
            return defaultsFile != null
                ? GetTagsFromFile(defaultsFile)
                : new Dictionary<string, List<string>>();
        }

        public static Dictionary<string, List<string>> GetTagsFromFile(string defaultsFile)
        {
            var defaultTags = new Dictionary<string, List<string>>();

            Logger.Log($"Loading default tags from {defaultsFile}");
            var parsed = YamlHelpers.ParseYamlStream(defaultsFile);
            if (parsed.Documents.Count() > 0)
            {
                var tagsNode = parsed.Documents[0].RootNode;
                if (tagsNode != null)
                {
                    defaultTags = UpdateCopyTags(defaultTags, null, tagsNode);
                }
            }

            return defaultTags;
        }

        public static Dictionary<string, List<string>> UpdateCopyTags(Dictionary<string, List<string>> tags, YamlMappingNode mapping)
        {
            var tagNode = mapping.Children.ContainsKey("tag") ? mapping.Children["tag"] : null;
            var tagsNode = mapping.Children.ContainsKey("tags") ? mapping.Children["tags"] : null;
            if (tagNode == null && tagsNode == null) return tags;

            return UpdateCopyTags(tags, tagNode, tagsNode);
        }

        private static Dictionary<string, List<string>> UpdateCopyTags(Dictionary<string, List<string>> tags, YamlNode? tagNode, YamlNode? tagsNode)
        {
            // make a copy that we'll update and return
            tags = new Dictionary<string, List<string>>(tags);

            var value = (tagNode as YamlScalarNode)?.Value;
            AddOptionalTag(tags, "tag", value);

            var values = (tagsNode as YamlScalarNode)?.Value;
            AddOptionalCommaSeparatedTags(tags, values);

            AddOptionalNameValueTags(tags, tagsNode as YamlMappingNode);
            AddOptionalTagsForEachChild(tags, tagsNode as YamlSequenceNode);

            return tags;
        }

        private static void AddOptionalTag(Dictionary<string, List<string>> tags, string name, string? value)
        {
            if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(value))
            {
                if (!tags.ContainsKey(name))
                {
                    tags.Add(name, new List<string>());
                }
                tags[name].Add(value);
            }
        }

        private static void AddOptionalCommaSeparatedTags(Dictionary<string, List<string>> tags, string? values)
        {
            if (values != null)
            {
                foreach (var tag in values.Split(",".ToArray(), StringSplitOptions.RemoveEmptyEntries))
                {
                    AddOptionalTag(tags, "tag", tag);
                }
            }
        }

        private static void AddOptionalNameValueTags(Dictionary<string, List<string>> tags, YamlMappingNode? mapping, string keyPrefix = "")
        {
            var children = mapping?.Children;
            if (children == null) return;

            foreach (var child in children)
            {
                var key = keyPrefix + (child.Key as YamlScalarNode)?.Value;

                if (child.Value is YamlScalarNode asScalar)
                {
                    var value = asScalar.Value;
                    AddOptionalTag(tags, key, value);
                }
                else if (child.Value is YamlSequenceNode || child.Value is YamlMappingNode)
                {
                    var value = child.Value.ToJsonString();
                    AddOptionalTag(tags, key, value);
                }
                else if(child.Value is YamlMappingNode asMapping)
                {
                    AddOptionalNameValueTags(tags, asMapping, $"{key}.");
                }
            }
        }

        private static void AddOptionalTagsForEachChild(Dictionary<string, List<string>> tags, YamlSequenceNode? sequence)
        {
            var children = sequence?.Children;
            if (children == null) return;

            foreach (var child in children)
            {
                if (child is YamlScalarNode asScalar)
                {
                    AddOptionalTag(tags, "tag", asScalar.Value);
                    continue;
                }

                if (child is YamlMappingNode asMapping)
                {
                    AddOptionalNameValueTags(tags, asMapping);
                    continue;
                }
            }
        }
    }
}
