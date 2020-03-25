using System;
using System.Collections.Generic;
using System.Text;

namespace DepartmentsEmployeesAPI.Models
{
    // C# representation of the Department table
    public class Department
    {
        public int Id { get; set; }
        public string DeptName { get; set; }
        public List<Employee> Employees { get; set; }
    }
}
