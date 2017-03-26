namespace TemperatureWeb.Migrations
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;
    using TemperatureWeb.Models;

    internal sealed class Configuration : DbMigrationsConfiguration<TemperatureWeb.Models.IoTDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(TemperatureWeb.Models.IoTDbContext context)
        {
            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method 
            //  to avoid creating duplicate seed data. E.g.
            //

            var user = new ApplicationUser()
            {
                UserName = "momosplit",
                Email = "momo@mailinator.com",                
                PasswordHash = "ALE3O2ifM24OW3YX7wkH35eeWdgkR1dhHZYkJgMMbujGLjCgFzbO/2ZHpjgQXl9X3g==",
                SecurityStamp = "0ea6cf71-ecf9-42d2-9f69-f4b9a5d53223",                                
                Devices = new List<Device>() { new Device() { Name = "FESBTemperature", Key = "FESBTemperature" } }
            };            

            context.Users.AddOrUpdate(
              p => p.UserName,
              user
            );

            context.SaveChanges();

            if(!context.Devices.Any())
            {
                context.Devices.AddOrUpdate(
                    p => p.Key,
                    new Device() { Name = "FESBTemperature", Key = "FESBTemperature", ApplicationUserId = user.Id });

                context.SaveChanges();
            }            
        }
    }
}
