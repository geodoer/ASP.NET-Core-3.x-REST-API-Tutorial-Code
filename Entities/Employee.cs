using System;

namespace Routine.Api.Entities
{
    public class Employee
    {
        public Guid Id { get; set; }
        public Guid CompanyId { get; set; }

        public string EmployeeNo { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public Gender Gender { get; set; }
        public DateTime DateOfBirth { get; set; }

        /// <summary>
        /// 关联的导航属性
        /// </summary>
        public Company Company { get; set; }
    }
}