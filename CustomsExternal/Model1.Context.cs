﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace CustomsExternal
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class CustomsExternalEntities : DbContext
    {
        public CustomsExternalEntities()
            : base("name=CustomsExternalEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<ConsignmentPackagesMeasures> ConsignmentPackagesMeasures { get; set; }
        public virtual DbSet<ConsignmentRegisteredFacilities> ConsignmentRegisteredFacilities { get; set; }
        public virtual DbSet<Consignments> Consignments { get; set; }
        public virtual DbSet<Declarations> Declarations { get; set; }
        public virtual DbSet<DeclarationTaxes> DeclarationTaxes { get; set; }
        public virtual DbSet<DocumentAttribute> DocumentAttribute { get; set; }
        public virtual DbSet<Documents> Documents { get; set; }
        public virtual DbSet<Registration> Registration { get; set; }
        public virtual DbSet<SupplierInvoiceItems> SupplierInvoiceItems { get; set; }
        public virtual DbSet<SupplierInvoices> SupplierInvoices { get; set; }
        public virtual DbSet<ChangeLog> ChangeLog { get; set; }
        public virtual DbSet<LoginHistory> LoginHistory { get; set; }
    }
}
