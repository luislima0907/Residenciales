using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using WebDBFinal.Entities;

namespace WebDBFinal.Context;

public partial class ResidencialesDbContext : DbContext
{
    public ResidencialesDbContext()
    {
    }

    public ResidencialesDbContext(DbContextOptions<ResidencialesDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AplicacionDocumento> AplicacionDocumentos { get; set; }

    public virtual DbSet<CargoMensualCasa> CargoMensualCasas { get; set; }

    public virtual DbSet<Casa> Casas { get; set; }

    public virtual DbSet<Censo> Censos { get; set; }

    public virtual DbSet<Cluster> Clusters { get; set; }

    public virtual DbSet<ControlGaritaSeguridad> ControlGaritaSeguridads { get; set; }

    public virtual DbSet<Departamento> Departamentos { get; set; }

    public virtual DbSet<DetalleCargoMensual> DetalleCargoMensuals { get; set; }

    public virtual DbSet<DetalleCenso> DetalleCensos { get; set; }

    public virtual DbSet<DetalleEstadoCuentum> DetalleEstadoCuenta { get; set; }

    public virtual DbSet<DetalleFacturacionSeguridad> DetalleFacturacionSeguridads { get; set; }

    public virtual DbSet<DetallePagoDocumento> DetallePagoDocumentos { get; set; }

    public virtual DbSet<DireccionSucursal> DireccionSucursals { get; set; }

    public virtual DbSet<DocumentoFiscal> DocumentoFiscals { get; set; }

    public virtual DbSet<EmpresaSeguridad> EmpresaSeguridads { get; set; }

    public virtual DbSet<EstadoCivil> EstadoCivils { get; set; }

    public virtual DbSet<EstadoCuentum> EstadoCuenta { get; set; }

    public virtual DbSet<GaritaSeguridad> GaritaSeguridads { get; set; }

    public virtual DbSet<IntegranteJuntum> IntegranteJunta { get; set; }

    public virtual DbSet<JuntaDirectiva> JuntaDirectivas { get; set; }

    public virtual DbSet<Licencium> Licencia { get; set; }

    public virtual DbSet<LineaVehiculo> LineaVehiculos { get; set; }

    public virtual DbSet<MarcaVehiculo> MarcaVehiculos { get; set; }

    public virtual DbSet<MarcajeLaboral> MarcajeLaborals { get; set; }

    public virtual DbSet<Multum> Multa { get; set; }

    public virtual DbSet<Municipio> Municipios { get; set; }

    public virtual DbSet<PeriodoFacturacionSeguridad> PeriodoFacturacionSeguridads { get; set; }

    public virtual DbSet<Persona> Personas { get; set; }

    public virtual DbSet<PersonaRol> PersonaRols { get; set; }

    public virtual DbSet<PersonaTelefono> PersonaTelefonos { get; set; }

    public virtual DbSet<RegistroIngresoOSalidum> RegistroIngresoOSalida { get; set; }

    public virtual DbSet<RegistroMovimientoResidencial> RegistroMovimientoResidencials { get; set; }

    public virtual DbSet<RegistroPersonaNoGratum> RegistroPersonaNoGrata { get; set; }

    public virtual DbSet<RegistroVehiculoNoPermitido> RegistroVehiculoNoPermitidos { get; set; }

    public virtual DbSet<RegistroVehiculoResidente> RegistroVehiculoResidentes { get; set; }

    public virtual DbSet<Residencial> Residencials { get; set; }

    public virtual DbSet<Sector> Sectors { get; set; }

    public virtual DbSet<Sucursal> Sucursals { get; set; }

    public virtual DbSet<TelefonoSucursal> TelefonoSucursals { get; set; }

    public virtual DbSet<TipoConcepto> TipoConceptos { get; set; }

    public virtual DbSet<TipoDocumentoFiscal> TipoDocumentoFiscals { get; set; }

    public virtual DbSet<TipoGaritum> TipoGarita { get; set; }

    public virtual DbSet<TipoIntegrante> TipoIntegrantes { get; set; }

    public virtual DbSet<TipoLicencium> TipoLicencia { get; set; }

    public virtual DbSet<TipoMarcaje> TipoMarcajes { get; set; }

    public virtual DbSet<TipoMultum> TipoMulta { get; set; }

    public virtual DbSet<TipoPago> TipoPagos { get; set; }

    public virtual DbSet<TipoRol> TipoRols { get; set; }

    public virtual DbSet<TipoTelefono> TipoTelefonos { get; set; }

    public virtual DbSet<Vehiculo> Vehiculos { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            // Esto solo se usará si no se configura en Program.cs
            //#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
            optionsBuilder.UseSqlServer("Server=${SERVER_NAME};Database=${DATABASE_NAME};User Id=sa;Password=${PASSWORD_DB};TrustServerCertificate=True;");
        }  
    }
      
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AplicacionDocumento>(entity =>
        {
            entity.Property(e => e.CodigoAplicacionDocumento).ValueGeneratedNever();
            entity.Property(e => e.IVA).HasDefaultValue(0m);

            entity.HasOne(d => d.CodigoMultaNavigation).WithMany(p => p.AplicacionDocumentos).HasConstraintName("FK_AplicacionDocumento_Multa");

            entity.HasOne(d => d.DetalleCargoMensual).WithMany(p => p.AplicacionDocumentos).HasConstraintName("FK_AplicacionDocumento_DetalleCargo");

            entity.HasOne(d => d.DocumentoFiscal).WithMany(p => p.AplicacionDocumentos)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AplicacionDocumento_DocumentoFiscal");
        });

        modelBuilder.Entity<CargoMensualCasa>(entity =>
        {
            entity.Property(e => e.CodigoCargoMensual).ValueGeneratedNever();
            entity.Property(e => e.Pagado).HasDefaultValue(false);
            entity.HasKey(e => e.CodigoCargoMensual);
            

            entity.Property(e => e.FechaVencimiento)
                .HasColumnType("date")  
                .IsRequired();

            // Índice único para evitar cargos duplicados
            entity.HasIndex(e => new { 
                e.NumeroCasa, 
                e.CodigoCluster, 
                e.CodigoSucursal, 
                e.CodigoSector, 
                e.MesCargo, 
                e.AnioCargo 
            }).IsUnique();
            
            entity.HasOne(d => d.Casa).WithMany(p => p.CargoMensualCasas)
                .OnDelete(DeleteBehavior.ClientSetNull)
                
                .HasConstraintName("FK_CargoMensualCasa_Casa");
        });

        modelBuilder.Entity<Casa>(entity =>
        {
            entity.Property(e => e.EsOcupada).HasDefaultValue(true);

            entity.HasOne(d => d.Cluster).WithMany(p => p.Casas)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Casa_Cluster");
        });

        modelBuilder.Entity<Censo>(entity =>
        {
            entity.Property(e => e.CodigoCenso).ValueGeneratedNever();
        });

        modelBuilder.Entity<Cluster>(entity =>
        {
            entity.HasOne(d => d.CodigoSectorNavigation).WithMany(p => p.Clusters)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Cluster_Sector");

            entity.HasOne(d => d.CodigoSucursalNavigation).WithMany(p => p.Clusters)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Cluster_Sucursal");
        });

        modelBuilder.Entity<ControlGaritaSeguridad>(entity =>
        {
            entity.Property(e => e.CodigoControl).ValueGeneratedNever();

            entity.HasOne(d => d.CodigoMarcajeNavigation).WithMany(p => p.ControlGaritaSeguridads)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ControlGaritaSeguridad_Marcaje");

            entity.HasOne(d => d.PersonaRol).WithMany(p => p.ControlGaritaSeguridads)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ControlGaritaSeguridad_PersonaRol");
        });

        modelBuilder.Entity<Departamento>(entity =>
        {
            entity.Property(e => e.CodigoDepartamento).ValueGeneratedNever();
        });

        modelBuilder.Entity<DetalleCargoMensual>(entity =>
        {
            entity.HasOne(d => d.CodigoCargoMensualNavigation).WithMany(p => p.DetalleCargoMensuals)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DetalleCargoMensual_CargoMensual");

            entity.HasOne(d => d.CodigoTipoConceptoNavigation).WithMany(p => p.DetalleCargoMensuals)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DetalleCargoMensual_TipoConcepto");
        });

        modelBuilder.Entity<DetalleCenso>(entity =>
        {
            entity.HasOne(d => d.CodigoCensoNavigation).WithMany(p => p.DetalleCensos)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DetalleCenso_Censo");

            entity.HasOne(d => d.PersonaRol).WithMany(p => p.DetalleCensos).HasConstraintName("FK_DetalleCenso_PersonaRol");

            entity.HasOne(d => d.Casa).WithMany(p => p.DetalleCensos)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DetalleCenso_Casa");
        });

        modelBuilder.Entity<DetalleEstadoCuentum>(entity =>
        {
            entity.HasOne(d => d.DetallePagoDocumentoNavigation).WithMany(p => p.DetalleEstadoCuenta)
                .HasForeignKey(d => new { d.CodigoDetallePagoDocumento, d.CodigoAplicacionDocumento })
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DetalleEstadoCuenta_DetallePago");

            entity.HasOne(d => d.NumeroEstadoCuentaNavigation).WithMany(p => p.DetalleEstadoCuenta)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DetalleEstadoCuenta_EstadoCuenta");
        });

        modelBuilder.Entity<DetalleFacturacionSeguridad>(entity =>
        {
            entity.HasKey(e => new { e.CodigoDetalleFacturacion, e.CodigoPeriodoFacturacion }).HasName("PK_DetalleFacturacion");

            entity.HasOne(d => d.CodigoControlNavigation).WithMany(p => p.DetalleFacturacionSeguridads).HasConstraintName("FK_DetalleFacturacionSeguridad_Control");

            entity.HasOne(d => d.CodigoMarcajeNavigation).WithMany(p => p.DetalleFacturacionSeguridads).HasConstraintName("FK_DetalleFacturacionSeguridad_Marcaje");

            entity.HasOne(d => d.CodigoPeriodoFacturacionNavigation).WithMany(p => p.DetalleFacturacionSeguridads)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DetalleFacturacionSeguridad_Periodo");
        });

        modelBuilder.Entity<DetallePagoDocumento>(entity =>
        {
            entity.Property(e => e.FechaPago).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.CodigoAplicacionDocumentoNavigation).WithMany(p => p.DetallePagoDocumentos)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DetallePagoDocumento_Aplicacion");

            entity.HasOne(d => d.CodigoTipoPagoNavigation).WithMany(p => p.DetallePagoDocumentos)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DetallePagoDocumento_TipoPago");
        });

        modelBuilder.Entity<DireccionSucursal>(entity =>
        {
            entity.HasOne(d => d.CodigoSucursalNavigation).WithMany(p => p.DireccionSucursals)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DireccionSucursal_Sucursal");

            entity.HasOne(d => d.Municipio).WithMany(p => p.DireccionSucursals)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DireccionSucursal_Municipio");
        });

        modelBuilder.Entity<DocumentoFiscal>(entity =>
        {
            entity.Property(e => e.FechaEmision).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.CodigoTipoDocumentoNavigation).WithMany(p => p.DocumentoFiscals)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DocumentoFiscal_TipoDocumento");

            entity.HasOne(d => d.PersonaRol).WithMany(p => p.DocumentoFiscals)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DocumentoFiscal_PersonaRol");
        });

        modelBuilder.Entity<EmpresaSeguridad>(entity =>
        {
            entity.Property(e => e.CodigoEmpresa).ValueGeneratedNever();
        });

        modelBuilder.Entity<EstadoCuentum>(entity =>
        {
            entity.HasKey(e => e.NumeroEstadoCuenta).HasName("PK_EstadoCuentaGenerado");

            entity.Property(e => e.NumeroEstadoCuenta).ValueGeneratedNever();
            entity.Property(e => e.FechaGeneracion).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.PersonaRol).WithMany(p => p.EstadoCuenta)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_EstadoCuentaGenerado_PersonaRol");

            entity.HasOne(d => d.Casa).WithMany(p => p.EstadoCuenta)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_EstadoCuentaGenerado_Casa");
        });

        modelBuilder.Entity<GaritaSeguridad>(entity =>
        {
            entity.Property(e => e.CodigoGaritaSeguridad).ValueGeneratedNever();

            entity.HasOne(d => d.CodigoTipoGaritaNavigation).WithMany(p => p.GaritaSeguridads)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_GaritaSeguridad_TipoGarita");

            entity.HasOne(d => d.Cluster).WithMany(p => p.GaritaSeguridads).HasConstraintName("FK_GaritaSeguridad_Cluster");
        });

        modelBuilder.Entity<IntegranteJuntum>(entity =>
        {
            entity.Property(e => e.CodigoIntegranteJunta).ValueGeneratedNever();
            entity.Property(e => e.FechaDesignacion).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.CodigoTipoIntegranteNavigation).WithMany(p => p.IntegranteJunta)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_IntegranteJunta_TipoIntegrante");

            entity.HasOne(d => d.PersonaRol).WithMany(p => p.IntegranteJunta)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_IntegranteJunta_PersonaRol");

            entity.HasOne(d => d.JuntaDirectiva).WithMany(p => p.IntegranteJunta)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_IntegranteJunta_Junta");
        });

        modelBuilder.Entity<JuntaDirectiva>(entity =>
        {
            entity.HasOne(d => d.Cluster).WithMany(p => p.JuntaDirectivas)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_JuntaDirectiva_Cluster");
        });

        modelBuilder.Entity<Licencium>(entity =>
        {
            entity.Property(e => e.CodigoLicencia).ValueGeneratedNever();

            entity.HasOne(d => d.CodigoPersonaNavigation).WithMany(p => p.Licencia)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Licencia_Persona");

            entity.HasOne(d => d.CodigoTipoLicenciaNavigation).WithMany(p => p.Licencia)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Licencia_TipoLicencia");
        });

        modelBuilder.Entity<LineaVehiculo>(entity =>
        {
            entity.HasOne(d => d.CodigoMarcaNavigation).WithMany(p => p.LineaVehiculos)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_LineaVehiculo_Marca");
        });

        modelBuilder.Entity<MarcaVehiculo>(entity =>
        {
            entity.Property(e => e.CodigoMarca).ValueGeneratedNever();
        });

        modelBuilder.Entity<MarcajeLaboral>(entity =>
        {
            entity.Property(e => e.CodigoMarcaje).ValueGeneratedNever();

            entity.HasOne(d => d.CodigoGaritaSeguridadNavigation).WithMany(p => p.MarcajeLaborals)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MarcajeLaboral_Garita");

            entity.HasOne(d => d.CodigoTipoMarcajeNavigation).WithMany(p => p.MarcajeLaborals)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MarcajeLaboral_TipoMarcaje");

            entity.HasOne(d => d.PersonaRol).WithMany(p => p.MarcajeLaborals)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MarcajeLaboral_PersonaRol");
        });

        modelBuilder.Entity<Multum>(entity =>
        {
            entity.Property(e => e.CodigoMulta).ValueGeneratedNever();
            entity.Property(e => e.Fecha).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Pagada).HasDefaultValue(false);

            entity.HasOne(d => d.CodigoTipoMultaNavigation).WithMany(p => p.Multa)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Multa_TipoMulta");

            entity.HasOne(d => d.Casa).WithMany(p => p.Multa)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Multa_Casa");
        });

        modelBuilder.Entity<Municipio>(entity =>
        {
            entity.HasOne(d => d.CodigoDepartamentoNavigation).WithMany(p => p.Municipios)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Municipio_Departamento");
        });

        modelBuilder.Entity<PeriodoFacturacionSeguridad>(entity =>
        {
            entity.Property(e => e.CodigoPeriodoFacturacion).ValueGeneratedNever();

            entity.HasOne(d => d.CodigoSucursalNavigation).WithMany(p => p.PeriodoFacturacionSeguridads)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PeriodoFacturacionSeguridad_Sucursal");
        });

        modelBuilder.Entity<Persona>(entity =>
        {
            entity.Property(e => e.CodigoPersona).ValueGeneratedNever();
            entity.Property(e => e.Genero).IsFixedLength();

            entity.HasOne(d => d.CodigoEstadoCivilNavigation).WithMany(p => p.Personas)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Persona_EstadoCivil");
        });

        modelBuilder.Entity<PersonaRol>(entity =>
        {
            entity.HasOne(d => d.CodigoPersonaNavigation).WithMany(p => p.PersonaRols)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PersonaRol_Persona");

            entity.HasOne(d => d.CodigoTipoRolNavigation).WithMany(p => p.PersonaRols)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PersonaRol_TipoRol");

            entity.HasOne(d => d.Casa).WithMany(p => p.PersonaRols).HasConstraintName("FK_PersonaRol_Casa");
        });

        modelBuilder.Entity<PersonaTelefono>(entity =>
        {
            entity.Property(e => e.CodigoPersonaTelefono).ValueGeneratedNever();

            entity.HasOne(d => d.CodigoPersonaNavigation).WithMany(p => p.PersonaTelefonos)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PersonaTelefono_Persona");

            entity.HasOne(d => d.CodigoTipoTelefonoNavigation).WithMany(p => p.PersonaTelefonos)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PersonaTelefono_TipoTelefono");
        });

        modelBuilder.Entity<RegistroIngresoOSalidum>(entity =>
        {
            entity.Property(e => e.CodigoEntradaOSalida).ValueGeneratedNever();
            entity.Property(e => e.FechaHora).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.TipoMovimiento).IsFixedLength();

            entity.HasOne(d => d.CodigoMovimientoResidencialNavigation).WithMany(p => p.RegistroIngresoOSalida)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RegistroIngresoOSalida_Movimiento");
        });

        modelBuilder.Entity<RegistroMovimientoResidencial>(entity =>
        {
            entity.Property(e => e.CodigoMovimientoResidencial).ValueGeneratedNever();

            entity.HasOne(d => d.CodigoGaritaSeguridadNavigation).WithMany(p => p.RegistroMovimientoResidencials)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RegistroMovimientoResidencial_Garita");

            entity.HasOne(d => d.CodigoLicenciaNavigation).WithMany(p => p.RegistroMovimientoResidencials).HasConstraintName("FK_RegistroMovimientoResidencial_Licencia");

            entity.HasOne(d => d.CodigoPersonaTelefonoNavigation).WithMany(p => p.RegistroMovimientoResidencials)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RegistroMovimientoResidencial_PersonaTelefono");

            entity.HasOne(d => d.CodigoVehiculoNavigation).WithMany(p => p.RegistroMovimientoResidencials).HasConstraintName("FK_RegistroMovimientoResidencial_Vehiculo");

            entity.HasOne(d => d.PersonaRol).WithMany(p => p.RegistroMovimientoResidencials)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RegistroMovimientoResidencial_PersonaRol");

            entity.HasOne(d => d.Casa).WithMany(p => p.RegistroMovimientoResidencials).HasConstraintName("FK_RegistroMovimientoResidencial_Casa");
        });

        modelBuilder.Entity<RegistroPersonaNoGratum>(entity =>
        {
            entity.Property(e => e.CodigoRegistroPersonaNoGrata).ValueGeneratedNever();
            entity.Property(e => e.Estado).HasDefaultValue(true);
            entity.Property(e => e.FechaDeclaracion).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.PersonaRol).WithMany(p => p.RegistroPersonaNoGrata)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RegistroPersonaNoGrata_PersonaRol");
        });

        modelBuilder.Entity<RegistroVehiculoNoPermitido>(entity =>
        {
            entity.Property(e => e.CodigoVehiculoNoPermitido).ValueGeneratedNever();
            entity.Property(e => e.Estado).HasDefaultValue(true);
            entity.Property(e => e.FechaDeclaracion).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.CodigoVehiculoNavigation).WithMany(p => p.RegistroVehiculoNoPermitidos)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RegistroVehiculoNoPermitido_Vehiculo");
        });

        modelBuilder.Entity<RegistroVehiculoResidente>(entity =>
        {
            entity.Property(e => e.CodigoVehiculoResidente).ValueGeneratedNever();
            entity.Property(e => e.Activo).HasDefaultValue(true);
            entity.Property(e => e.FechaRegistro).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.CodigoVehiculoNavigation).WithMany(p => p.RegistroVehiculoResidentes)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RegistroVehiculoResidente_Vehiculo");

            entity.HasOne(d => d.PersonaRol).WithMany(p => p.RegistroVehiculoResidentes)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RegistroVehiculoResidente_PersonaRol");

            entity.HasOne(d => d.Casa).WithMany(p => p.RegistroVehiculoResidentes)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RegistroVehiculoResidente_Casa");
        });

        modelBuilder.Entity<Residencial>(entity =>
        {
            entity.Property(e => e.CodigoResidencial).ValueGeneratedNever();
        });

        modelBuilder.Entity<Sector>(entity =>
        {
            entity.Property(e => e.CodigoSector).ValueGeneratedNever();
        });

        modelBuilder.Entity<Sucursal>(entity =>
        {
            entity.Property(e => e.CodigoSucursal).ValueGeneratedNever();

            entity.HasOne(d => d.CodigoEmpresaNavigation).WithMany(p => p.Sucursals).HasConstraintName("FK_Sucursal_Empresa");

            entity.HasOne(d => d.CodigoResidencialNavigation).WithMany(p => p.Sucursals).HasConstraintName("FK_Sucursal_Residencial");
        });

        modelBuilder.Entity<TelefonoSucursal>(entity =>
        {
            entity.HasOne(d => d.CodigoSucursalNavigation).WithMany(p => p.TelefonoSucursals)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TelefonoSucursal_Sucursal");

            entity.HasOne(d => d.CodigoTipoTelefonoNavigation).WithMany(p => p.TelefonoSucursals)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TelefonoSucursal_TipoTelefono");
        });

        modelBuilder.Entity<TipoLicencium>(entity =>
        {
            entity.Property(e => e.TipoLicencia).IsFixedLength();
        });

        modelBuilder.Entity<Vehiculo>(entity =>
        {
            entity.Property(e => e.CodigoVehiculo).ValueGeneratedNever();

            entity.HasOne(d => d.LineaVehiculo).WithMany(p => p.Vehiculos)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Vehiculo_LineaVehiculo");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
