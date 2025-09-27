using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ContosoUniversity.Models
{
	public class Course
	{
		public int CourseID { get; set; }
		public int Title { get; set; }
		public int Credits { get; set; }

		//Navigation property:
		public ICollection<Enrollment> Enrollments { get; set; }
	}
}
