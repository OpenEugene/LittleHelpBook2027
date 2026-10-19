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
using OpenEugene.Module.Categories.Services;

namespace OpenEugene.Module.Categories.Controllers
{
    // Access is controlled by the calling module's View/Edit permissions
    // (the module id travels in the authorization querystring).
    [Route(ControllerRoutes.ApiRoute)]
    public class CategoryController : ModuleControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoryController(ICategoryService categoryService, ILogManager logger, IHttpContextAccessor accessor) : base(logger, accessor)
        {
            _categoryService = categoryService;
        }

        // GET: api/<controller>
        [HttpGet]
        [Authorize(Policy = PolicyNames.ViewModule)]
        public async Task<ActionResult<List<CategoryTree>>> Get()
        {
            try
            {
                return await _categoryService.GetCategoriesAsync(AuthEntityId(EntityNames.Module));
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Read, ex, "Error Loading Categories");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        // GET api/<controller>/5
        [HttpGet("{id:int}")]
        [Authorize(Policy = PolicyNames.ViewModule)]
        public async Task<ActionResult<Category>> Get(int id)
        {
            try
            {
                var category = await _categoryService.GetCategoryAsync(id, AuthEntityId(EntityNames.Module));
                if (category == null)
                {
                    return StatusCode(StatusCodes.Status404NotFound);
                }
                return category;
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Read, ex, "Error Loading Category {CategoryId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        // POST api/<controller>
        [HttpPost]
        [Authorize(Policy = PolicyNames.EditModule)]
        public async Task<ActionResult<Category>> Post([FromBody] Category category)
        {
            if (!ModelState.IsValid || category == null)
            {
                return StatusCode(StatusCodes.Status400BadRequest);
            }
            try
            {
                category = await _categoryService.AddCategoryAsync(category, AuthEntityId(EntityNames.Module));
                _logger.Log(LogLevel.Information, this, LogFunction.Create, "Category Added {CategoryId}", category.CategoryId);
                return category;
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Create, ex, "Error Adding Category {Name}", category.Name);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        // PUT api/<controller>/5
        [HttpPut("{id:int}")]
        [Authorize(Policy = PolicyNames.EditModule)]
        public async Task<ActionResult<Category>> Put(int id, [FromBody] Category category)
        {
            if (!ModelState.IsValid || category == null || category.CategoryId != id)
            {
                return StatusCode(StatusCodes.Status400BadRequest);
            }
            try
            {
                category = await _categoryService.UpdateCategoryAsync(category, AuthEntityId(EntityNames.Module));
                _logger.Log(LogLevel.Information, this, LogFunction.Update, "Category Updated {CategoryId}", id);
                return category;
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Update, ex, "Error Updating Category {CategoryId}", id);
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
                await _categoryService.DeleteCategoryAsync(id, AuthEntityId(EntityNames.Module));
                _logger.Log(LogLevel.Information, this, LogFunction.Delete, "Category Deleted {CategoryId}", id);
                return StatusCode(StatusCodes.Status200OK);
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Delete, ex, "Error Deleting Category {CategoryId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        // GET api/<controller>/sub/5
        [HttpGet("sub/{id:int}")]
        [Authorize(Policy = PolicyNames.ViewModule)]
        public async Task<ActionResult<SubCategory>> GetSubCategory(int id)
        {
            try
            {
                var subCategory = await _categoryService.GetSubCategoryAsync(id, AuthEntityId(EntityNames.Module));
                if (subCategory == null)
                {
                    return StatusCode(StatusCodes.Status404NotFound);
                }
                return subCategory;
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Read, ex, "Error Loading SubCategory {SubCategoryId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        // POST api/<controller>/sub
        [HttpPost("sub")]
        [Authorize(Policy = PolicyNames.EditModule)]
        public async Task<ActionResult<SubCategory>> PostSubCategory([FromBody] SubCategory subCategory)
        {
            if (!ModelState.IsValid || subCategory == null)
            {
                return StatusCode(StatusCodes.Status400BadRequest);
            }
            try
            {
                subCategory = await _categoryService.AddSubCategoryAsync(subCategory, AuthEntityId(EntityNames.Module));
                _logger.Log(LogLevel.Information, this, LogFunction.Create, "SubCategory Added {SubCategoryId}", subCategory.SubCategoryId);
                return subCategory;
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Create, ex, "Error Adding SubCategory {Name}", subCategory.Name);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        // PUT api/<controller>/sub/5
        [HttpPut("sub/{id:int}")]
        [Authorize(Policy = PolicyNames.EditModule)]
        public async Task<ActionResult<SubCategory>> PutSubCategory(int id, [FromBody] SubCategory subCategory)
        {
            if (!ModelState.IsValid || subCategory == null || subCategory.SubCategoryId != id)
            {
                return StatusCode(StatusCodes.Status400BadRequest);
            }
            try
            {
                subCategory = await _categoryService.UpdateSubCategoryAsync(subCategory, AuthEntityId(EntityNames.Module));
                _logger.Log(LogLevel.Information, this, LogFunction.Update, "SubCategory Updated {SubCategoryId}", id);
                return subCategory;
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Update, ex, "Error Updating SubCategory {SubCategoryId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        // DELETE api/<controller>/sub/5
        [HttpDelete("sub/{id:int}")]
        [Authorize(Policy = PolicyNames.EditModule)]
        public async Task<ActionResult> DeleteSubCategory(int id)
        {
            try
            {
                await _categoryService.DeleteSubCategoryAsync(id, AuthEntityId(EntityNames.Module));
                _logger.Log(LogLevel.Information, this, LogFunction.Delete, "SubCategory Deleted {SubCategoryId}", id);
                return StatusCode(StatusCodes.Status200OK);
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Delete, ex, "Error Deleting SubCategory {SubCategoryId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
    }
}
