using cwiczenie3.DTO;
namespace cwiczenie3.Services
{
    public interface IStudentsDbService
    {
        public string PostStudent(PostStudentRequest student);

        public string PromoteStudents(PromoteStudentRequest request);
        
        public string GetStudent(string index);
        
        public AuthenticationResult Login(LoginRequest request);
        
        public AuthenticationResult Login(string request);
    }
}