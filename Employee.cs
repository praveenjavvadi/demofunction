using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Linq;
using System.Collections.Generic;
using System.Net.Http;
using System.Linq;

namespace EmployeeFunction
{
    public static class Employee
    {
        [FunctionName("GetEmployees")]
        public static IActionResult GetEmployees([HttpTrigger(AuthorizationLevel.Anonymous, "Get", Route = "GetEmployees")]HttpRequest req, ILogger log,
            [CosmosDB(databaseName: "Employee", collectionName: "Employee", ConnectionStringSetting = "CosmosDBConnection", SqlQuery = "Select * from Employee")]IEnumerable<EmployeeEntity> EmpEntity)
        {   
            try
            {
               
                if(EmpEntity.Count()>=1)
                {
                    log.LogInformation("Getting Employees");
                    return new OkObjectResult(EmpEntity);
                }
                string str = "{ 'EmployeeEntity': { 'Success': 'True','Data': 'null','Message':'No Data Found'} }";
                dynamic json = JsonConvert.DeserializeObject(str);
                return new OkObjectResult(json);
            }
            catch
            {
                string str = "{ 'EmployeeEntity': { 'Success': 'False','Data': 'null','Message':'Exception'} }";
                dynamic json = JsonConvert.DeserializeObject(str);
                return new OkObjectResult(json);
            }
        }
        [FunctionName("CreateEmployee")]
        public static async Task<IActionResult> CreateEmployee(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "CreateEmployee")]HttpRequest req,
        [CosmosDB(
        databaseName: "Employee",
        collectionName: "Employee",
        ConnectionStringSetting = "CosmosDBConnection")]
        IAsyncCollector<object> todos, ILogger log)
        {
            try { 
            log.LogInformation("Creating a new Employee list item");
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var input = JsonConvert.DeserializeObject<EmployeeEntity>(requestBody);
            if(input.EmployeeName=="")
                {
                    string str = "{  'Success': 'false','Data': 'null','Message':'Employee Name not found' }";
                    dynamic json = JsonConvert.DeserializeObject(str);
                    return new OkObjectResult(json);
                }
            else if(input.EmployeeSalary==0)
                {
                    string str = "{ 'EmployeeEntity': { 'Success': 'false','Data': 'null','Message':'Employee Salary not found'} }";
                    dynamic json = JsonConvert.DeserializeObject(str);
                    return new OkObjectResult(json);
                }
            var Emp = new EmployeeEntity() { EmployeeName = input.EmployeeName,EmployeeSalary=input.EmployeeSalary };
            await todos.AddAsync(new { id = Emp.EmployeeId, Emp.EmployeeName, Emp.EmployeeSalary});
             return new OkObjectResult(Emp);
                }
            catch
            {
                string str = "{ 'EmployeeEntity': { 'Success': 'False','Data': 'null','Message':'Exception'} }";
                dynamic json = JsonConvert.DeserializeObject(str);
                return new OkObjectResult(json);
            }
        }
    }
}
