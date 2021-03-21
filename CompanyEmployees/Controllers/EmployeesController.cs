using AutoMapper;
using Contracts;
using Entities.DataTransferObjects;
using Entities.Models;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;

namespace CompanyEmployees.Controllers
{
    [Route("api/companies/{companyId}/employees")]
    [ApiController]
    public class EmployeesController : ControllerBase
    {
        private readonly IRepositoryManager repository;
        private readonly ILoggerManager logger;
        private readonly IMapper mapper;

        public EmployeesController(IRepositoryManager repository, ILoggerManager logger, IMapper mapper)
        {
            this.repository = repository;
            this.logger = logger;
            this.mapper = mapper;
        }
        [HttpGet]
        public IActionResult GetEmployeesForCompany(Guid companyId)
        {
            var company = repository.Company.GetCompany(companyId, trackChanges: false);
            if(company == null)
            {
                logger.LogInfo($"Company with id {companyId} doesn't exist in the database.");
                return NotFound();
            }
            else
            {
                var emplFromDb = repository.Employee.GetEmployees(companyId, trackChanges: false);
                var emplDto = mapper.Map<IEnumerable<EmployeeDto>>(emplFromDb);
                return Ok(emplDto);
            }
        }
        [HttpGet("{id}", Name = "GetEmployee")]
        public IActionResult GetEmployeeForCompany(Guid companyId, Guid id)
        {
            var company = repository.Company.GetCompany(companyId, trackChanges: false);
            if (company == null)
            {
                logger.LogInfo($"Company with id {companyId} doesn't exist in the database.");
                return NotFound();
            }
            else
            {
                var empl = repository.Employee.GetEmployee(companyId, id, trackChanges: false);
                if(empl == null)
                {
                    logger.LogInfo($"Employee with id: {id} doesn't exist in the database."); 
                    return NotFound();
                }
                else
                {
                    var employee = mapper.Map<EmployeeDto>(empl);
                    return Ok(employee);
                }  
            }
        }
        [HttpPost]
        public IActionResult  CreateEmployeeForCompany(Guid companyId, [FromBody]EmployeeForCreationDto employee)
        {
            if(employee == null)
            {
                logger.LogError("EmployeeForCreationDto object sent from client is null.");
                return BadRequest("EmployeeForCreationDto object is null");
            }

            var company = repository.Company.GetCompany(companyId, trackChanges: false);
            if(company == null)
            {
                logger.LogInfo($"Company with id {companyId} doesn't exist in the database.");
                return NotFound();
            }

            var employeeEntity = mapper.Map<Employee>(employee);

            repository.Employee.CreateEmployeeForCompany(companyId, employeeEntity);
            repository.Save();

            var employeeToReturn = mapper.Map<EmployeeDto>(employeeEntity);

            return CreatedAtRoute("GetEmployee", new { companyId, id = employeeToReturn.Id }, employeeToReturn);
        }
        [HttpDelete("{id}")]
        public IActionResult DeleteEmployeeForCompany(Guid companyId, Guid id)
        {
            var company = repository.Company.GetCompany(companyId, trackChanges: false);
            if(company == null)
            {
                logger.LogInfo($"Company with id {companyId} doesn't exist in the database.");
                return NotFound();
            }

            var employeeForCompany = repository.Employee.GetEmployee(companyId, id, trackChanges: false);

            if(employeeForCompany == null)
            {
                logger.LogInfo($"Employee with id: {id} doesn't exist in the database.");
                return NotFound();
            }

            repository.Employee.DeleteEmployee(employeeForCompany);
            repository.Save();

            return NoContent();
        }
        [HttpPut("{id}")]
        public IActionResult UpdateEmployeeForCompany(Guid companyId, Guid id, [FromBody]EmployeeForUpdateDto employee)
        {
            if(employee == null)
            {
                logger.LogError("EmployeeForUpdateDto object sent from client is null.");
                return BadRequest("EmployeeForUpdateDto object is null");
            }

            var company = repository.Company.GetCompany(companyId, trackChanges: false);
            if(company == null)
            {
                logger.LogInfo($"Company with id {companyId} doesn't exist in the database.");
                return NotFound();
            }
            var employeeEntity = repository.Employee.GetEmployee(companyId, id, trackChanges: true);
            if (employeeEntity == null)
            {
                logger.LogInfo($"Employee with id {id} doesn't exist in the database.");
                return NotFound();
            }
            mapper.Map(employee, employeeEntity);

            repository.Save();
            return NoContent();
        }
        [HttpPatch("{id}")]
        public IActionResult PartiallyUpdateEmployeeForCompany(Guid companyId, Guid id,
            [FromBody] JsonPatchDocument<EmployeeForUpdateDto> patchDoc)
        {
            if (patchDoc == null)
            {
                logger.LogError("patchDoc object sent from client is null.");
                return BadRequest("patchDoc object is null");
            }

            var company = repository.Company.GetCompany(companyId, trackChanges: false);
            if (company == null)
            {
                logger.LogInfo($"Company with id: {companyId} doesn't exist in the database");
                return NotFound();
            }

            var employeeEntity = repository.Employee.GetEmployee(companyId, id, trackChanges: true);
            if(employeeEntity == null)
            {
                logger.LogInfo($"Employee with id {id} doesn't exist in the database.");
                return NotFound();
            }
            var employeeToPatch = mapper.Map<EmployeeForUpdateDto>(employeeEntity);

            patchDoc.ApplyTo(employeeToPatch);

            mapper.Map(employeeToPatch, employeeEntity);

            repository.Save();

            return NoContent();
        }
    }
}
