using Microsoft.EntityFrameworkCore;
using TerapiaDeApoio.Domain.Entities;

namespace TerapiaDeApoio.Infrastructure.Data;

public class TerapiaDeApoioDbContext : DbContext
{
    public TerapiaDeApoioDbContext(
        DbContextOptions<TerapiaDeApoioDbContext> options)
        : base(options)
    {
    }

    public DbSet<Paciente> Pacientes => Set<Paciente>();

    public DbSet<Profissional> Profissionais => Set<Profissional>();

    public DbSet<Servico> Servicos => Set<Servico>();

    public DbSet<DisponibilidadeProfissional>
        DisponibilidadesProfissionais
        => Set<DisponibilidadeProfissional>();

    public DbSet<ProfissionalServico> ProfissionalServicos
        => Set<ProfissionalServico>();

    public DbSet<Agendamento> Agendamentos => Set<Agendamento>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ProfissionalServico>()
            .HasKey(ps => new
            {
                ps.ProfissionalId,
                ps.ServicoId
            });

        modelBuilder.Entity<ProfissionalServico>()
            .HasOne(ps => ps.Profissional)
            .WithMany(p => p.ProfissionalServicos)
            .HasForeignKey(ps => ps.ProfissionalId);

        modelBuilder.Entity<ProfissionalServico>()
            .HasOne(ps => ps.Servico)
            .WithMany(s => s.ProfissionalServicos)
            .HasForeignKey(ps => ps.ServicoId);

        modelBuilder.Entity<DisponibilidadeProfissional>()
            .HasOne(d => d.Profissional)
            .WithMany(p => p.Disponibilidades)
            .HasForeignKey(d => d.ProfissionalId);

        modelBuilder.Entity<Agendamento>(entity =>
        {
            entity.HasOne(a => a.Paciente)
                .WithMany(p => p.Agendamentos)
                .HasForeignKey(a => a.PacienteId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(a => a.Profissional)
                .WithMany(p => p.Agendamentos)
                .HasForeignKey(a => a.ProfissionalId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(a => a.Servico)
                .WithMany(s => s.Agendamentos)
                .HasForeignKey(a => a.ServicoId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.Property(a => a.DataHoraInicio)
                .HasColumnType("timestamp without time zone");

            entity.Property(a => a.DataHoraFim)
                .HasColumnType("timestamp without time zone");

            entity.Property(a => a.CriadoEm)
                .HasColumnType("timestamp with time zone");
        });
    }
}