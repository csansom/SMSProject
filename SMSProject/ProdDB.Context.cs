﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace SMSProject
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class DB_A4A060_csEntities : DbContext
    {
        public DB_A4A060_csEntities()
            : base("name=DB_A4A060_csEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<C__MigrationHistory> C__MigrationHistory { get; set; }
        public virtual DbSet<AspNetRole> AspNetRoles { get; set; }
        public virtual DbSet<AspNetUserClaim> AspNetUserClaims { get; set; }
        public virtual DbSet<AspNetUserLogin> AspNetUserLogins { get; set; }
        public virtual DbSet<AspNetUser> AspNetUsers { get; set; }
        public virtual DbSet<Bolu> Bolus { get; set; }
        public virtual DbSet<Cows_log> Cows_log { get; set; }
        public virtual DbSet<FarmCow> FarmCows { get; set; }
        public virtual DbSet<Farm> Farms { get; set; }
        public virtual DbSet<Log> Logs { get; set; }
        public virtual DbSet<MeasData> MeasDatas { get; set; }
        public virtual DbSet<SP_SETTINGS> SP_SETTINGS { get; set; }
        public virtual DbSet<sysdiagram> sysdiagrams { get; set; }
        public virtual DbSet<UserCabinet> UserCabinets { get; set; }
        public virtual DbSet<Z_AlertLogs> Z_AlertLogs { get; set; }
        public virtual DbSet<datagapstab> datagapstabs { get; set; }
    }
}
