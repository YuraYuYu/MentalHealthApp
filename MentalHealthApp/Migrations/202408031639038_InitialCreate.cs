namespace MentalHealthApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Appointments", "RatingScore", c => c.Double());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Appointments", "RatingScore", c => c.Int());
        }
    }
}
