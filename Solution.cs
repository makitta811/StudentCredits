using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace Solution
{

    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions options) : base(options)
        { }

        public DbSet<Student> StudentsDB { get; set; }
        public DbSet<Course> CoursesDB { get; set; }
        public DbSet<StudentCoursesXRef> studentCoursesXRefsDB { get; set; }
        public DbSet<Instructor> InstructorsDB { get; set; }
    }

    public class StudentCoursesXRef
    {
        public int StudentPin { get; set; }
        public int CoursePin { get; set; }

        public DateTime ComplitionDate { get; set; }
    }

    public class Instructor
    {
        public int Id { get; set; }
        public string InstructorName { get; set; }
    }



    public class Student
    {
        public int Pin { get; set; }
        public string FirstName { get; set; }
    }
    public class Course
    {
        public int Id { get; set; }
        public int Credit { get; set; }
        public int InstructorId { get; set; }
    }

    public class CourseInstructor
    {
        public Course Course { get; set; }
        public Instructor Instructor { get; set; }
    }

    public class StudentsAndCourses
    {
        public Student Student { get; set; }
       
        public List<CourseInstructor> Courses { get; set; }

      
    }


    public class FinalResults
    {
        public int TotalCredits { get; set; }
        public string StudentName { get; set; }
        public List<CourseInstructor> CourseInstructors { get; set; }
    }


    public class Solution
    {
        private readonly ApplicationDbContext _context;

        public Solution(ApplicationDbContext context)
        {
            _context = context;
        }
        public IEnumerable<IGrouping<Student, StudentsAndCourses>> SelectStudents(int[] studentPins, int minCredit, DateTime startDate, DateTime endDate)
        {

            if (studentPins.Length != 0)
            {
                List<StudentCoursesXRef> selectedCourseXref =  _context.studentCoursesXRefsDB
                    .Where(cours => studentPins.Contains(cours.StudentPin) && cours.ComplitionDate >= startDate && cours.ComplitionDate <= endDate)
                    .ToList();

                var studentsAndCourses = selectedCourseXref.Select(xref =>
                _context.StudentsDB.Where(student => student.Pin == xref.StudentPin).ToList()
                .Select(student => new StudentsAndCourses
                {
                    Student = student,
                    Courses = _context.CoursesDB
                                .Where(course => xref.CoursePin == course.Id && course.Credit >= minCredit)
                                .ToList()
                                .Select(course =>
                                  _context.InstructorsDB
                                    .Where(i => i.Id == course.InstructorId).ToList()
                                    .Select( i =>  new CourseInstructor { Instructor = i, Course = course })).SelectMany(t => t).ToList()
                               
                }

               )).SelectMany(x => x)
                 .GroupBy(s => s.Student);

                return studentsAndCourses;
            }

            else
            {
                    List<StudentCoursesXRef> selectedCourseXref =  _context.studentCoursesXRefsDB
                        .Where(cours =>  cours.ComplitionDate >= startDate && cours.ComplitionDate <= endDate)
                        .ToList();

                    var studentsAndCourses = selectedCourseXref.Select(xref =>
                    _context.StudentsDB.Where(student => student.Pin == xref.StudentPin).ToList()
                    .Select(student => new StudentsAndCourses
                    {
                        Student = student,
                        Courses = _context.CoursesDB
                                .Where(course => xref.CoursePin == course.Id && course.Credit >= minCredit)
                                .ToList()
                                .Select(course =>
                                  _context.InstructorsDB
                                    .Where(i => i.Id == course.InstructorId).ToList()
                                    .Select(i => new CourseInstructor { Instructor = i, Course = course })).SelectMany(t => t).ToList()


                    }

                   )).SelectMany(x => x)
                     .GroupBy(s => s.Student);

               return studentsAndCourses;
            }

            
        }

        public List<FinalResults> GroupStudentCredits(int[] studentPins, int minCredit, DateTime startDate, DateTime endDate)
        {
            return SelectStudents(studentPins, minCredit, startDate, endDate)
                 .SelectMany(x => x)
                 .Select(studetnAndCourses => new FinalResults
                 {
                     TotalCredits = studetnAndCourses.Courses.Select(c => c.Course.Credit).Sum(),
                     StudentName = studetnAndCourses.Student.FirstName,
                     CourseInstructors = studetnAndCourses.Courses
                 }).ToList();
        }
    }
}
