using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebDBFinal.Entities;

[PrimaryKey("NumeroCasa", "CodigoCluster", "CodigoSucursal", "CodigoSector")]
[Table("Casa")]
public partial class Casa
{
    [Key]
    public int NumeroCasa { get; set; }

    [Key]
    public int CodigoCluster { get; set; }

    [Key]
    public int CodigoSucursal { get; set; }

    [Key]
    public int CodigoSector { get; set; }

    public bool EsAlquilada { get; set; }

    public bool EsOcupada { get; set; }

    [InverseProperty("Casa")]
    public virtual ICollection<CargoMensualCasa> CargoMensualCasas { get; set; } = new List<CargoMensualCasa>();

    [ForeignKey("CodigoCluster, CodigoSucursal, CodigoSector")]
    [InverseProperty("Casas")]
    public virtual Cluster Cluster { get; set; } = null!;

    [InverseProperty("Casa")]
    public virtual ICollection<DetalleCenso> DetalleCensos { get; set; } = new List<DetalleCenso>();

    [InverseProperty("Casa")]
    public virtual ICollection<EstadoCuentum> EstadoCuenta { get; set; } = new List<EstadoCuentum>();

    [InverseProperty("Casa")]
    public virtual ICollection<Multa> Multa { get; set; } = new List<Multa>();

    [InverseProperty("Casa")]
    public virtual ICollection<PersonaRol> PersonaRols { get; set; } = new List<PersonaRol>();

    [InverseProperty("Casa")]
    public virtual ICollection<RegistroMovimientoResidencial> RegistroMovimientoResidencials { get; set; } = new List<RegistroMovimientoResidencial>();

    [InverseProperty("Casa")]
    public virtual ICollection<RegistroVehiculoResidente> RegistroVehiculoResidentes { get; set; } = new List<RegistroVehiculoResidente>();
}
