using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.RegularExpressions;
using BITCollege_EU.Data;
using System.Data.SqlClient;
using System.Data;

namespace BITCollege_EU.Models
{
    /// <summary>
    /// Student Model - to represent the Students table in the database
    /// </summary>
    public class Student
    {
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int StudentId { get; set; }

        [Required]
        [ForeignKey("GradePointState")]
        public int GradePointStateId { get; set; }

        [ForeignKey("AcademicProgram")]
        public int? AcademicProgramId { get; set; }

        [Display(Name = "Student\nNumber")]
        public long StudentNumber { get; set; }

        [Required]
        [StringLength(35)]
        [Display(Name = "First\nName")]
        public String FirstName { get; set; }

        [Required]
        [StringLength(35, MinimumLength = 1)]
        [Display(Name = "Last\nName")]
        public String LastName { get; set; }

        [Required]
        [StringLength(35, MinimumLength = 1)]
        public String Address { get; set; }

        [Required]
        [StringLength(35, MinimumLength = 1)]
        public String City { get; set; }

        [Required]
        [RegularExpression(@"^(?:AB|BC|MB|N[BLTSU]|ON|PE|QC|SK|YT)$", ErrorMessage = "Please enter a valid Province code.")]
        public String Province { get; set; }

        [Required]
        [RegularExpression(@"^([A-Za-z]\d[A-Za-z][ -]?\d[A-Za-z]\d)", ErrorMessage = "Please enter a valid Postal Code.")]
        [Display(Name = "Postal\nCode")]
        public String PostalCode { get; set; }

        [Required]
        [Display(Name = "Date\nCreated")]
        public DateTime DateCreated { get; set; }

        [Display(Name = "Grade\nPoint\nAverage")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:0.00}")]
        [Range(0, 4.5)]
        public double? GradePointAverage { get; set; }

        [Required]
        [Display(Name = "Outstanding\nFees")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:c}")]
        public double OutstandingFees { get; set; }

        public String Notes { get; set; }

        [Display(Name = "Name")]
        public String FullName
        { 
            get
            {
                return String.Format("{0} {1}", FirstName, LastName);
            }
        }

        [Display(Name = "Address")]
        public String FullAddress
        {
            get
            {
                return String.Format("{0} {1}, {2} {3}", Address, City, Province, PostalCode);
            }
        }

        /// <summary>
        /// Defining a protected static variable of the data context object.
        /// </summary>
        protected static BITCollege_EUContext db = new BITCollege_EUContext();

        /// <summary>
        /// Initiates the process of ensuring that the Student is always associated with the correct state.
        /// </summary>
        public void ChangeState()
        {
            //declares GradePointState variables to compare in the dowhile.
            GradePointState gradePointState = null;
            GradePointState newGradePointState = null;
            do
            {
                //while gradePointState and newGradePointState variables are not equal to each other
                //set gradePointState to the GradePointStateId of the student and run the StateChangeCheck()
                //method, then set the newGradePointState to the GradePointStateId and evaluate again until equal.
                gradePointState = db.GradePointStates.Find(this.GradePointStateId);
                gradePointState.StateChangeCheck(this);              
                newGradePointState = db.GradePointStates.Find(this.GradePointStateId);

            } while (gradePointState != newGradePointState);                                          
        }

        /// <summary>
        /// Sets next student number based on the value returned from NextNumber.
        /// </summary>
        public void SetNextStudentNumber()
        {
            long? studentNumber = StoredProcedure.NextNumber("NextStudent");
            if (studentNumber != null)
            {
                StudentNumber = (long)studentNumber;
            }
            else
            {
                StudentNumber = -1;
            }
        }

        //defining navigational properties for Student
        public virtual ICollection<Registration> Registration { get; set; }
        public virtual ICollection<StudentCard> StudentCard { get; set; }
        public virtual GradePointState GradePointState { get; set; }
        public virtual AcademicProgram AcademicProgram { get; set; }

    }

    /// <summary>
    /// StudentCard Model -
    /// </summary>
    public class StudentCard
    {
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int StudentCardId { get; set; }

        [Required]
        [ForeignKey("Student")]
        public int StudentId { get; set; }

        [Required]
        public long CardNumber { get; set; }

        //defining navigational properties for StudentCard
        public virtual Student Student { get; set; }
    }

    /// <summary>
    /// AcademicProgram Model - to represent the AcademicPrograms table in the database
    /// </summary>
    public class AcademicProgram
    {
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int AcademicProgramId { get; set; }

        [Required]
        [Display(Name = "Program")]
        public String ProgramAcronym { get; set; }

        [Required]
        [Display(Name = "Program\nName")]
        public String Description { get; set; }

        //defining navigational properties for AcademicProgram
        public virtual ICollection<Student> Student { get; set; }
        public virtual ICollection<Course> Course { get; set; }
    }

    /// <summary>
    /// GradePointState Model - to represent the GradePointStates table in the database
    /// </summary>
    public abstract class GradePointState
    {
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        [Key]
        public int GradePointStateId { get; set; }

        [Required]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:0.00}")]
        [Display(Name = "Lower\nLimit")]
        public double LowerLimit { get; set; }

        [Required]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:0.00}")]
        [Display(Name = "Upper\nLimit")]
        public double UpperLimit { get; set; }

        [Required]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:0.00}")]
        [Display(Name = "Tution\nRate\nFactor")]
        public double TuitionRateFactor { get; set; }

        [Display(Name = "Grade\nPoint\nState")]
        public String Description 
        {
            get
            {
                var match = Regex.Match(this.GetType().Name, @"(.*)State.*");
                if (match.Success)
                {
                    return match.Groups[1].Value;
                }
                else
                {
                    return "Invalid State Name.";
                }
            }
        }

        /// <summary>
        /// Virtual method to be overloaded in the child classes. Checks the student's GradePointStateId, GradePoint
        /// Average, and total courses completed to evaluate the TuitionRateAdjustment returned.
        /// </summary>
        /// <param name="student">Represents the student in the database</param>
        /// <returns>The TuitionRateAdjustment for the student</returns>
        public virtual double TuitionRateAdjustment(Student student)
        {
            return 1.0;
        }

        /// <summary>
        /// Virtual method to be overloaded in the child classes. Checks if the students GradePointStateId property 
        /// is in the correct state and adjusts it as necessary.
        /// </summary>
        /// <param name="student">Represents the student in the database.</param>
        public virtual void StateChangeCheck(Student student)
        {          
        }

        /// <summary>
        /// Defining a protected static variable of the data context object.
        /// </summary>
        protected static BITCollege_EUContext db = new BITCollege_EUContext();
       
        //defining navigational properties for GradePointState
        public virtual ICollection<Student> Student { get; set; }
    }

    /// <summary>
    /// SuspendedState Model - to represent the SuspendedStates table in the database
    /// </summary>
    public class SuspendedState : GradePointState
    {
        private static SuspendedState suspendedState = null;

        /// <summary>
        /// Sets the UpperLimit, LowerLimit, and TuitionRateFactor of SuspendedState.
        /// </summary>
        private SuspendedState()
        {

            UpperLimit = 1.00;
            LowerLimit = 0.00;
            TuitionRateFactor = 1.1;
           
        }
        
        /// <summary>
        /// Method that checks the GetInstance of SuspendedState and creates a new one if not found in the database
        /// or returns the one found.
        /// </summary>
        /// <returns>The SuspendedState instance</returns>
        public static SuspendedState GetInstance()
        {          
            if (suspendedState == null)
            {
                //if suspendedState is null, uses SingleOrDefault() method
                //to grab the only record of SuspendedStates and store it in the variable.
                //If suspendedState still evaluates to null, there was never anything in the database               
                suspendedState = db.SuspendedStates.SingleOrDefault();
                if (suspendedState == null)
                {
                    //sets a new SuspendedState to the variable and populates it before persisting it to the database.
                    suspendedState = new SuspendedState();
                    db.SuspendedStates.Add(suspendedState);
                    db.SaveChanges();
                }            
            }
            return suspendedState;
        }

        /// <summary>
        /// The override method for Suspended State's TuitionRateAdjustment.
        /// Calculates the tuitionRateAdjustment for the student when the method is called.
        /// </summary>
        /// <param name="student">Represents the student in the database.</param>
        /// <returns>The tuitionRateAdjustment</returns>
        public override double TuitionRateAdjustment(Student student)
        {
            //Sets students gpa and tuitionratefactor as variables.
            double? gradePointAverage = student.GradePointAverage;
            double tuitionRateAdjustment = TuitionRateFactor;

            //if gpa is below a certain amount, adjust the tuition rate.
            if (gradePointAverage < 0.50)
            {
                tuitionRateAdjustment += 0.05;
            }
            else if (gradePointAverage < 0.75)
            {
                tuitionRateAdjustment += 0.02;
            }
            return tuitionRateAdjustment;
        }

        /// <summary>
        /// Checks if the students gpa is in the right state and sets it to the next state adjactent to Suspended State if it
        /// passes the upper or lower limit.
        /// </summary>
        /// <param name="student">Represents the student in the database.</param>
        public override void StateChangeCheck(Student student)
        {
            if (student.GradePointAverage > this.UpperLimit)
            {
                student.GradePointStateId = ProbationState.GetInstance().GradePointStateId;
            }
            if (student.GradePointAverage < this.LowerLimit)
            {
                student.GradePointStateId = SuspendedState.GetInstance().GradePointStateId;
            }
            db.SaveChanges();
        }
    }

    /// <summary>
    /// ProbationState Model - to represent the ProbationStates table in the database
    /// </summary>
    public class ProbationState : GradePointState
    {
        private static ProbationState probationState = null;

        /// <summary>
        /// Sets the UpperLimit, LowerLimit, and TuitionRateFactor of ProbationState.
        /// </summary>
        private ProbationState()
        {
            LowerLimit = 1.00;
            UpperLimit = 2.00;
            TuitionRateFactor = 1.075;
        }

        /// <summary>
        /// Method that checks the GetInstance of ProbationState and creates a new one if not found in the database
        /// or returns the one found.
        /// </summary>
        /// <returns>The ProbationState instance</returns>
        public static ProbationState GetInstance()
        {
            //if probationState is null, uses SingleOrDefault() method
            //to grab the only record of ProbationStates and store it in the variable.
            //If probationState still evaluates to null, there was never anything in the database
            if (probationState == null)
            {
                //sets a new SuspendedState to the variable and populates it before persisting it to the database.
                probationState = db.ProbationStates.SingleOrDefault();
                if (probationState == null)
                {
                    probationState = new ProbationState();
                    db.ProbationStates.Add(probationState);
                    db.SaveChanges();
                }
            }
            return probationState;
        }

        /// <summary>
        /// The override method for Probation State's TuitionRateAdjustment.
        /// Calculates the tuitionRateAdjustment for the student when the method is called.
        /// </summary>
        /// <param name="student">Represents the student in the database.</param>
        /// <returns>tuitionRateAdjustment</returns>
        public override double TuitionRateAdjustment(Student student)
        {
            //sets variables for the students gpa, the tuition rate factor, and 
            //makes the LINQ statement to select the courses the student is registered in and that has a grade.
            IQueryable<Registration> registrations = from results in db.Registrations
                                                     where results.StudentId == student.StudentId
                                                     where results.Grade != null
                                                     select results;
            double? gradePointAverage = student.GradePointAverage;
            double tuitionRateAdjustment = TuitionRateFactor;

            //if student has equal to or more than 5 graded courses,
            //adjust the tuition rate to the new discount.
            if (registrations.Count() >= 5)
            {
                tuitionRateAdjustment -= 0.04;
            }
            return tuitionRateAdjustment;
        }

        /// <summary>
        /// Checks if the students gpa is in the right state and sets it to the next state adjactent to Probation State if it
        /// passes the upper or lower limit.
        /// </summary>
        /// <param name="student">Represents the student in the database.</param>
        public override void StateChangeCheck(Student student)
        {             
            if (student.GradePointAverage > this.UpperLimit)
            {
                student.GradePointStateId = RegularState.GetInstance().GradePointStateId;
            }
            if (student.GradePointAverage < this.LowerLimit)
            {
                student.GradePointStateId = SuspendedState.GetInstance().GradePointStateId;
            }
            db.SaveChanges();
        }
    }

    /// <summary>
    /// RegularState Model - to represent the RegularStates table in the database
    /// </summary>
    public class RegularState : GradePointState
    {
        private static RegularState regularState = null;

        /// <summary>
        /// Sets the UpperLimit, LowerLimit, and TuitionRateFactor of RegularState.
        /// </summary>
        private RegularState()
        {
            LowerLimit = 2;
            UpperLimit = 3.70;
            TuitionRateFactor = 1;
        }

        /// <summary>
        /// Method that checks the GetInstance of RegularState and creates a new one if not found in the database
        /// or returns the one found.
        /// </summary>
        /// <returns>The RegularState Instance</returns>
        public static RegularState GetInstance()
        {
            //if RegularState is null, uses SingleOrDefault() method
            //to grab the only record of RegularStates and store it in the variable.
            //If regularState still evaluates to null, there was never anything in the database
            if (regularState == null)
            {
                //sets a new RegularState to the variable and populates it before persisting it to the database.
                regularState = db.RegularStates.SingleOrDefault();
                if (regularState == null)
                {
                    regularState = new RegularState();
                    db.RegularStates.Add(regularState);
                    db.SaveChanges();
                }
            }
            return regularState;
        }

        /// <summary>
        /// The override method for Regular State's TuitionRateAdjustment.
        /// Calculates the tuitionRateAdjustment for the student when the method is called. 
        /// </summary>
        /// <param name="student">Represents the student in the database</param>
        /// <returns>TuitionRateFactor</returns>
        public override double TuitionRateAdjustment(Student student)
        {
            //No evaluation needs to be done for Regular State at this time.
            return TuitionRateFactor;
        }

        /// <summary>
        /// Checks if the students gpa is in the right state and sets it to the next state adjactent to Regular State if it
        /// passes the upper or lower limit.
        /// </summary>
        /// <param name="student">Represents the student in the database</param>
        public override void StateChangeCheck(Student student)
        {
            if (student.GradePointAverage > this.UpperLimit)
            {
                student.GradePointStateId = HonoursState.GetInstance().GradePointStateId;
            }
            if (student.GradePointAverage < this.LowerLimit)
            {
                student.GradePointStateId = ProbationState.GetInstance().GradePointStateId;
            }
            db.SaveChanges();
        }
    
    }

    /// <summary>
    /// HonoursState Model - to represent the HonoursStates table in the database
    /// </summary>
    public class HonoursState : GradePointState
    {
        private static HonoursState honoursState = null;

        /// <summary>
        /// Sets the UpperLimit, LowerLimit, and TuitionRateFactor of HonoursState.
        /// </summary>
        private HonoursState()
        {
            LowerLimit = 3.70;
            UpperLimit = 4.5;
            TuitionRateFactor = .9;
        }

        /// <summary>
        /// Method that checks the GetInstance of HonoursState and creates a new one if not found in the database
        /// or returns the one found.
        /// </summary>
        /// <returns>The HonoursState instance</returns>
        public static HonoursState GetInstance()
        {
            //if HonoursState is null, uses SingleOrDefault() method
            //to grab the only record of HonoursStates and store it in the variable.
            //If honoursState still evaluates to null, there was never anything in the database
            if (honoursState == null)
            {
                //sets a new HonoursState to the variable and populates it before persisting it to the database.
                honoursState = db.HonoursStates.SingleOrDefault();
                if (honoursState == null)
                {
                    honoursState = new HonoursState();
                    db.HonoursStates.Add(honoursState);
                    db.SaveChanges();
                }
            }
            return honoursState;
        }

        /// <summary>
        /// The override method for Honour State's TuitionRateAdjustment.
        /// Calculates the tuitionRateAdjustment for the student when the method is called. 
        /// </summary>
        /// <param name="student">Represents the student in the database</param>
        /// <returns>The TuitionRateAdjustment</returns>
        public override double TuitionRateAdjustment(Student student)
        {
            //sets variables for the students gpa, the tuition rate factor, and 
            //makes the LINQ statement to select the courses the student is registered in and that has a grade.
            IQueryable<Registration> registrations = from results in db.Registrations
                                                     where results.StudentId == student.StudentId
                                                     where results.Grade != null
                                                     select results;
            double? gradePointAverage = student.GradePointAverage;
            double tuitionRateAdjustment = TuitionRateFactor;

            //if student has equal to or more than 5 graded courses,
            //and/or their gpa is above 4.25, 
            //adjust the tuition rate to the new discount.
            if (registrations.Count() >= 5)
            {
                tuitionRateAdjustment -= 0.05;
            }
            if (gradePointAverage > 4.25)
            {
                tuitionRateAdjustment -= 0.02;
            }
            return tuitionRateAdjustment;
        }

        /// <summary>
        /// Checks if the students gpa is in the right state and sets it to the next state adjactent to Honours State if it
        /// passes the upper or lower limit.
        /// </summary>
        /// <param name="student">Represents the student in the database</param>
        public override void StateChangeCheck(Student student)
        {
            if (student.GradePointAverage < this.LowerLimit)
            {
                student.GradePointStateId = RegularState.GetInstance().GradePointStateId;
            }
            if (student.GradePointAverage > this.UpperLimit)
            {
                student.GradePointStateId = HonoursState.GetInstance().GradePointStateId;
            }
            db.SaveChanges();
        }
    }

    /// <summary>
    /// Course Model - to represent the Courses table in the database
    /// </summary>
    public abstract class Course
    {
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        [Key]
        public int CourseId { get; set; }

        [ForeignKey("AcademicProgram")]
        public int? AcademicProgramId { get; set; }

        [Display(Name = "Course\nNumber")]
        public String CourseNumber { get; set; }

        [Required]
        public String Title { get; set; }

        [Required]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:0.00}")]
        [Display(Name = "Credit\nHours")]
        public double CreditHours { get; set; }

        [Required]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:$0.00}")]
        [Display(Name = "Tuition\nAmount")]
        public double TuitionAmount { get; set; }

        [Display(Name = "Course\nType")]
        public String CourseType 
        {
            get
            {              
                var match = Regex.Match(this.GetType().Name, @"(.*)Course.*");
                if (match.Success)
                {
                    return match.Groups[1].Value;
                }
                else
                {
                    return "Invalid Course Name.";
                }
            }
        }


        public String Notes { get; set; }

        /// <summary>
        /// Method to be overriden in the Course subclasses
        /// </summary>
        public virtual void SetNextCourseNumber()
        {

        }

        //defining navigational properties for Course
        public virtual ICollection<Registration> Registration { get; set; }
        public virtual AcademicProgram AcademicProgram { get; set; }
    }

    /// <summary>
    /// GradedCourse Model - to represent the GradedCourses table in the database
    /// </summary>
    public class GradedCourse : Course
    {
        [Required]
        [Display(Name = "Assignment\nWeight")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:p}")]
        public double AssignmentWeight { get; set; }

        [Required]
        [Display(Name = "Midterm\nWeight")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:p}")]
        public double MidtermWeight { get; set; }

        [Required]
        [Display(Name = "Final\nWeight")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString ="{0:p}")]
        public double FinalWeight { get; set; }

        /// <summary>
        /// Override method that sets appropriate letter followed by the value returned from NextNumber.
        /// </summary>
        public override void SetNextCourseNumber()
        {
            long? courseNumber = StoredProcedure.NextNumber("NextGradedCourse");
            if (courseNumber != null)
            {
                CourseNumber = "G-" + courseNumber.ToString();
            }
            else
            {
                CourseNumber = "G-Error";
            }
        }
    }

    /// <summary>
    /// MasteryCourse Model - to represent the MasteryCourses table in the database
    /// </summary>
    public class MasteryCourse : Course
    {
        [Required]
        [Display(Name = "Maximum\nAttempts")]
        public int MaximumAttempts { get; set; }

        /// <summary>
        /// Override method that sets appropriate letter followed by the value returned from NextNumber.
        /// </summary>
        public override void SetNextCourseNumber()
        {
            long? courseNumber = StoredProcedure.NextNumber("NextMasterCourse");
            if (courseNumber != null)
            {
                CourseNumber = "M-" + courseNumber.ToString();
            }
            else
            {
                CourseNumber = "M-Error";
            }
        }
    }

    /// <summary>
    /// AuditCourse Model - to represent the AuditCourses table in the database
    /// </summary>
    public class AuditCourse : Course
    {
        /// <summary>
        /// Override method that sets appropriate letter followed by the value returned from NextNumber.
        /// </summary>
        public override void SetNextCourseNumber()
        {
            long? courseNumber = StoredProcedure.NextNumber("NextAuditCourse");
            if (courseNumber != null)
            {
                CourseNumber = "A-" + courseNumber.ToString();
            }
            else
            {
                CourseNumber = "A-Error";
            }
        }
    }

    /// <summary>
    /// Registration Model - to represent the Registrations table in the database
    /// </summary>
    public class Registration
    {
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int RegistrationId { get; set; }

        [Required]
        [ForeignKey("Student")]
        public int StudentId { get; set; }

        [Required]
        [ForeignKey("Course")]
        public int CourseId { get; set; }

        [Display(Name = "Registration\nNumber")]
        public long RegistrationNumber { get; set; }

        [Required]
        [Display(Name = "Registration\nDate")]
        [DisplayFormat(DataFormatString = "{0:d}")]
        public DateTime RegistrationDate { get; set; }

        [DisplayFormat(NullDisplayText = "Ungraded")]
        [Range(0,1)]
        public double? Grade { get; set; }


        public String Notes { get; set; }

        /// <summary>
        /// Method that sets appropriate letter followed by the value returned from NextNumber.
        /// </summary>
        public void SetNextRegistrationNumber()
        {
            long? registrationNumber = StoredProcedure.NextNumber("NextRegistration");
            if (registrationNumber != null)
            {
                RegistrationNumber = (long)registrationNumber;
            }
            else
            {
                RegistrationNumber = -1;
            }
        }

        //defining navigational properties for Registration
        public virtual Course Course { get; set; }
        public virtual Student  Student { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public abstract class NextUniqueNumber
    {
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        [Key]
        public int NextUniqueNumberId { get; set; }

        [Required]
        public long NextAvailableNumber { get; set; }

        /// <summary>
        /// Defining a protected static variable of the data context object.
        /// </summary>
        protected static BITCollege_EUContext db = new BITCollege_EUContext();
    }

    /// <summary>
    /// 
    /// </summary>
    public class NextGradedCourse : NextUniqueNumber
    {
        /// <summary>
        /// 
        /// </summary>
        private static NextGradedCourse nextGradedCourse = null;

        private NextGradedCourse()
        {
            NextAvailableNumber = 200000;
        }

        /// <summary>
        /// Method that checks the GetInstance of NextGradedCourse and creates a new one if not found in the database
        /// or returns the one found.
        /// </summary>
        /// <returns>The NextGradedCourse instance</returns>
        public static NextGradedCourse GetInstance()
        {
            //if nextGradedCourse is null, uses SingleOrDefault() method
            //to grab the only record of NextGradedCourse and store it in the variable.
            //If nextGradedCourse still evaluates to null, there was never anything in the database
            if (nextGradedCourse == null)
            {
                //sets a new NextGradedCourse to the variable and populates it before persisting it to the database.
                nextGradedCourse = db.NextGradedCourses.SingleOrDefault();
                if (nextGradedCourse == null)
                {
                    nextGradedCourse = new NextGradedCourse();
                    db.NextGradedCourses.Add(nextGradedCourse);
                    db.SaveChanges();
                }
            }
            return nextGradedCourse;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class NextAuditCourse : NextUniqueNumber
    {
        /// <summary>
        /// 
        /// </summary>
        private static NextAuditCourse nextAuditCourse = null;

        /// <summary>
        /// 
        /// </summary>
        private NextAuditCourse()
        {
            NextAvailableNumber = 2000;
        }

        /// <summary>
        /// Method that checks the GetInstance of NextAuditCourse and creates a new one if not found in the database
        /// or returns the one found.
        /// </summary>
        /// <returns>The NextGradedCourse instance</returns>
        public static NextAuditCourse GetInstance()
        {
            //if nextAuditCourse is null, uses SingleOrDefault() method
            //to grab the only record of NextAuditCourse and store it in the variable.
            //If nextAuditCourse still evaluates to null, there was never anything in the database
            if (nextAuditCourse == null)
            {
                //sets a new NextAuditCourse to the variable and populates it before persisting it to the database.
                nextAuditCourse = db.NextAuditCourses.SingleOrDefault();
                if (nextAuditCourse == null)
                {
                    nextAuditCourse = new NextAuditCourse();
                    db.NextAuditCourses.Add(nextAuditCourse);
                    db.SaveChanges();
                }
            }
            return nextAuditCourse;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class NextMasteryCourse : NextUniqueNumber
    {
        /// <summary>
        /// 
        /// </summary>
        private static NextMasteryCourse nextMasteryCourse = null;

        /// <summary>
        /// 
        /// </summary>
        private NextMasteryCourse()
        {
            NextAvailableNumber = 20000;
        }

        /// <summary>
        /// Method that checks the GetInstance of NextMasteryCourse and creates a new one if not found in the database
        /// or returns the one found.
        /// </summary>
        /// <returns>The NextMasteryCourse instance</returns>
        public static NextMasteryCourse GetInstance()
        {
            //if nextMasteryCourse is null, uses SingleOrDefault() method
            //to grab the only record of NextGradedCourse and store it in the variable.
            //If nextGradedCourse still evaluates to null, there was never anything in the database
            if (nextMasteryCourse == null)
            {
                //sets a new NextMasteryCourse to the variable and populates it before persisting it to the database.
                nextMasteryCourse = db.NextMasteryCourses.SingleOrDefault();
                if (nextMasteryCourse == null)
                {
                    nextMasteryCourse = new NextMasteryCourse();
                    db.NextMasteryCourses.Add(nextMasteryCourse);
                    db.SaveChanges();
                }
            }
            return nextMasteryCourse;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class NextStudent : NextUniqueNumber
    {
        /// <summary>
        /// 
        /// </summary>
        private static NextStudent nextStudent = null;

        /// <summary>
        /// 
        /// </summary>
        private NextStudent()
        {
            NextAvailableNumber = 20000000;
        }

        /// <summary>
        /// Method that checks the GetInstance of NextStudent and creates a new one if not found in the database
        /// or returns the one found.
        /// </summary>
        /// <returns>The NextStudent instance</returns>
        public static NextStudent GetInstance()
        {
            //if nextStudent is null, uses SingleOrDefault() method
            //to grab the only record of NextStudent and store it in the variable.
            //If nextStudent still evaluates to null, there was never anything in the database
            if (nextStudent == null)
            {
                //sets a new nextStudent to the variable and populates it before persisting it to the database.
                nextStudent = db.NextStudents.SingleOrDefault();
                if (nextStudent == null)
                {
                    nextStudent = new NextStudent();
                    db.NextStudents.Add(nextStudent);
                    db.SaveChanges();
                }
            }
            return nextStudent;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class NextRegistration : NextUniqueNumber
    {
        /// <summary>
        /// 
        /// </summary>
        private static NextRegistration nextRegistration = null;

        /// <summary>
        /// 
        /// </summary>
        private NextRegistration()
        {
            NextAvailableNumber = 700;
        }

        /// <summary>
        /// Method that checks the GetInstance of NextRegistration and creates a new one if not found in the database
        /// or returns the one found.
        /// </summary>
        /// <returns>The NextRegistration instance</returns>
        public static NextRegistration GetInstance()
        {
            //if nextGradedCourse is null, uses SingleOrDefault() method
            //to grab the only record of NextRegistration and store it in the variable.
            //If nextRegistration still evaluates to null, there was never anything in the database
            if (nextRegistration == null)
            {
                //sets a new NextRegistration to the variable and populates it before persisting it to the database.
                nextRegistration = db.NextRegistrations.SingleOrDefault();
                if (nextRegistration == null)
                {
                    nextRegistration = new NextRegistration();
                    db.NextRegistrations.Add(nextRegistration);
                    db.SaveChanges();
                }
            }
            return nextRegistration;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public static class StoredProcedure
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="discriminator"></param>
        /// <returns></returns>
        public static long? NextNumber(string discriminator)
        {
            try
            {
                //Creates a new SqlConnection called connection, setting the Data Source as localhost and
                //the Initial Catalog as BITCollege_EUContext.
                SqlConnection connection = new SqlConnection("Data Source=localhost; " +
                "Initial Catalog=BITCollege_EUContext;Integrated Security=True");

                //Initially sets returnValue to 0.
                long? returnValue = 0;

                //Creates a new SqlCommand called storedProcedure and sets it to next_number to call that procedure
                //and gives it the sql connection for the database it will be using.
                SqlCommand storedProcedure = new SqlCommand("next_number", connection);

                //Tells it that the command type for storedProcedure is a Stored Procedure.
                storedProcedure.CommandType = CommandType.StoredProcedure;

                //Sets the value of @Discriminator as the string discriminator given.
                storedProcedure.Parameters.AddWithValue("@Discriminator", discriminator);

                //Creates a new SqlParameter called outputParameter and sets the value of @NewVal to SqlDbType.BigInt.
                SqlParameter outputParameter = new SqlParameter("@NewVal", SqlDbType.BigInt)
                {
                    //Sets the ParameterDirection of outputParameter as Output only.
                    Direction = ParameterDirection.Output
                };

                //Adds outputParameter as a parameter to storedProcedure.
                storedProcedure.Parameters.Add(outputParameter);

                //Opens a database connection using the property settings given by connection.
                connection.Open();

                //Executes storeProcedure and returns the number of rows affected.
                storedProcedure.ExecuteNonQuery();

                //Closes the database connection.
                connection.Close();

                //Sets returnValue to the value of outputParameter and cast it to a long.
                returnValue = (long?)outputParameter.Value;

                //Returns the final value.
                return returnValue;
            }
            catch
            {
                return null;
            }
        }
    }
}