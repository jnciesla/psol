namespace PSol.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Initial : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Mobs",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 36),
                        Health = c.Single(nullable: false),
                        Shield = c.Single(nullable: false),
                        X = c.Single(nullable: false),
                        Y = c.Single(nullable: false),
                        Dir = c.Int(nullable: false),
                        Rotation = c.Single(nullable: false),
                        SpawnDate = c.DateTime(nullable: false),
                        Type_Id = c.String(maxLength: 36),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.MobTypes", t => t.Type_Id)
                .Index(t => t.Type_Id);
            
            CreateTable(
                "dbo.MobTypes",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 36),
                        Name = c.String(maxLength: 255),
                        MaxHealth = c.Int(nullable: false),
                        MaxShield = c.Int(nullable: false),
                        Sprite = c.Int(nullable: false),
                        Level = c.Int(nullable: false),
                        BonusExp = c.Int(nullable: false),
                        Credits = c.Int(nullable: false),
                        MaxSpawned = c.Int(nullable: false),
                        SpawnTimeMin = c.Int(nullable: false),
                        SpawnTimeMax = c.Int(nullable: false),
                        SpawnRadius = c.Single(nullable: false),
                        Star_Id = c.String(maxLength: 36),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Stars", t => t.Star_Id)
                .Index(t => t.Star_Id);
            
            CreateTable(
                "dbo.Stars",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 36),
                        Name = c.String(maxLength: 255),
                        X = c.Single(nullable: false),
                        Y = c.Single(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Planets",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 36),
                        StarId = c.String(maxLength: 36),
                        Name = c.String(maxLength: 255),
                        X = c.Single(nullable: false),
                        Y = c.Single(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Stars", t => t.StarId)
                .Index(t => t.StarId);
            
            CreateTable(
                "dbo.Users",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 36),
                        Login = c.String(maxLength: 255),
                        Password = c.String(maxLength: 255),
                        Name = c.String(maxLength: 255),
                        MaxHealth = c.Int(nullable: false),
                        Health = c.Int(nullable: false),
                        MaxShield = c.Int(nullable: false),
                        Shield = c.Int(nullable: false),
                        Sprite = c.Int(nullable: false),
                        Level = c.Int(nullable: false),
                        Exp = c.Int(nullable: false),
                        Map = c.Int(nullable: false),
                        X = c.Single(nullable: false),
                        Y = c.Single(nullable: false),
                        Dir = c.Int(nullable: false),
                        Rotation = c.Single(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Mobs", "Type_Id", "dbo.MobTypes");
            DropForeignKey("dbo.MobTypes", "Star_Id", "dbo.Stars");
            DropForeignKey("dbo.Planets", "StarId", "dbo.Stars");
            DropIndex("dbo.Planets", new[] { "StarId" });
            DropIndex("dbo.MobTypes", new[] { "Star_Id" });
            DropIndex("dbo.Mobs", new[] { "Type_Id" });
            DropTable("dbo.Users");
            DropTable("dbo.Planets");
            DropTable("dbo.Stars");
            DropTable("dbo.MobTypes");
            DropTable("dbo.Mobs");
        }
    }
}