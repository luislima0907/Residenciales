using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebDBFinal.Entities;

[Table("MarcaVehiculo")]
public partial class MarcaVehiculo
{
    [Key]
    public int CodigoMarca { get; set; }

    [StringLength(80)]
    [Unicode(false)]
    public string Descripcion { get; set; } = null!;

    [InverseProperty("CodigoMarcaNavigation")]
    public virtual ICollection<LineaVehiculo> LineaVehiculos { get; set; } = new List<LineaVehiculo>();
}
