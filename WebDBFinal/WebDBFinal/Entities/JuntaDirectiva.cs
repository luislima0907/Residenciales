using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebDBFinal.Entities;

[PrimaryKey("CodigoJunta", "CodigoCluster", "CodigoSucursal", "CodigoSector")]
[Table("JuntaDirectiva")]
public partial class JuntaDirectiva
{
    [Key]
    public int CodigoJunta { get; set; }

    [Key]
    public int CodigoCluster { get; set; }

    [Key]
    public int CodigoSucursal { get; set; }

    [Key]
    public int CodigoSector { get; set; }

    public DateOnly FechaInicio { get; set; }

    public DateOnly FechaFin { get; set; }

    [StringLength(100)]
    [Unicode(false)]
    public string Descripcion { get; set; } = null!;

    [ForeignKey("CodigoCluster, CodigoSucursal, CodigoSector")]
    [InverseProperty("JuntaDirectivas")]
    public virtual Cluster Cluster { get; set; } = null!;

    [InverseProperty("JuntaDirectiva")]
    public virtual ICollection<IntegranteJuntum> IntegranteJunta { get; set; } = new List<IntegranteJuntum>();
}
