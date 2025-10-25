using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebDBFinal.Entities;

[Table("RegistroMovimientoResidencial")]
public partial class RegistroMovimientoResidencial
{
    [Key]
    public int CodigoMovimientoResidencial { get; set; }

    public int CodigoGaritaSeguridad { get; set; }

    public int? CodigoLicencia { get; set; }

    public int CodigoPersonaRol { get; set; }

    public int CodigoPersona { get; set; }

    public byte CodigoTipoRol { get; set; }

    public int CodigoPersonaTelefono { get; set; }

    public int? NumeroCasa { get; set; }

    public int? CodigoCluster { get; set; }

    public int? CodigoSucursal { get; set; }

    public int? CodigoSector { get; set; }

    public int? CodigoVehiculo { get; set; }

    public int? CodigoVehiculoResidente { get; set; }

    [StringLength(100)]
    [Unicode(false)]
    public string? MotivoVisita { get; set; }

    [ForeignKey("NumeroCasa, CodigoCluster, CodigoSucursal, CodigoSector")]
    [InverseProperty("RegistroMovimientoResidencials")]
    public virtual Casa? Casa { get; set; }

    [ForeignKey("CodigoGaritaSeguridad")]
    [InverseProperty("RegistroMovimientoResidencials")]
    public virtual GaritaSeguridad CodigoGaritaSeguridadNavigation { get; set; } = null!;

    [ForeignKey("CodigoLicencia")]
    [InverseProperty("RegistroMovimientoResidencials")]
    public virtual Licencia? CodigoLicenciaNavigation { get; set; }

    [ForeignKey("CodigoPersonaTelefono")]
    [InverseProperty("RegistroMovimientoResidencials")]
    public virtual PersonaTelefono CodigoPersonaTelefonoNavigation { get; set; } = null!;

    [ForeignKey("CodigoVehiculo")]
    [InverseProperty("RegistroMovimientoResidencials")]
    public virtual Vehiculo? CodigoVehiculoNavigation { get; set; }

    [ForeignKey("CodigoPersonaRol, CodigoPersona, CodigoTipoRol")]
    [InverseProperty("RegistroMovimientoResidencials")]
    public virtual PersonaRol PersonaRol { get; set; } = null!;

    [InverseProperty("CodigoMovimientoResidencialNavigation")]
    public virtual ICollection<RegistroIngresoOSalida> RegistroIngresoOSalida { get; set; } = new List<RegistroIngresoOSalida>();
}
