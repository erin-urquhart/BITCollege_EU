namespace BITCollege_EU.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class studentRegistrationCorseDataAnnotations : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Courses", "CourseNumber", c => c.String());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Courses", "CourseNumber", c => c.String(nullable: false));
        }
    }
}
