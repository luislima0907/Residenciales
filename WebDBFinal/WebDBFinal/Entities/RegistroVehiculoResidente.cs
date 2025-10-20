using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebDBFinal.Entities;

[Table("RegistroVehiculoResidente")]
[Index("NumeroTarjetaTalanquera", Name = "UK_RegistroVehiculoResidente_Tarjeta", IsUnique = true)]
public partial class RegistroVehiculoResidente
{
    [Key]
    public int CodigoVehiculoResidente { get; set; }

    public int CodigoVehiculo { get; set; }

    public int CodigoPersonaRol { get; set; }

    public byte CodigoTipoRol { get; set; }

    public int CodigoPersona { get; set; }

    public int NumeroCasa { get; set; }

    public int CodigoCluster { get; set; }

    public int CodigoSucursal { get; set; }

    public int CodigoSector { get; set; }

    [StringLength(20)]
    [Unicode(false)]
    public string NumeroTarjetaTalanquera { get; set; } = null!;

    public DateOnly? FechaRegistro { get; set; }

    public bool? Activo { get; set; }

    [ForeignKey("NumeroCasa, CodigoCluster, CodigoSucursal, CodigoSector")]
    [InverseProperty("RegistroVehiculoResidentes")]
    public virtual Casa Casa { get; set; } = null!;

    [ForeignKey("CodigoVehiculo")]
    [InverseProperty("RegistroVehiculoResidentes")]
    public virtual Vehiculo CodigoVehiculoNavigation { get; set; } = null!;

    [ForeignKey("CodigoPersonaRol, CodigoPersona, CodigoTipoRol")]
    [InverseProperty("RegistroVehiculoResidentes")]
    public virtual PersonaRol PersonaRol { get; set; } = null!;
}
