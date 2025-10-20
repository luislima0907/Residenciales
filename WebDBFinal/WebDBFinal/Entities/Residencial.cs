using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebDBFinal.Entities;

[Table("Residencial")]
public partial class Residencial
{
    [Key]
    public int CodigoResidencial { get; set; }

    [StringLength(100)]
    [Unicode(false)]
    public string Nombre { get; set; } = null!;

    [StringLength(30)]
    [Unicode(false)]
    public string ContactoPrincipal { get; set; } = null!;

    [InverseProperty("CodigoResidencialNavigation")]
    public virtual ICollection<Sucursal> Sucursals { get; set; } = new List<Sucursal>();
}
