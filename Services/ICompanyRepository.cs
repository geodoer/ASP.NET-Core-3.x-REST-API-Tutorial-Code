using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Routine.Api.DtoParameters;
using Routine.Api.Entities;
using Routine.Api.Helpers;

namespace Routine.Api.Services
{
    /// <summary>
    /// Company的CRUD接口
    /// </summary>
    public interface ICompanyRepository
    {
        //针对公司的CRUD
        Task<PagedList<Company>> GetCompaniesAsync(CompanyDtoParameters parameters);
        Task<Company> GetCompanyAsync(Guid companyId);
        Task<IEnumerable<Company>> GetCompaniesAsync(IEnumerable<Guid> companyIds);
        void AddCompany(Company company);
        void UpdateCompany(Company company);
        void DeleteCompany(Company company);
        Task<bool> CompanyExistsAsync(Guid companyId);

        //针对员工的CRUD
        Task<IEnumerable<Employee>> GetEmployeesAsync(Guid companyId, EmployeeDtoParameters parameters);
        Task<Employee> GetEmployeeAsync(Guid companyId, Guid employeeId);
        void AddEmployee(Guid companyId, Employee employee);
        void UpdateEmployee(Employee employee);
        void DeleteEmployee(Employee employee);

        //保存
        Task<bool> SaveAsync();
    }
}
