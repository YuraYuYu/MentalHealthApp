namespace MentalHealthApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateDatabaseSchema : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Psychologists", "Photo", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Psychologists", "Photo");
        }
    }
}
