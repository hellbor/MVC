using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ContosoUniversity.Models
{
	public enum Grade { A, B, C, D, E, F }
	public class Enrollment
	{
		public int EnrollmentID { get; set; }
		public int CoutseID { get; set; }
		public int StudentID { get; set; }
		public Grade? Grade { get; set; }

		//Navigation property:
		public Student Student { get; set; }
		public Course Course { get; set; }
	}
}
