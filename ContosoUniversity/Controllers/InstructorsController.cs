using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ContosoUniversity.Data;
using ContosoUniversity.Models;

using ContosoUniversity.Models.ViewModels;

namespace ContosoUniversity.Controllers
{
	public class InstructorsController : Controller
	{
		private readonly UniversityContext _context;

		public InstructorsController(UniversityContext context)
		{
			_context = context;
		}

		// GET: Instructors
		public async Task<IActionResult> Index(int? id, int? courseID)
		{
			InstructorIndexData viewModel = new InstructorIndexData();

			viewModel.Instructors =
				await _context.Instructors
				.Include(i => i.OfficeAssignment)
				.Include(i => i.CourseAssignments)
					.ThenInclude(i => i.Course)
						.ThenInclude(i => i.Enrollments)
							.ThenInclude(i => i.Student)
				.Include(i => i.CourseAssignments)
					.ThenInclude(i => i.Course)
						.ThenInclude(i => i.Department)
				.AsNoTracking()
				.OrderBy(i => i.LastName)
				.ToListAsync();

			if (id != null)
			{
				ViewData["InstructorID"] = id.Value;
				Instructor instructor = viewModel.Instructors.Where(i => i.ID == id.Value).Single();
				viewModel.Courses = instructor.CourseAssignments.Select(s => s.Course);
			}

			if (courseID != null)
			{
				ViewData["CourseID"] = courseID.Value;
				//viewModel.Enrollments = viewModel.Courses.Where(x => x.CourseID == courseID).Single().Enrollments;
				Course selectedCourse = viewModel.Courses.Where(x => x.CourseID == courseID).Single();
				await _context.Entry(selectedCourse).Collection(x => x.Enrollments).LoadAsync();
				foreach (Enrollment enrollment in selectedCourse.Enrollments)
				{
					await _context.Entry(enrollment).Reference(x => x.Student).LoadAsync();
				}
				viewModel.Enrollments = selectedCourse.Enrollments;
			}

			return View(viewModel);
			//return View(await _context.Instructors.ToListAsync());
		}

		// GET: Instructors/Details/5
		public async Task<IActionResult> Details(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var instructor = await _context.Instructors
				.FirstOrDefaultAsync(m => m.ID == id);
			if (instructor == null)
			{
				return NotFound();
			}

			return View(instructor);
		}

		// GET: Instructors/Create
		public IActionResult Create()
		{
			var instructor = new Instructor();
			instructor.CourseAssignments = new List<CourseAssignment>();
			PopulateAssignedCourseData(instructor);
			return View();
		}

		// POST: Instructors/Create
		// To protect from overposting attacks, enable the specific properties you want to bind to.
		// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create([Bind("ID,LastName,FirstName,HireDate,OfficeAssignment")] Instructor instructor, string[] selectedCourses)
		{
			if (selectedCourses != null)
			{
				instructor.CourseAssignments = new List<CourseAssignment>();
				foreach (var course in selectedCourses)
				{
					var courseToAdd = new CourseAssignment { InstructorID = instructor.ID, CourseID = int.Parse(course) };
					instructor.CourseAssignments.Add(courseToAdd);
				}
			}
			if (ModelState.IsValid)
			{
				_context.Add(instructor);
				await _context.SaveChangesAsync();
				return RedirectToAction(nameof(Index));
			}
			PopulateAssignedCourseData(instructor);
			return View(instructor);
		}

		// GET: Instructors/Edit/5
		public async Task<IActionResult> Edit(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			//var instructor = await _context.Instructors.FindAsync(id);

			Instructor instructor = await _context.Instructors
				.Include(i => i.OfficeAssignment)
				.Include(i => i.CourseAssignments).ThenInclude(i => i.Course)
				.AsNoTracking()
				.FirstOrDefaultAsync(m => m.ID == id);

			//var instructor = await _context.Instructors
			//	.Include(i => i.OfficeAssignment)
			//	.AsNoTracking()
			//	.FirstOrDefaultAsync(m => m.ID == id);

			if (instructor == null)
			{
				return NotFound();
			}
			PopulateAssignedCourseData(instructor);
			return View(instructor);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(int? id, string[] selectedCourses)
		{
			if (id == null)
			{
				return NotFound();
			}

			var instructorToUpdate = await _context.Instructors
				.Include(i => i.OfficeAssignment)
				.Include(i => i.CourseAssignments)
					.ThenInclude(i => i.Course)
				.FirstOrDefaultAsync(m => m.ID == id);

			if (await TryUpdateModelAsync<Instructor>(
				instructorToUpdate,
				"",
				i => i.FirstMidName, i => i.LastName, i => i.HireDate, i => i.OfficeAssignment))
			{
				if (String.IsNullOrWhiteSpace(instructorToUpdate.OfficeAssignment?.Location))
				{
					instructorToUpdate.OfficeAssignment = null;
				}
				UpdateInstructorCourses(selectedCourses, instructorToUpdate);
				try
				{
					await _context.SaveChangesAsync();
				}
				catch (DbUpdateException /* ex */)
				{
					//Log the error (uncomment ex variable name and write a log.)
					ModelState.AddModelError("", "Unable to save changes. " +
						"Try again, and if the problem persists, " +
						"see your system administrator.");
				}
				return RedirectToAction(nameof(Index));
			}
			UpdateInstructorCourses(selectedCourses, instructorToUpdate);
			PopulateAssignedCourseData(instructorToUpdate);
			return View(instructorToUpdate);
		}

		void PopulateAssignedCourseData(Instructor instructor)
		{
			DbSet<Course> allCourses = _context.Courses;
			HashSet<int> instructorCourses =
				new HashSet<int>(instructor.CourseAssignments.Select(c => c.CourseID));
			List<AssignedCourseData> viewModel = new List<AssignedCourseData>();
			foreach (Course course in allCourses)
			{
				viewModel.
					Add(new AssignedCourseData
					{
						CourseID = course.CourseID,
						Title = course.Title,
						Assigned = instructorCourses.Contains(course.CourseID)
					});
			}
			ViewData["Courses"] = viewModel;
		}

		// POST: Instructors/Edit/5
		//[HttpPost, ActionName("Edit")]
		//[ValidateAntiForgeryToken]
		//public async Task<IActionResult> EditPost(int? id, string[] selectedCourses)
		//{
		//	if (id == null) return NotFound();

		//	Instructor instructor = await _context.Instructors
		//		.Include(i => i.OfficeAssignment)
		//		.Include(i => i.CourseAssignments)
		//			.ThenInclude(i => i.Course)
		//		.FirstOrDefaultAsync(m => m.ID == id);

		//	if (await TryUpdateModelAsync<Instructor>(instructor, "", i => i.FirstName, i => i.LastName, i => i.HireDate, i => i.OfficeAssignment))
		//	{
		//		if (string.IsNullOrWhiteSpace(instructor.OfficeAssignment?.Location)) instructor.OfficeAssignment = null;
		//		UpdateInstructorCourses(selectedCourses, instructor);
		//		try
		//		{
		//			await _context.SaveChangesAsync();
		//		}
		//		catch (DbUpdateException ex)
		//		{
		//			ModelState.AddModelError("Error", "Unable to save changes");
		//		}
		//		return RedirectToAction(nameof(Index));
		//	}
		//	UpdateInstructorCourses(selectedCourses, instructor);
		//	PopulateAssignedCourseData(instructor);
		//	return View(instructor);
		//}

		[HttpPost, ActionName("Edit")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> EditPost(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var instructorToUpdate = await _context.Instructors
				.Include(i => i.OfficeAssignment)
				.FirstOrDefaultAsync(s => s.ID == id);

			if (await TryUpdateModelAsync<Instructor>(
				instructorToUpdate,
				"",
				i => i.FirstMidName, i => i.LastName, i => i.HireDate, i => i.OfficeAssignment))
			{
				if (String.IsNullOrWhiteSpace(instructorToUpdate.OfficeAssignment?.Location))
				{
					instructorToUpdate.OfficeAssignment = null;
				}
				try
				{
					await _context.SaveChangesAsync();
				}
				catch (DbUpdateException /* ex */)
				{
					//Log the error (uncomment ex variable name and write a log.)
					ModelState.AddModelError("", "Unable to save changes. " +
						"Try again, and if the problem persists, " +
						"see your system administrator.");
				}
				return RedirectToAction(nameof(Index));
			}
			return View(instructorToUpdate);
		}

		private void UpdateInstructorCourses(string[] selectedCourses, Instructor instructorToUpdate)
		{
			if (selectedCourses == null)
			{
				instructorToUpdate.CourseAssignments = new List<CourseAssignment>();
				return;
			}

			var selectedCoursesHS = new HashSet<string>(selectedCourses);
			var instructorCourses = new HashSet<int>
				(instructorToUpdate.CourseAssignments.Select(c => c.Course.CourseID));
			foreach (var course in _context.Courses)
			{
				if (selectedCoursesHS.Contains(course.CourseID.ToString()))
				{
					if (!instructorCourses.Contains(course.CourseID))
					{
						instructorToUpdate.CourseAssignments.Add(new CourseAssignment { InstructorID = instructorToUpdate.ID, CourseID = course.CourseID });
					}
				}
				else
				{

					if (instructorCourses.Contains(course.CourseID))
					{
						CourseAssignment courseToRemove = instructorToUpdate.CourseAssignments.FirstOrDefault(i => i.CourseID == course.CourseID);
						_context.Remove(courseToRemove);
					}
				}
			}
		}

		//void UpdateInstructorCourses(string[] selectedCourses, Instructor instructor)
		//{
		//	if (selectedCourses == null)
		//	{
		//		instructor.CourseAssignments = new List<CourseAssignment>();
		//		return;
		//	}

		//	HashSet<string> selectedCoursesHS = new HashSet<string>(selectedCourses);
		//	HashSet<int> instructorCourses =
		//		new HashSet<int>(instructor.CourseAssignments.Select(c => c.CourseID));
		//	foreach (Course course in _context.Courses)
		//	{
		//		if (selectedCoursesHS.Contains(course.CourseID.ToString()))
		//		{
		//			if (!instructorCourses.Contains(course.CourseID))
		//				instructor.CourseAssignments.Add
		//				(
		//					new CourseAssignment
		//					{
		//						InstructorID = instructor.ID,
		//						CourseID = course.CourseID,
		//					}
		//				);
		//		}
		//		else
		//		{
		//			if(instructorCourses.Contains(course.CourseID))
		//			{
		//				CourseAssignment erased = instructor
		//					.CourseAssignments
		//					.FirstOrDefault(i => i.CourseID == course.CourseID);
		//				_context.Remove(erased);
		//			}
		//		}
		//	}
		//}
		/*[HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,LastName,FirstName,HireDate")] Instructor instructor)
        {
            if (id != instructor.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(instructor);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!InstructorExists(instructor.ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(instructor);
        }*/

		// GET: Instructors/Delete/5
		public async Task<IActionResult> Delete(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var instructor = await _context.Instructors
				.FirstOrDefaultAsync(m => m.ID == id);
			if (instructor == null)
			{
				return NotFound();
			}

			return View(instructor);
		}

		// POST: Instructors/Delete/5
		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		//public async Task<IActionResult> DeleteConfirmed(int id)
		//{
		//	var instructor = await _context.Instructors.FindAsync(id);
		//	if (instructor != null)
		//	{
		//		_context.Instructors.Remove(instructor);
		//	}

		//	await _context.SaveChangesAsync();
		//	return RedirectToAction(nameof(Index));
		//}

		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> DeleteConfirmed(int id)
		{
			Instructor instructor = await _context.Instructors
				.Include(i => i.CourseAssignments)
				.SingleAsync(i => i.ID == id);

			var departments = await _context.Departments
				.Where(d => d.InstructorID == id)
				.ToListAsync();
			departments.ForEach(d => d.InstructorID = null);

			_context.Instructors.Remove(instructor);

			await _context.SaveChangesAsync();
			return RedirectToAction(nameof(Index));
		}

		private bool InstructorExists(int id)
		{
			return _context.Instructors.Any(e => e.ID == id);
		}
	}
}
