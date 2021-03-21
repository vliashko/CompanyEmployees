using AutoMapper;
using CompanyEmployees.ModelBinders;
using Contracts;
using Entities.DataTransferObjects;
using Entities.Models;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

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
        public IActionResult GetCompanies()
        {
            var companies = repository.Company.GetAllCompanies(trackChanges: false);
            var companiesDto = mapper.Map<IEnumerable<CompanyDto>>(companies);

            return Ok(companiesDto);
        }
        [HttpGet("{id}", Name = "CompanyById")]
        public IActionResult GetCompany(Guid id)
        {
            var company = repository.Company.GetCompany(id, trackChanges: false);
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
        public IActionResult CreateCompany([FromBody]CompanyForCreationDto company)
        {
            if(company == null)
            {
                logger.LogError("CompanyForCreationDto object sent from client  null.");
                return BadRequest("CompanyForCreationDto object is null");
            }
            var companyEntity = mapper.Map<Company>(company);
            repository.Company.CreateCompany(companyEntity);
            repository.Save();

            var companyToReturn = mapper.Map<CompanyDto>(companyEntity);

            return CreatedAtRoute("CompanyById", new { id = companyToReturn.Id }, companyToReturn);
        }
        [HttpGet("collection/({ids})", Name = "CompanyCollection")]
        public IActionResult GetCompanyCollection([ModelBinder(BinderType = typeof(ArrayModelBinder))]IEnumerable<Guid> ids)
        {
            if(ids == null)
            {
                logger.LogError("Parameter ids is null.");
                return BadRequest("Parameter ids is null");
            }

            var companyEntities = repository.Company.GetByIds(ids, trackChanges: false);

            if(ids.Count() != companyEntities.Count())
            {
                logger.LogError("Some ids are not valid in a collection.");
                return NotFound();
            }

            var companiesToReturn = mapper.Map<IEnumerable<CompanyDto>>(companyEntities);

            return Ok(companiesToReturn);
        }
        [HttpPost("collection")]
        public IActionResult CreateCompanyCollection([FromBody]IEnumerable<CompanyForCreationDto> companyCollection)
        {
            if(companyCollection == null)
            {
                logger.LogError("Company collection sent from client is null.");
                return BadRequest("Company collection is null");
            }

            var companyEntities = mapper.Map<IEnumerable<Company>>(companyCollection);
            foreach (var company in companyEntities)
            {
                repository.Company.CreateCompany(company);
            }
            repository.Save();

            var companyCollectionToReturn = mapper.Map<IEnumerable<CompanyDto>>(companyEntities);
            var ids = string.Join(",", companyCollectionToReturn.Select(c => c.Id));

            return CreatedAtRoute("CompanyCollection", new { ids }, companyCollectionToReturn);
        }
        [HttpDelete("{id}")]
        public IActionResult DeleteCompany(Guid id)
        {
            var company = repository.Company.GetCompany(id, trackChanges: false);
            if(company == null)
            {
                logger.LogInfo($"Company with id: {id} doesn't exist in the database");
                return NotFound();
            }

            repository.Company.DeleteCompany(company);
            repository.Save();

            return NoContent();
        }
        [HttpPut("{id}")]
        public IActionResult UpdateCompany(Guid id, [FromBody]CompanyForUpdateDto company)
        {
            if(company == null)
            {
                logger.LogError("CompanyForUpdateDto object sent from client is null.");
                return BadRequest("CompanyForUpdateDto object is null");
            }

            var companyEntity = repository.Company.GetCompany(id, trackChanges: true);

            if(companyEntity == null)
            {
                logger.LogInfo($"Company with id: {id} doesn't exist in the database");
                return NotFound();
            }

            mapper.Map(company, companyEntity);
            repository.Save();

            return NoContent();
        }
    }
}
