using AutoMapper;
using CompanyEmployees.ActionFilters;
using Contracts;
using Entities.DataTransferObjects;
using Entities.Models;
using Entities.RequestFeatures;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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
        public async Task<IActionResult> GetEmployeesForCompany(Guid companyId, [FromQuery]EmployeeParameters employeeParameters)
        {
            if(!employeeParameters.ValidAgeRange)
            {
                return BadRequest("Max age can't be less than min age.");
            }
            var company = await repository.Company.GetCompanyAsync(companyId, trackChanges: false);
            if(company == null)
            {
                logger.LogInfo($"Company with id {companyId} doesn't exist in the database.");
                return NotFound();
            }
            else
            {
                var emplFromDb = await repository.Employee.GetEmployeesAsync(companyId, employeeParameters, trackChanges: false);
                Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(emplFromDb.MetaData));
                var emplDto = mapper.Map<IEnumerable<EmployeeDto>>(emplFromDb);
                return Ok(emplDto);
            }
        }
        [HttpGet("{id}", Name = "GetEmployee")]
        public async Task<IActionResult> GetEmployeeForCompany(Guid companyId, Guid id)
        {
            var company = await repository.Company.GetCompanyAsync(companyId, trackChanges: false);
            if (company == null)
            {
                logger.LogInfo($"Company with id {companyId} doesn't exist in the database.");
                return NotFound();
            }
            else
            {
                var empl = await repository.Employee.GetEmployeeAsync(companyId, id, trackChanges: false);
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
        public async Task<IActionResult> CreateEmployeeForCompany(Guid companyId, [FromBody]EmployeeForCreationDto employee)
        {
            if(employee == null)
            {
                logger.LogError("EmployeeForCreationDto object sent from client is null.");
                return BadRequest("EmployeeForCreationDto object is null");
            }

            if(!ModelState.IsValid)
            {
                logger.LogError("Invalid model state for EmployeeForCreationDto object.");
                return UnprocessableEntity(ModelState);
            }

            var company = await repository.Company.GetCompanyAsync(companyId, trackChanges: false);
            if(company == null)
            {
                logger.LogInfo($"Company with id {companyId} doesn't exist in the database.");
                return NotFound();
            }

            var employeeEntity = mapper.Map<Employee>(employee);

            repository.Employee.CreateEmployeeForCompany(companyId, employeeEntity);
            await repository.SaveAsync();

            var employeeToReturn = mapper.Map<EmployeeDto>(employeeEntity);

            return CreatedAtRoute("GetEmployee", new { companyId, id = employeeToReturn.Id }, employeeToReturn);
        }
        [HttpDelete("{id}")]
        [ServiceFilter(typeof(ValidateEmployeeForCompanyExistsAttribute))]
        public async Task<IActionResult> DeleteEmployeeForCompany(Guid companyId, Guid id)
        {
            var employeeForCompany = HttpContext.Items["employee"] as Employee;

            repository.Employee.DeleteEmployee(employeeForCompany);
            await repository.SaveAsync();

            return NoContent();
        }
        [HttpPut("{id}")]
        [ServiceFilter(typeof(ValidateEmployeeForCompanyExistsAttribute))]
        public async Task<IActionResult> UpdateEmployeeForCompany(Guid companyId, Guid id, [FromBody]EmployeeForUpdateDto employee)
        {
            var employeeEntity = HttpContext.Items["employee"] as Employee;
            mapper.Map(employee, employeeEntity);

            await repository.SaveAsync();
            return NoContent();
        }
        [HttpPatch("{id}")]
        [ServiceFilter(typeof(ValidateEmployeeForCompanyExistsAttribute))]
        public async Task<IActionResult> PartiallyUpdateEmployeeForCompany(Guid companyId, Guid id,
            [FromBody] JsonPatchDocument<EmployeeForUpdateDto> patchDoc)
        {
            var employeeEntity = HttpContext.Items["employee"] as Employee;
            var employeeToPatch = mapper.Map<EmployeeForUpdateDto>(employeeEntity);

            patchDoc.ApplyTo(employeeToPatch, ModelState); 
            TryValidateModel(employeeToPatch);

            if (!ModelState.IsValid) 
            { 
                logger.LogError("Invalid model state for the patch document"); 
                return UnprocessableEntity(ModelState); 
            }

            mapper.Map(employeeToPatch, employeeEntity);

            await repository.SaveAsync();

            return NoContent();
        }
    }
}
