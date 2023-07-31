//
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE.md file in the project root for full license information.
//

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Net;
using Newtonsoft.Json.Linq;
using Azure.AI.Details.Common.CLI.ConsoleGui;

namespace Azure.AI.Details.Common.CLI
{
    public partial class AzCliConsoleGui
    {
        public class AiResourcePicker
        {
            public static async Task<AzCli.CognitiveServicesResourceInfo> PickOrCreateCognitiveResource(bool interactive, string subscriptionId = null, string regionFilter = null, string groupFilter = null, string resourceFilter = null, string kind = null, string sku = "F0", bool agreeTerms = false)
            {
                var createNewItem = !string.IsNullOrEmpty(resourceFilter)
                    ? $"(Create `{resourceFilter}`)"
                    : interactive ? "(Create new)" : null;

                (var resource, var error) = await FindCognitiveServicesResource(interactive, subscriptionId, regionFilter, groupFilter, resourceFilter, kind, createNewItem);
                if (resource != null && resource.Value.Name == null)
                {
                    (resource, error) = await TryCreateCognitiveServicesResource(interactive, subscriptionId, regionFilter, groupFilter, resourceFilter, kind, sku, agreeTerms);
                }

                if (resource == null && error != null)
                {
                    throw new ApplicationException($"ERROR: Loading or creating resource:\n{error}");
                }
                else if (resource == null)
                {
                    throw new ApplicationException($"CANCELED: No resource selected");
                }

                return resource.Value;
            }

            // public static async Task<AzCli.CognitiveServicesDeploymentInfo> PickOrCreateCognitiveResourceDeployment(bool interactive, string subscriptionId, string regionLocation, string group, string resourceName, string deploymentFilter)
            // {
            //     var createNewItem = !string.IsNullOrEmpty(deploymentFilter)
            //         ? $"(Create `{deploymentFilter}`)"
            //         : interactive ? "(Create new)" : null;

            //     (var deployment, var error) = await FindCognitiveServicesResourceDeployment(interactive, subscriptionId, regionLocation, group, resourceName, deploymentFilter, createNewItem);
            //     if (deployment != null && deployment.Value.Name == null)
            //     {
            //         (deployment, error) = await TryCreateCognitiveServicesResourceDeployment(interactive, subscriptionId, regionLocation, group, resourceName, deploymentFilter);
            //     }

            //     if (deployment == null && error != null)
            //     {
            //         throw new ApplicationException($"ERROR: Loading or creating resource deployment:\n{error}");
            //     }
            //     else if (deployment == null)
            //     {
            //         throw new ApplicationException($"CANCELED: No resource deployment selected");
            //     }

            //     return deployment.Value;
            // }

            public static async Task<AzCli.CognitiveServicesKeyInfo> LoadCognitiveServicesResourceKeys(string subscriptionId, AzCli.CognitiveServicesResourceInfo resource)
            {
                ConsoleHelpers.WriteLineWithHighlight($"\n`{Program.SERVICE_RESOURCE_DISPLAY_NAME_ALL_CAPS} KEYS`");

                Console.Write("Keys: *** Loading ***");
                var keys = await AzCli.ListCognitiveServicesKeys(subscriptionId, resource.Group, resource.Name);

                Console.Write("\r");
                Console.WriteLine($"Key1: {keys.Payload.Key1.Substring(0, 4)}****************************");
                Console.WriteLine($"Key2: {keys.Payload.Key2.Substring(0, 4)}****************************");
                return keys.Payload;
            }

            public static async Task<(AzCli.CognitiveServicesResourceInfo? Resource, string Error)> FindCognitiveServicesResource(bool interactive, string subscriptionId = null, string regionLocationFilter = null, string groupFilter = null, string resourceFilter = null, string kind = null, string allowCreateResourceOption = null)
            {
                var allowCreateResource = !string.IsNullOrEmpty(allowCreateResourceOption);

                Console.Write("\rName: *** Loading choices ***");
                var response = await AzCli.ListCognitiveServicesResources(subscriptionId, kind);

                Console.Write("\rName: ");
                if (string.IsNullOrEmpty(response.StdOutput) && !string.IsNullOrEmpty(response.StdError))
                {
                    ConsoleHelpers.WriteLineError($"ERROR: Loading Cognitive Services resources: {response.StdError}");
                    return (null, response.StdError);
                }

                var resources = response.Payload
                    .Where(x => MatchResourceFilter(x, regionLocationFilter, groupFilter, resourceFilter))
                    .OrderBy(x => x.Name + x.RegionLocation)
                    .ToList();

                var exactMatch = resourceFilter != null && resources.Count(x => ExactMatchResource(x, regionLocationFilter, groupFilter, resourceFilter)) == 1;
                if (exactMatch) resources = resources.Where(x => ExactMatchResource(x, regionLocationFilter, groupFilter, resourceFilter)).ToList();

                if (resources.Count() == 0)
                {
                    if (!allowCreateResource)
                    {
                        ConsoleHelpers.WriteLineError($"*** No Cognitive Services resources found ***");
                        return (null, null);
                    }
                    else if (!interactive)
                    {
                        Console.WriteLine(allowCreateResourceOption);
                        return (new AzCli.CognitiveServicesResourceInfo(), null);
                    }
                }
                else if (resources.Count() == 1 && (!interactive || exactMatch))
                {
                    var resource = resources.First();
                    DisplayName(resource);
                    return (resource, null);
                }
                else if (!interactive)
                {
                    ConsoleHelpers.WriteLineError("*** More than 1 resource found ***");
                    Console.WriteLine();
                    DisplayResources(resources, "  ");
                    return (null, null);
                }

                return interactive
                    ? ListBoxPickCognitiveServicesResource(resources.ToArray(), allowCreateResourceOption)
                    : (null, null);
            }

            public static async Task<(AzCli.CognitiveServicesResourceInfo? Resource, string Error)> TryCreateCognitiveServicesResource(bool interactive, string subscriptionId = null, string regionLocationFilter = null, string groupFilter = null, string resourceFilter = null, string kind = null, string sku = "F0", bool agreeTerms = false)
            {
                ConsoleHelpers.WriteLineWithHighlight("\n`RESOURCE GROUP`");

                (var regionLocation, var errorLoc) = await RegionLocationPicker.FindRegionAsync(interactive, regionLocationFilter, true);
                if (regionLocation == null) return (null, errorLoc);

                var group = await ResourceGroupPicker.PickOrCreateResourceGroup(interactive, subscriptionId, regionLocationFilter, groupFilter);

                ConsoleHelpers.WriteLineWithHighlight($"\n`CREATE {Program.SERVICE_RESOURCE_DISPLAY_NAME_ALL_CAPS}`");
                Console.WriteLine($"Region: {group.RegionLocation}");
                Console.WriteLine($"Group: {group.Name}");

                var name = AskPrompt("Name: ", resourceFilter);
                if (string.IsNullOrEmpty(name)) return (null, null);
                if (!agreeTerms && !CheckAgreeTerms(kind)) return (null, null);

                Console.Write("*** CREATING ***");
                var response = await AzCli.CreateCognitiveServicesResource(subscriptionId, group.Name, group.RegionLocation, name, kind, sku);

                Console.Write("\r");
                if (string.IsNullOrEmpty(response.StdOutput) && !string.IsNullOrEmpty(response.StdError))
                {
                    ConsoleHelpers.WriteLineError($"ERROR: Creating Cognitive Services resources: {response.StdError}");
                    return (null, response.StdError);
                }

                Console.WriteLine("\r*** CREATED ***  ");
                return (response.Payload, null);
            }

            private static (AzCli.CognitiveServicesResourceInfo? Resource, string Error) ListBoxPickCognitiveServicesResource(AzCli.CognitiveServicesResourceInfo[] resources, string p0)
            {
                var list = resources.Select(x => $"{x.Name} ({x.RegionLocation}, {x.Kind})").ToList();

                var hasP0 = !string.IsNullOrEmpty(p0);
                if (hasP0) list.Insert(0, p0);

                var normal = new Colors(ConsoleColor.White, ConsoleColor.Blue);
                var selected = new Colors(ConsoleColor.White, ConsoleColor.Red);

                var picked = ListBoxPicker.PickIndexOf(list.ToArray(), 60, 30, normal, selected);
                if (picked < 0)
                {
                    return (null, null);
                }

                if (hasP0 && picked == 0)
                {
                    Console.WriteLine(p0);
                    return (new AzCli.CognitiveServicesResourceInfo(), null);
                }

                if (hasP0) picked--;
                Console.WriteLine($"{resources[picked].Name}");
                return (resources[picked], null);
            }

            private static bool ExactMatchResource(AzCli.CognitiveServicesResourceInfo resource, string regionLocationFilter, string groupFilter, string resourceFilter)
            {
                return !string.IsNullOrEmpty(resourceFilter) && resource.Name.ToLower() == resourceFilter &&
                    (string.IsNullOrEmpty(regionLocationFilter) || resource.RegionLocation.ToLower() == regionLocationFilter) &&
                    (string.IsNullOrEmpty(groupFilter) || resource.Group.ToLower() == groupFilter);
            }

            private static bool MatchResourceFilter(AzCli.CognitiveServicesResourceInfo resource, string regionLocationFilter, string groupFilter, string resourceFilter)
            {
                if (resourceFilter != null && ExactMatchResource(resource, regionLocationFilter, groupFilter, resourceFilter))
                {
                    return true;
                }

                var name = resource.Name.ToLower();
                var group = resource.Group.ToLower();
                var regionLocation = resource.RegionLocation.ToLower();

                return (string.IsNullOrEmpty(resourceFilter) || name.Contains(resourceFilter) || StringHelpers.ContainsAllCharsInOrder(name, resourceFilter)) &&
                    (string.IsNullOrEmpty(regionLocationFilter) || name.Contains(regionLocationFilter) || StringHelpers.ContainsAllCharsInOrder(regionLocation, regionLocationFilter)) &&
                    (string.IsNullOrEmpty(groupFilter) || name.Contains(groupFilter) || StringHelpers.ContainsAllCharsInOrder(group, groupFilter));
            }

            private static void DisplayResources(List<AzCli.CognitiveServicesResourceInfo> resources, string prefix)
            {
                foreach (var resource in resources)
                {
                    Console.Write(prefix);
                    DisplayName(resource);
                }
            }

            private static void DisplayName(AzCli.CognitiveServicesResourceInfo resource)
            {
                Console.Write($"{resource.Name} ({resource.RegionLocation})");
                Console.WriteLine(new string(' ', 20));
            }

            private static bool CheckAgreeTerms(string kind)
            {
                var checkAttestation = kind.ToLower() switch
                {
                    "face" => true,
                    "cognitiveservices" => true,
                    _ => false
                };

                if (checkAttestation)
                {
                    Console.Write("\nI certify that use of this service is not by or for a police department in the United States: ");
                    var yesOrNo = ListBoxPickYesNo();
                    Console.WriteLine();
                    return yesOrNo;
                }

                return true;
            }

            private static bool ListBoxPickYesNo()
            {
                var choices = "Yes;No".Split(';').ToArray();
                var normal = new Colors(ConsoleColor.White, ConsoleColor.Blue);
                var selected = new Colors(ConsoleColor.White, ConsoleColor.Red);

                var picked = ListBoxPicker.PickIndexOf(choices, int.MinValue, 4, normal, selected);
                Console.WriteLine(picked switch {
                    0 => "Yes",
                    1 => "No",
                    _ => "Canceled"
                });
                return (picked == 0);
            }
        }
    }
}