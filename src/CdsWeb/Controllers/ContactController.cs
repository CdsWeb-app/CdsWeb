using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.PowerPlatform.Cds.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;

namespace CdsWeb.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ContactController : ControllerBase
    {
        private readonly ILogger<ContactController> _logger;
        private readonly IOrganizationService _orgService;
        private readonly CdsServiceClient _cdsServiceClient;

        public ContactController(ILogger<ContactController> logger, IOrganizationService orgService)
        {
            _logger = logger;
            _orgService = orgService;
            _cdsServiceClient = (CdsServiceClient)_orgService;
        }

        [HttpGet]
        public IEnumerable<Entity> Get()
        {
            var fetchxml =
                $@"<fetch version='1.0' output-format='xml-platform' mapping='logical'>
                        <entity name='contact'>
                        <all-attributes />
                        </entity>
                    </fetch>";

            var contactResponse = _cdsServiceClient.RetrieveMultiple(new FetchExpression(fetchxml));

            return contactResponse.Entities;
        }


        [HttpGet("{id}")]
        public string Get(Guid id)
        {
            var contactResponse = _orgService.Retrieve("contact", id, new ColumnSet(true));

            return contactResponse.GetAttributeValue<string>("fullname");
        }
    }
}
