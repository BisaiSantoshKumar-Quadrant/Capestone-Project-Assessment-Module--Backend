namespace QAssessment_project.DTO
{
    public class EmployeeDTO
    {
        public int EmployeeId { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public DateTime JoinedDate { get; set; }
        public int RoleID { get; set; } // Reference to Role
    }

}
