using AutoMapper;
using Contracts;
using Entities.DataTransferObjects;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;

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
        [HttpGet("{id}")]
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
    }
}
