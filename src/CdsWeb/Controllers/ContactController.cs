using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CdsWeb.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ContactController : ControllerBase
    {
        private readonly ILogger<ContactController> _logger;
        private readonly IOrganizationService _orgService;

        public ContactController(ILogger<ContactController> logger, IOrganizationService orgService)
        {
            _logger = logger;
            _orgService = orgService;
        }

        [HttpGet]
        public IEnumerable<Guid> Get()
        {
            var fetchxml =
                $@"<fetch version='1.0' output-format='xml-platform' mapping='logical'>
                        <entity name='contact'>
                        <all-attributes />
                        </entity>
                    </fetch>";

            var contactResponse = _orgService.RetrieveMultiple(new FetchExpression(fetchxml));

            return contactResponse.Entities.Select(a => a.Id);
        }


        [HttpGet("{id}")]
        public string Get(Guid id)
        {
            var contactResponse = _orgService.Retrieve("contact", id, new ColumnSet(true));

            return contactResponse.GetAttributeValue<string>("fullname");
        }
    }
}
