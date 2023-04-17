using Microsoft.EntityFrameworkCore;
using Microsoft.UI.Xaml.Media.Imaging;
using System.IO;
using System;
using System.Diagnostics;
using Microsoft.UI.Xaml.Media;

namespace QRCoderDemo
{
    public class ImageData
    {
        public string Name { get; set; }

        public DateTime CreateTime { get; set; }= DateTime.Now;
    }
    public class SQLiteContext : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            optionsBuilder.UseSqlite("Data Source=My.db");
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ImageData>().ToTable("T_ImageData")
                .HasKey(x=>x.Name);
            base.OnModelCreating(modelBuilder);
        }
        public DbSet<ImageData> ImageDatas { get; set; }  
    }
}
