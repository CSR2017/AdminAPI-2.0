// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System.Threading.Tasks;
#if NET48
using EdFi.Ods.Common;
#else
using EdFi.Common;
#endif
using RestSharp;

namespace EdFi.Ods.AdminApp.Management.Api
{
    public class OdsRestClientFactory : IOdsRestClientFactory
    {
        private readonly IOdsApiConnectionInformationProvider _odsApiConnectionInformationProvider;
        private TokenRetriever _tokenRetriever;
        private RestClient _restClient;

        public OdsRestClientFactory(IOdsApiConnectionInformationProvider odsApiConnectionInformationProvider)
        {
            _odsApiConnectionInformationProvider = odsApiConnectionInformationProvider;
        }

        public async Task<IOdsRestClient> Create()
        {
            var connectionInfo = await _odsApiConnectionInformationProvider.GetConnectionInformationForEnvironment();
            _tokenRetriever = new TokenRetriever(connectionInfo);
            _restClient = new RestClient(connectionInfo.ApiBaseUrl);
            return new OdsRestClient(connectionInfo, _restClient, _tokenRetriever);
        }
    }
}
