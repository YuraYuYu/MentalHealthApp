namespace MentalHealthApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateDatabaseSchema : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Appointments", "CaseFilePath", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Appointments", "CaseFilePath");
        }
    }
}
