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
using Azure.AI.Details.Common.CLI.ConsoleGui;

namespace Azure.AI.Details.Common.CLI
{
    public partial class AzCliConsoleGui
    {
        public static async Task<AzCli.CognitiveServicesSpeechResourceInfo> PickOrCreateAndConfigCognitiveServicesSpeechServicesKindResource(bool interactive, string subscriptionId, string regionFilter = null, string groupFilter = null, string resourceFilter = null, string kinds = null, string sku = null, bool yes = false)
        {
            kinds ??= "SpeechServices";
            var sectionHeader = "SPEECH RESOURCE";

            var regionLocation = !string.IsNullOrEmpty(regionFilter) ? await AzCliConsoleGui.PickRegionLocationAsync(interactive, regionFilter) : new AzCli.AccountRegionLocationInfo();
            var resource = await AzCliConsoleGui.PickOrCreateCognitiveResource(sectionHeader, interactive, subscriptionId, regionLocation.Name, groupFilter, resourceFilter, kinds, sku, yes);

            var keys = await AzCliConsoleGui.LoadCognitiveServicesResourceKeys(sectionHeader, subscriptionId, resource);

            ConfigSetHelpers.ConfigSpeechResource(subscriptionId, resource.RegionLocation, resource.Endpoint, keys.Key1);

            return new AzCli.CognitiveServicesSpeechResourceInfo
            {
                Id = resource.Id,
                Group = resource.Group,
                Name = resource.Name,
                Kind = resource.Kind,
                RegionLocation = resource.RegionLocation,
                Endpoint = resource.Endpoint,
                Key = keys.Key1,
            };
        }
        public static async Task<AzCli.CognitiveServicesVisionResourceInfo> PickOrCreateAndConfigCognitiveServicesComputerVisionKindResource(bool interactive, string subscriptionId, string regionFilter = null, string groupFilter = null, string resourceFilter = null, string kinds = null, string sku = null, bool yes = false)
        {
            kinds ??= "ComputerVision";
            var sectionHeader = "VISION RESOURCE";

            var regionLocation = !string.IsNullOrEmpty(regionFilter) ? await AzCliConsoleGui.PickRegionLocationAsync(interactive, regionFilter) : new AzCli.AccountRegionLocationInfo();
            var resource = await AzCliConsoleGui.PickOrCreateCognitiveResource(sectionHeader, interactive, subscriptionId, regionLocation.Name, groupFilter, resourceFilter, kinds, sku, yes);

            var keys = await AzCliConsoleGui.LoadCognitiveServicesResourceKeys(sectionHeader, subscriptionId, resource);

            ConfigSetHelpers.ConfigVisionResource(subscriptionId, resource.RegionLocation, resource.Endpoint, keys.Key1);

            return new AzCli.CognitiveServicesVisionResourceInfo
            {
                Id = resource.Id,
                Group = resource.Group,
                Name = resource.Name,
                Kind = resource.Kind,
                RegionLocation = resource.RegionLocation,
                Endpoint = resource.Endpoint,
                Key = keys.Key1,
            };
        }
    }
}
