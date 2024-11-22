using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace WebAPIBiblioteca.Models;

public partial class MiBibliotecaContext : DbContext
{
    public MiBibliotecaContext(DbContextOptions<MiBibliotecaContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Autore> Autores { get; set; }

    public virtual DbSet<Editoriale> Editoriales { get; set; }

    public virtual DbSet<Libro> Libros { get; set; }

    public virtual DbSet<Operacione> Operaciones { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Autore>(entity =>
        {
            entity.HasKey(e => e.IdAutor).HasName("PK__Autores__DD33B031D2D32164");

            entity.Property(e => e.Nombre).HasMaxLength(100);
        });

        modelBuilder.Entity<Editoriale>(entity =>
        {
            entity.HasKey(e => e.IdEditorial).HasName("PK__Editoria__EF8386715FF6846C");

            entity.Property(e => e.Nombre).HasMaxLength(100);
        });

        modelBuilder.Entity<Libro>(entity =>
        {
            entity.HasKey(e => e.Isbn).HasName("PK__Libros__447D36EB56C744C8");

            entity.Property(e => e.Isbn)
                .HasMaxLength(100)
                .HasColumnName("ISBN");
            entity.Property(e => e.FotoPortadaUrl).HasColumnName("FotoPortadaURL");
            entity.Property(e => e.Precio).HasColumnType("decimal(9, 2)");
            entity.Property(e => e.Titulo).HasMaxLength(100);

            entity.HasOne(d => d.Autor).WithMany(p => p.Libros)
                .HasForeignKey(d => d.AutorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Autores_Libros");

            entity.HasOne(d => d.Editorial).WithMany(p => p.Libros)
                .HasForeignKey(d => d.EditorialId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Editoriales_Libros");
        });

        modelBuilder.Entity<Operacione>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Operacio__3214EC07851CF16D");

            entity.Property(e => e.Controller).HasMaxLength(50);
            entity.Property(e => e.FechaAccion).HasColumnType("datetime");
            entity.Property(e => e.Ip).HasMaxLength(50);
            entity.Property(e => e.Operacion).HasMaxLength(50);
            entity.Property(e => e.Usuario).HasMaxLength(100);
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Usuarios__3214EC078F96DFF7");

            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.EnlaceCambioPass).HasMaxLength(500);
            entity.Property(e => e.FechaEnvioEnlace).HasColumnType("datetime");
            entity.Property(e => e.Password).HasMaxLength(500);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
