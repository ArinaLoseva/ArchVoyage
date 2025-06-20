using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Channels;
using static System.Reflection.Metadata.BlobBuilder;

namespace ArchVoyage.Models
{
    public class AppDBContext : DbContext
    {
        public AppDBContext(DbContextOptions<AppDBContext> options) : base(options)
        {
        }

        public DbSet<Users> Users { get; set; }

        public DbSet<TechData> TechData { get; set; }

        public DbSet<Forms> Forms { get; set; }

        public DbSet<Routes> Routes { get; set; }

        public DbSet<Moderation> Moderation { get; set; }

    }
}
