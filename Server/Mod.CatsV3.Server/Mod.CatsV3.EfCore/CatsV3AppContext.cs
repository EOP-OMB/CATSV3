using Microsoft.EntityFrameworkCore;
using Mod.CatsV3.Domain.Entities;
using Mod.Framework.EfCore;
using Mod.Framework.Runtime.Session;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Mod.CatsV3.EfCore
{
    public class CatsV3AppContext : ModDbContext<CatsV3AppContext>
    {
        #region Tables
        public DbSet<Correspondence> Correspondences { get; set; }
        public DbSet<Collaboration> Collaborations  { get; set; }
        public DbSet<Reviewer>  Reviewers { get; set; }
        public DbSet<Originator> Originators { get; set; }
        public DbSet<FYIUser> FYIUsers { get; set; }
        public DbSet<SurrogateOriginator> surrogateOriginators { get; set; }
        public DbSet<SurrogateReviewer> surrogateReviewers { get; set; }
        public DbSet<LetterType> letterTypes { get; set; }
        public DbSet<LeadOffice> LeadOffices { get; set; }
        public DbSet<LeadOfficeMember> leadOfficeMembers { get; set; }
        public DbSet<LeadOfficeOfficeManager> leadOfficeOfficeManagers { get; set; }
        public DbSet<ExternalUser> ExternalUsers { get; set; }
        public DbSet<Folder> Folders { get; set; }
        public DbSet<Document> Documents { get; set; }
        public DbSet<ReviewRound> ReviewRounds { get; set; }
        public DbSet<DLGroup> DLGroups { get; set; }
       // public DbSet<EmployeeExtended> Employees { get; set; }
        public DbSet<DLGroupMembers> DLGroupMembers { get; set; }
        public DbSet<EmailNotificationLog> EmailNotificationLogs { get; set; }
        public DbSet<OldIQMasterList> OldIQMasterLists { get; set; }
        public DbSet<HelpDocument> HelpDocuments { get; set; }
        public DbSet<CorrespondenceCopiedArchived> CorrespondenceCopiedArchiveds { get; set; }
        public DbSet<CorrespondenceCopiedOffice> copiedOffices { get; set; }
        public DbSet<Administration> administrations { get; set; }
        public DbSet<Role>  Roles { get; set; }
        #endregion

        public CatsV3AppContext(DbContextOptions<CatsV3AppContext> options, IModSession session)
            : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());            

            base.OnModelCreating(modelBuilder);


            //modelBuilder.Entity<Correspondence>()
            //  .Property(b => b.CATSID)
            //  .HasDefaultValueSql("getdate()");

            ////modelBuilder.Entity<Correspondence>()
            ////    .Property(b => b.CATSID)
            ////    .ValueGeneratedOnAdd();
            //DateTime dt = DateTime.Now;

            //base.OnModelCreating(modelBuilder);
            //modelBuilder.Entity<Correspondence>()
            //    .Property(b => b.FiscalYear)
            //    .HasDefaultValue(dt.ToString("yyyy"));


        }


    }
}
