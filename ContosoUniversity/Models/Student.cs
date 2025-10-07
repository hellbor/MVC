using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ContosoUniversity.Models
{
	public class Student
	{
		public int ID { get; set; }
		[Required]
		[DisplayName("Фамилия")]
		[StringLength(24)]
		[RegularExpression(@"^[A-Z]+[a-z]*$")]
		public string LastName { get; set; }

		[Required]
		[DisplayName("Имя")]
		[StringLength(24)]
		[RegularExpression(@"^[A-Z]+[a-z]*$")]
		public string FirstName { get; set; }

		[Required]
		[DisplayName("Дата поступления")]
		//[DataType(DataType.Date)]
		//[DisplayFormat(DataFormatString = "{0:yyyy.MM.dd}", ApplyFormatInEditMode = true)]
		[DataType(DataType.Date)]
		[DisplayFormat(DataFormatString = "{0:yyyy.MM.dd}", ApplyFormatInEditMode = true)]
		public DateTime EnrollmentDate { get; set; }    //Дата поступления

		//Calculated properties:
		[DisplayName("Студент")]
		public string FullName {  get => $"{LastName} {FirstName}"; }

		/////////////////////////////////////////////////////////////////////////////////////

		//Navigation property:
		public ICollection<Enrollment> Enrollments { get; set; }
	}
}
