using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Oqtane.Controllers;
using Oqtane.Enums;
using Oqtane.Infrastructure;
using Oqtane.Shared;
using OpenEugene.LittleHelpBook.Models;
using OpenEugene.Module.Providers.Services;

namespace OpenEugene.Module.Providers.Controllers
{
    // Access is controlled by the calling module's View/Edit permissions
    // (the module id travels in the authorization querystring).
    [Route(ControllerRoutes.ApiRoute)]
    public class ProviderController : ModuleControllerBase
    {
        private readonly IProviderService _providerService;

        public ProviderController(IProviderService providerService, ILogManager logger, IHttpContextAccessor accessor) : base(logger, accessor)
        {
            _providerService = providerService;
        }

        // GET: api/<controller>
        [HttpGet]
        [Authorize(Policy = PolicyNames.ViewModule)]
        public async Task<ActionResult<List<ProviderListItem>>> Get()
        {
            try
            {
                return await _providerService.GetProvidersAsync(AuthEntityId(EntityNames.Module));
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Read, ex, "Error Loading Providers");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        // GET api/<controller>/5
        [HttpGet("{id:int}")]
        [Authorize(Policy = PolicyNames.ViewModule)]
        public async Task<ActionResult<ProviderDetail>> Get(int id)
        {
            try
            {
                var provider = await _providerService.GetProviderAsync(id, AuthEntityId(EntityNames.Module));
                if (provider == null)
                {
                    return StatusCode(StatusCodes.Status404NotFound);
                }
                return provider;
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Read, ex, "Error Loading Provider {ProviderId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        // GET api/<controller>/categories
        [HttpGet("categories")]
        [Authorize(Policy = PolicyNames.ViewModule)]
        public async Task<ActionResult<List<CategoryTree>>> GetCategories()
        {
            try
            {
                return await _providerService.GetCategoriesAsync(AuthEntityId(EntityNames.Module));
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Read, ex, "Error Loading Categories");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        // POST api/<controller>
        [HttpPost]
        [Authorize(Policy = PolicyNames.EditModule)]
        public async Task<ActionResult<ProviderDetail>> Post([FromBody] ProviderDetail provider)
        {
            if (!ModelState.IsValid || provider?.Provider == null)
            {
                return StatusCode(StatusCodes.Status400BadRequest);
            }
            try
            {
                provider = await _providerService.AddProviderAsync(provider, AuthEntityId(EntityNames.Module));
                _logger.Log(LogLevel.Information, this, LogFunction.Create, "Provider Added {ProviderId}", provider.Provider.ProviderId);
                return provider;
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Create, ex, "Error Adding Provider {Name}", provider.Provider.Name);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        // PUT api/<controller>/5
        [HttpPut("{id:int}")]
        [Authorize(Policy = PolicyNames.EditModule)]
        public async Task<ActionResult<ProviderDetail>> Put(int id, [FromBody] ProviderDetail provider)
        {
            if (!ModelState.IsValid || provider?.Provider == null || provider.Provider.ProviderId != id)
            {
                return StatusCode(StatusCodes.Status400BadRequest);
            }
            try
            {
                provider = await _providerService.UpdateProviderAsync(provider, AuthEntityId(EntityNames.Module));
                _logger.Log(LogLevel.Information, this, LogFunction.Update, "Provider Updated {ProviderId}", id);
                return provider;
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Update, ex, "Error Updating Provider {ProviderId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        // DELETE api/<controller>/5
        [HttpDelete("{id:int}")]
        [Authorize(Policy = PolicyNames.EditModule)]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                await _providerService.DeleteProviderAsync(id, AuthEntityId(EntityNames.Module));
                _logger.Log(LogLevel.Information, this, LogFunction.Delete, "Provider Deleted {ProviderId}", id);
                return StatusCode(StatusCodes.Status200OK);
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Delete, ex, "Error Deleting Provider {ProviderId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
    }
}
