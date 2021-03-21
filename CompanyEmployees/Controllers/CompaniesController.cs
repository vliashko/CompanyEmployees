using AutoMapper;
using CompanyEmployees.ActionFilters;
using CompanyEmployees.ModelBinders;
using Contracts;
using Entities.DataTransferObjects;
using Entities.Models;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CompanyEmployees.Controllers
{
    [Route("api/companies")]
    [ApiController]
    public class CompaniesController : ControllerBase
    {
        private readonly IRepositoryManager repository;
        private readonly ILoggerManager logger;
        private readonly IMapper mapper;

        public CompaniesController(IRepositoryManager repository, ILoggerManager logger, IMapper mapper)
        {
            this.repository = repository;
            this.logger = logger;
            this.mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetCompanies()
        {
            var companies = await repository.Company.GetAllCompaniesAsync(trackChanges: false);
            var companiesDto = mapper.Map<IEnumerable<CompanyDto>>(companies);

            return Ok(companiesDto);
        }
        [HttpGet("{id}", Name = "CompanyById")]
        public async Task<IActionResult> GetCompany(Guid id)
        {
            var company = await repository.Company.GetCompanyAsync(id, trackChanges: false);
            if(company == null)
            {
                logger.LogInfo($"Company with id: {id} doesn't exist in the database");
                return NotFound();
            }
            else
            {
                var companyDto = mapper.Map<CompanyDto>(company);
                return Ok(companyDto);
            }
        }
        [HttpPost]
        [ServiceFilter(typeof(ValidationFilterAttribute))]
        public async Task<IActionResult> CreateCompany([FromBody]CompanyForCreationDto company)
        {
            var companyEntity = mapper.Map<Company>(company);
            repository.Company.CreateCompany(companyEntity);
            await repository.SaveAsync();

            var companyToReturn = mapper.Map<CompanyDto>(companyEntity);

            return CreatedAtRoute("CompanyById", new { id = companyToReturn.Id }, companyToReturn);
        }
        [HttpGet("collection/({ids})", Name = "CompanyCollection")]
        public async Task<IActionResult> GetCompanyCollection([ModelBinder(BinderType = typeof(ArrayModelBinder))]IEnumerable<Guid> ids)
        {
            if(ids == null)
            {
                logger.LogError("Parameter ids is null.");
                return BadRequest("Parameter ids is null");
            }

            var companyEntities = await repository.Company.GetByIdsAsync(ids, trackChanges: false);

            if(ids.Count() != companyEntities.Count())
            {
                logger.LogError("Some ids are not valid in a collection.");
                return NotFound();
            }

            var companiesToReturn = mapper.Map<IEnumerable<CompanyDto>>(companyEntities);

            return Ok(companiesToReturn);
        }
        [HttpPost("collection")]
        public async Task<IActionResult> CreateCompanyCollection([FromBody]IEnumerable<CompanyForCreationDto> companyCollection)
        {
            if(companyCollection == null)
            {
                logger.LogError("Company collection sent from client is null.");
                return BadRequest("Company collection is null");
            }
            if (!ModelState.IsValid)
            {
                logger.LogError("Invalid model state for the CompanyForCreationDto collection");
                return UnprocessableEntity(ModelState);
            }
            var companyEntities = mapper.Map<IEnumerable<Company>>(companyCollection);
            foreach (var company in companyEntities)
            {
                repository.Company.CreateCompany(company);
            }
            await repository.SaveAsync();

            var companyCollectionToReturn = mapper.Map<IEnumerable<CompanyDto>>(companyEntities);
            var ids = string.Join(",", companyCollectionToReturn.Select(c => c.Id));

            return CreatedAtRoute("CompanyCollection", new { ids }, companyCollectionToReturn);
        }
        [HttpDelete("{id}")]
        [ServiceFilter(typeof(ValidateCompanyExistsAttribute))]
        public async Task<IActionResult> DeleteCompany(Guid id)
        {
            var company = HttpContext.Items["company"] as Company;

            repository.Company.DeleteCompany(company);
            await repository.SaveAsync();

            return NoContent();
        }
        [HttpPut("{id}")]
        [ServiceFilter(typeof(ValidationFilterAttribute))]
        [ServiceFilter(typeof(ValidateCompanyExistsAttribute))]
        public async Task<IActionResult> UpdateCompany(Guid id, [FromBody]CompanyForUpdateDto company)
        {
            var companyEntity = HttpContext.Items["company"] as Company;

            mapper.Map(company, companyEntity);
            await repository.SaveAsync();

            return NoContent();
        }
    }
}
