
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace AMC_THEATER_1.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext() : base("DB2Connection")
        {
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            // ✅ Correct way to set schema for EF6
            modelBuilder.Entity<MST_TT_TYPE>().ToTable("MST_TT_TYPE", "AMCTHEATER");
            modelBuilder.Entity<MST_DOCS>().ToTable("MST_DOCS", "AMCTHEATER");
            modelBuilder.Entity<MST_STATUS>().ToTable("MST_STATUS", "AMCTHEATER");
            modelBuilder.Entity<USER_LOGIN_DETAILS>().ToTable("USER_LOGIN_DETAILS", "AMCTHEATER");
            modelBuilder.Entity<DEPT_LOGIN_DETAILS>().ToTable("DEPT_LOGIN_DETAILS", "AMCTHEATER");
            modelBuilder.Entity<TRN_REGISTRATION>().ToTable("TRN_REGISTRATION", "AMCTHEATER");
            modelBuilder.Entity<NO_OF_SCREENS>().ToTable("NO_OF_SCREENS", "AMCTHEATER");
            modelBuilder.Entity<TRN_THEATRE_DOCS>().ToTable("TRN_THEATRE_DOCS", "AMCTHEATER");
            modelBuilder.Entity<THEATER_TAX_PAYMENT>().ToTable("THEATER_TAX_PAYMENT", "AMCTHEATER");
            modelBuilder.Entity<NO_OF_SCREENS_TAX>().ToTable("NO_OF_SCREENS_TAX", "AMCTHEATER");

            // Uncomment if needed
            // modelBuilder.Entity<PAYMENT_HISTORY>().ToTable("PAYMENT_HISTORY", "AMCTHEATER");
            // modelBuilder.Entity<RECEIPT_FILTER>().ToTable("RECEIPT_FILTER", "AMCTHEATER");
            // modelBuilder.Entity<T_RECEIPT>().ToTable("T_RECEIPT", "AMCTHEATER");
            // modelBuilder.Entity<PENDINGDUEADMIN>().ToTable("PENDINGDUEADMIN", "AMCTHEATER");
            // modelBuilder.Entity<PAYMENTLIST>().ToTable("PAYMENTLIST", "AMCTHEATER");
            // modelBuilder.Entity<TRN_SCREEN_TAX_PRICE>().ToTable("TRN_SCREEN_TAX_PRICE", "AMCTHEATER");
            // modelBuilder.Entity<ActionRequest>().ToTable("ActionRequest", "AMCTHEATER");
            // modelBuilder.Entity<MonthTable>().ToTable("MonthTables", "AMCTHEATER");

            base.OnModelCreating(modelBuilder);
        }

        public DbSet<MST_TT_TYPE> MST_TT_TYPE { get; set; }
        public DbSet<MST_DOCS> MST_DOCS { get; set; }
        public DbSet<MST_STATUS> MST_STATUS { get; set; }
        public DbSet<USER_LOGIN_DETAILS> USER_LOGIN_DETAILS { get; set; }
        public DbSet<DEPT_LOGIN_DETAILS> DEPT_LOGIN_DETAILS { get; set; }
        public DbSet<TRN_REGISTRATION> TRN_REGISTRATION { get; set; }
        public DbSet<NO_OF_SCREENS> NO_OF_SCREENS { get; set; }
        public DbSet<TRN_THEATRE_DOCS> TRN_THEATRE_DOCS { get; set; }
        public DbSet<THEATER_TAX_PAYMENT> THEATER_TAX_PAYMENT { get; set; }
        public DbSet<NO_OF_SCREENS_TAX> NO_OF_SCREENS_TAX { get; set; }
    }
}
