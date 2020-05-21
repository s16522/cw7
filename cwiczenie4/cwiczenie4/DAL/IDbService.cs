using System.Collections;
using System.Collections.Generic;
using cwiczenie3.Models;

namespace cwiczenie3.DAL
{
    public interface IDbService
    {
        public IEnumerable<Student> GetStudents();
    }
}