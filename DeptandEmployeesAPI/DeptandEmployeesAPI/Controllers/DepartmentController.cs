using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Data;
using Microsoft.Data.SqlClient;
using DepartmentsEmployeesAPI.Models;
using Microsoft.AspNetCore.Http;
using DepartmentsEmployeesAPI.Data;

namespace DeptandEmployeesAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DepartmentController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly DepartmentRepository _repo;

        public DepartmentController(IConfiguration config)
        {
            _config = config;
            _repo = new DepartmentRepository();
        }

        public SqlConnection Connection
        {
            get
            {
                return new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            }
        }


        //// Get all departments from the database
        //[HttpGet]
        //public async Task<IActionResult> Get()
        //{
        //    using (SqlConnection conn = Connection)
        //    {
        //        conn.Open();
        //        using (SqlCommand cmd = conn.CreateCommand())
        //        {
        //            cmd.CommandText = @"SELECT d.Id, d.DeptName, e.FirstName, e.LastName, e.DepartmentId, e.Id as EmployeeId
        //                                FROM Department d
        //                                LEFT JOIN Employee e on d.id = e.DepartmentId";
        //            SqlDataReader reader = cmd.ExecuteReader();
        //            List<Department> departments = new List<Department>();

        //            while (reader.Read())
        //            {
        //                Department department = new Department
        //                {
        //                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
        //                    DeptName = reader.GetString(reader.GetOrdinal("DeptName")),
        //                    Employees = new List<Employee>()
        //                };

        //                departments.Add(department);
        //            }
        //            reader.Close();

        //            return Ok(departments);
        //        }
        //    }
        //}

        // Get all departments from the database
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var departments = _repo.GetAllDepartments(); 
            return Ok(departments);
        }

        [HttpGet("{id}", Name = "GetDepartment")]
        public async Task<IActionResult> Get([FromRoute] int id)
        {
            var department = _repo.GetDepartmentById(id);
            if (department == null) {
                return NotFound();
            }
            else
            {
                return Ok(department);
            }

        }


        //// Get a single department from Id
        //[HttpGet("{id}", Name = "GetDepartment")]
        //public async Task<IActionResult> Get([FromRoute] int id)
        //{
        //    using (SqlConnection conn = Connection)
        //    {
        //        conn.Open();
        //        using (SqlCommand cmd = conn.CreateCommand())
        //        {
        //            cmd.CommandText = @"
        //             SELECT e.FirstName, e.LastName, e.Id as EmployeeId, d.DeptName, d.Id
        //             FROM Department d
        //             LEFT JOIN Employee e on d.Id = e.DepartmentId
        //             WHERE d.id = @id";
        //            cmd.Parameters.Add(new SqlParameter("@id", id));
        //            SqlDataReader reader = cmd.ExecuteReader();

        //            Department department = null;

        //            if (reader.Read())
        //            {
        //                department = new Department
        //                {
        //                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
        //                    DeptName = reader.GetString(reader.GetOrdinal("DeptName"))
        //                };
        //                department.Employees.Add(new Employee()
        //                {
        //                    FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
        //                    LastName = reader.GetString(reader.GetOrdinal("LastName")),
        //                    Id = reader.GetInt32(reader.GetOrdinal("EmployeeId")),
        //                });
        //                reader.Close();

        //                return Ok(department);
        //            }
        //            else
        //            {
        //                return NotFound();
        //            }
        //        }
        //    }
        //}

        //post new Department
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Department department)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"INSERT INTO Department (DeptName)
                                      OUTPUT INSERTED.Id
                                      VALUES (@departmentName)";
                    cmd.Parameters.Add(new SqlParameter("@departmentName", department.DeptName));
                    int newId = (int)cmd.ExecuteScalar();
                    department.Id = newId;
                    return CreatedAtRoute("GetDepartment", new { id = newId }, department);
                }
            }
        }

        //PUT or UPDATE a department
        //[HttpPut("{id}")]
        //public async Task<IActionResult> Put([FromRoute] int id, [FromBody] Department department)
        //{
        //    try
        //    {
        //        var department = _repo.UpdateDepartment(id);
        //    }
        //}

        [HttpPut("{id}")]
        public async Task<IActionResult> Put([FromRoute] int id, [FromBody] Department department)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"UPDATE Department
                                            SET DeptName = @deptName, 
                                            WHERE Id = @id";
                        cmd.Parameters.Add(new SqlParameter("@deptName", department.DeptName));
                        cmd.Parameters.Add(new SqlParameter("@id", id));

                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            return new StatusCodeResult(StatusCodes.Status204NoContent);
                        }
                        throw new Exception("No rows affected");
                    }
                }
            }
            catch (Exception)
            {
                if (!DepartmentExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"DELETE FROM Department WHERE Id = @id";
                        cmd.Parameters.Add(new SqlParameter("@id", id));

                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            return new StatusCodeResult(StatusCodes.Status204NoContent);
                        }
                        throw new Exception("No rows affected");
                    }
                }
            }
            catch (Exception)
            {
                if (!DepartmentExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }
        private bool DepartmentExists(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT Id, DeptName 
                        FROM Department
                        WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    SqlDataReader reader = cmd.ExecuteReader();
                    return reader.Read();
                }
            }
        }
    }
}
