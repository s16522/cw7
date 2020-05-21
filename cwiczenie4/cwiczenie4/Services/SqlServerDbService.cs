using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Security.Claims;
using cwiczenie3.DTO;
using cwiczenie3.Handler;
using cwiczenie3.Models;

namespace cwiczenie3.Services
{
    public class SqlServerDbService : IStudentsDbService
    {
        private static string _connStr = $@"
            Server=127.0.0.1,1433;
            Database=Master;
            User Id=SA;
            Password=<YourNewStrong@Passw0rd>
            ";

        public List<Student> GetStudents()
        {
            List<Student> _students = new List<Student>();
            using (var client = new SqlConnection(_connStr))
            {
                using (var query = new SqlCommand())
                {
                    query.Connection = client;
                    query.CommandText =
                        "Select FirstName, LastName, BirthDate, Semester, Name from Student JOIN Enrollment ON (Student.IdEnrollment = Enrollment.IdEnrollment) JOIN Studies ON (Enrollment.IdStudy = Studies.IdStudy)";

                    client.Open();
                    var dr = query.ExecuteReader();
                    while (dr.Read())
                    {
                        var st = new Student();
                        st.FirstName = dr["FirstName"].ToString();
                        st.LastName = dr["LastName"].ToString();
                        st.BirthDate = DateTime.Parse(dr["BirthDate"].ToString());
                        //st.Name = dr["Name"].ToString();
                        st.Semester = Int32.Parse(dr["Semester"].ToString());
                        _students.Add(st);
                    }
                }
            }

            return _students;
        }

        public string PostStudent(PostStudentRequest student)
        {
            Enrollment enrollment = new Enrollment();

            using (var connection = new SqlConnection(_connStr))
            using (var query = new SqlCommand())
            {
                query.Connection = connection;
                connection.Open();

                var transaction = connection.BeginTransaction();

                try
                {
                    query.CommandText = "SELECT IdStudy FROM Studies where Name=@name";
                    query.Parameters.AddWithValue("name", student.Studies);
                    query.Transaction = transaction;

                    var studs = query.ExecuteReader();
                    if (!studs.Read())
                    {
                        return "Not Found";
                    }

                    int idstudies = (int) studs["IdStudy"];
                    studs.Close();

                    query.CommandText =
                        "SELECT IdEnrollment FROM Enrollment WHERE IdEnrollment >= (SELECT MAX(IdEnrollment) FROM Enrollment)";
                    var idenr = query.ExecuteReader();
                    
                    int idEnroll = (int) idenr["IdEnrollment"] + 10;
                    idenr.Close();

                    query.CommandText =
                        "SELECT idEnrollment, StartDate from Enrollment WHERE idStudy=@idStudy AND Semester=1 ORDER BY StartDate";
                    query.Parameters.AddWithValue("idStudy", idstudies);

                    DateTime enrollDate;

                    var enrol = query.ExecuteReader();
                    if (!enrol.Read())
                    {
                        enrollDate = DateTime.Now;
                        query.CommandText = "INSERT INTO Enrollment VALUES(@id, @Semester, @IdStud, @StartDate)";
                        query.Parameters.AddWithValue("id", idEnroll);
                        query.Parameters.AddWithValue("Semester", 1);
                        query.Parameters.AddWithValue("IdStud", idstudies);
                        query.Parameters.AddWithValue("StartDate", enrollDate);
                        enrol.Close();
                        query.ExecuteNonQuery();
                    }
                    else
                    {
                        idEnroll = (int) enrol["IdEnrollment"];
                        enrollDate = (DateTime) enrol["StartDate"];
                        enrol.Close();
                    }

                    enrollment.IdEnrollment = idEnroll;
                    enrollment.Semester = 1;
                    enrollment.IdStudy = idstudies;
                    enrollment.StartDate = enrollDate;

                    query.CommandText = "SELECT IndexNumber FROM Student WHERE IndexNumber=@indexNum";
                    query.Parameters.AddWithValue("indexNum", student.IndexNumber);

                    DateTime bDate = Convert.ToDateTime(student.BirthDate);
                    string formattedDate = bDate.ToString("yyyy-MM-dd HH:mm:ss.fff");

                    try
                    {
                        query.CommandText =
                            "INSERT INTO Student(IndexNumber, FirstName, LastName, BirthDate, IdEnrollment) VALUES (@index, @fName, @lName, @birthDate, @idEnrollment)";
                        query.Parameters.AddWithValue("index", student.IndexNumber);
                        query.Parameters.AddWithValue("fName", student.FirstName);
                        query.Parameters.AddWithValue("lName", student.LastName);
                        query.Parameters.AddWithValue("birthDate", formattedDate);
                        query.Parameters.AddWithValue("idEnrollment", idEnroll);

                        query.ExecuteNonQuery();

                        transaction.Commit();
                    }
                    catch (SqlException ex)
                    {
                        transaction.Rollback();
                        return "BadRequest";
                    }
                }
                catch (SqlException exc)
                {
                    transaction.Rollback();
                    return "BadRequest";
                }

                return enrollment.ToString();
            }
        }
        
        public string PromoteStudents(PromoteStudentRequest request)
        {
            using (var connection = new SqlConnection(_connStr))
            
            {
             
                SqlCommand command = new SqlCommand();
                command.Connection = connection;
                connection.Open();

                int newSem = request.Semester + 1;

                command.CommandText = "SELECT IdStudy FROM Studies where Name=@name";
                command.Parameters.AddWithValue("name", request.Studies);

                var studs = command.ExecuteReader();
                if (!studs.Read())
                {
                      
                      return "Not found";
                }
                int idstudies = (int)studs["IdStudy"];
                studs.Close();

                Enrollment enrollment = new Enrollment();

                command.CommandText = "SELECT idEnrollment,idStudy,Semester,StartDate FROM Enrollment WHERE idStudy=@idStudy AND Semester=@Semester";
                command.Parameters.AddWithValue("idStudy", idstudies);
                command.Parameters.AddWithValue("Semester", newSem);

                var enr = command.ExecuteReader();
                if (enr.Read())
                {
                    enrollment.IdEnrollment = (int)enr["IdEnrollment"];
                    enrollment.IdStudy = (int)enr["IdStudy"];
                    enrollment.Semester = (int)enr["Semester"];
                    enrollment.StartDate = (DateTime)enr["StartDate"];
                }
                enr.Close();
                

                return enrollment.ToString();
            }
        }
        public string GetStudent(string index)
        {
            using (var connection = new SqlConnection(_connStr))
            using (var query = new SqlCommand())
            {
                query.Connection = connection;

                query.CommandText = "SELECT IndexNumber FROM Student WHERE IndexNumber=@index";
                query.Parameters.AddWithValue("index", index);
                connection.Open();

                var dr = query.ExecuteReader();
                if (dr.Read())
                {
                    return index;
                }
                return null;
            } 
        }
        
        public AuthenticationResult Login(LoginRequest request)
        {
            var salt = "";
            using (var connection = new SqlConnection(_connStr))
            using (var query = new SqlCommand())
            {
                query.Connection = connection;
                connection.Open();
                query.CommandText = "select Salt from Student where IndexNumber = @index";
                query.Parameters.AddWithValue("index", request.Login);

                using (var dataReader = query.ExecuteReader())
                {
                    if (!dataReader.Read())
                    {
                        return null;
                    }
                    salt = dataReader["Salt"].ToString();
                }
                query.CommandText = "select Role from Student where IndexNumber = @index and Password = @password;";
                query.Parameters.AddWithValue("password", PasswordHandler.CreateHash(request.Password, salt));
                return Authenticate(query);
            }

        }
        
        public AuthenticationResult Login(string token)
        {
            using (var connection = new SqlConnection(_connStr))
            using (var query = new SqlCommand())
            {
                query.Connection = connection;
                connection.Open();
                query.CommandText = "select IndexNumber, Role from Student s left join RefreshToken r on  r.StudentID = s.IndexNumber where Token=@token and ValidTo > GETDATE();";
                query.Parameters.AddWithValue("token", token);

                return Authenticate(query);
            }
        }

        private AuthenticationResult Authenticate(SqlCommand query)
        {
            var result = new AuthenticationResult();
            using (var dataReader = query.ExecuteReader())
            {
                if (!dataReader.Read())
                {
                   return null;
                }

                if (!query.Parameters.Contains("index"))
                {
                    query.Parameters.AddWithValue("index", dataReader["IndexNumber"].ToString());
                }

                result.Claims = new[]
                {
                    new Claim(ClaimTypes.Name, query.Parameters["index"].Value.ToString()),
                    new Claim(ClaimTypes.Role, dataReader["Role"].ToString())
                };
            }
            result.RefreshToken = Guid.NewGuid().ToString();
            AddToken(query, result.RefreshToken);
            return result;
        }

        
        private void AddToken(SqlCommand query, string token)
        {
            query.CommandText =
                "insert into RefreshToken(Token, StudentId, ValidTo) values (@newToken, @index, @validTo);";
            query.Parameters.AddWithValue("newToken", token);
            query.Parameters.AddWithValue("validTo", DateTime.Now.AddDays(1));
            query.ExecuteNonQuery();
        }

    }
}