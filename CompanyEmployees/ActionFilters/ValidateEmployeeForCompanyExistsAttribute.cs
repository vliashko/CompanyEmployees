using Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Threading.Tasks;

namespace CompanyEmployees.ActionFilters
{
    public class ValidateEmployeeForCompanyExistsAttribute : IAsyncActionFilter
    {
        private readonly IRepositoryManager repository;
        private readonly ILoggerManager logger;
        public ValidateEmployeeForCompanyExistsAttribute(IRepositoryManager repository, ILoggerManager logger)
        {
            this.repository = repository;
            this.logger = logger;
        }
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var method = context.HttpContext.Request.Method;
            var trackChanges = method.Equals("PUT") || method.Equals("PATCH");

            var companyId = (Guid)context.ActionArguments["companyId"];
            var company = await repository.Company.GetCompanyAsync(companyId, trackChanges);

            if(company == null)
            {
                logger.LogInfo($"Company with id: {companyId} doesn't exist in the database."); 
                context.Result = new NotFoundResult(); 
                return;
            }

            var id = (Guid)context.ActionArguments["id"];
            var employee = await repository.Employee.GetEmployeeAsync(companyId, id, trackChanges);

            if(employee == null)
            {
                logger.LogInfo($"Employee with id: {id} doesn't exist in the database."); 
                context.Result = new NotFoundResult();
            }
            else
            {
                context.HttpContext.Items.Add("employee", employee);
                await next();
            }
        }
    }
}
