using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebDBFinal.Entities;

[PrimaryKey("CodigoCluster", "CodigoSucursal", "CodigoSector")]
[Table("Cluster")]
public partial class Cluster
{
    [Key]
    public int CodigoCluster { get; set; }

    [Key]
    public int CodigoSucursal { get; set; }

    [Key]
    public int CodigoSector { get; set; }

    [InverseProperty("Cluster")]
    public virtual ICollection<Casa> Casas { get; set; } = new List<Casa>();

    [ForeignKey("CodigoSector")]
    [InverseProperty("Clusters")]
    public virtual Sector CodigoSectorNavigation { get; set; } = null!;

    [ForeignKey("CodigoSucursal")]
    [InverseProperty("Clusters")]
    public virtual Sucursal CodigoSucursalNavigation { get; set; } = null!;

    [InverseProperty("Cluster")]
    public virtual ICollection<GaritaSeguridad> GaritaSeguridads { get; set; } = new List<GaritaSeguridad>();

    [InverseProperty("Cluster")]
    public virtual ICollection<JuntaDirectiva> JuntaDirectivas { get; set; } = new List<JuntaDirectiva>();
}
