using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebDBFinal.Entities;

[Table("Sector")]
public partial class Sector
{
    [Key]
    public int CodigoSector { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string Nombre { get; set; } = null!;

    [StringLength(100)]
    [Unicode(false)]
    public string Descripcion { get; set; } = null!;

    public short CantidadDeCasas { get; set; }

    [InverseProperty("CodigoSectorNavigation")]
    public virtual ICollection<Cluster> Clusters { get; set; } = new List<Cluster>();
}
