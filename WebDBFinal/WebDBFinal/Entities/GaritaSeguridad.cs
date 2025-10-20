using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebDBFinal.Entities;

[Table("GaritaSeguridad")]
public partial class GaritaSeguridad
{
    [Key]
    public int CodigoGaritaSeguridad { get; set; }

    public byte CodigoTipoGarita { get; set; }

    public int? CodigoCluster { get; set; }

    public int? CodigoSucursal { get; set; }

    public int? CodigoSector { get; set; }

    [ForeignKey("CodigoCluster, CodigoSucursal, CodigoSector")]
    [InverseProperty("GaritaSeguridads")]
    public virtual Cluster? Cluster { get; set; }

    [ForeignKey("CodigoTipoGarita")]
    [InverseProperty("GaritaSeguridads")]
    public virtual TipoGaritum CodigoTipoGaritaNavigation { get; set; } = null!;

    [InverseProperty("CodigoGaritaSeguridadNavigation")]
    public virtual ICollection<MarcajeLaboral> MarcajeLaborals { get; set; } = new List<MarcajeLaboral>();

    [InverseProperty("CodigoGaritaSeguridadNavigation")]
    public virtual ICollection<RegistroMovimientoResidencial> RegistroMovimientoResidencials { get; set; } = new List<RegistroMovimientoResidencial>();
}
