using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace WebAPIFacturacion.Models;

public partial class MiFacturacionContext : DbContext
{
    public MiFacturacionContext(DbContextOptions<MiFacturacionContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Cliente> Clientes { get; set; }

    public virtual DbSet<Factura> Facturas { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Cliente>(entity =>
        {
            entity.HasKey(e => e.IdCliente).HasName("PK__Clientes__D59466429F379E58");

            entity.Property(e => e.Ciudad).HasMaxLength(150);
            entity.Property(e => e.Nombre).HasMaxLength(150);
        });

        modelBuilder.Entity<Factura>(entity =>
        {
            entity.HasKey(e => e.Nfactura).HasName("PK__Facturas__C77EA12CB4B606E9");

            entity.Property(e => e.Nfactura).HasColumnName("NFactura");
            entity.Property(e => e.Importe).HasColumnType("decimal(9, 2)");

            entity.HasOne(d => d.IdClienteNavigation).WithMany(p => p.Facturas)
                .HasForeignKey(d => d.IdCliente)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Clientes_Facturas");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
