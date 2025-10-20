using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebDBFinal.Entities;

[PrimaryKey("CodigoLinea", "CodigoMarca")]
[Table("LineaVehiculo")]
public partial class LineaVehiculo
{
    [Key]
    public int CodigoLinea { get; set; }

    [Key]
    public int CodigoMarca { get; set; }

    [StringLength(100)]
    [Unicode(false)]
    public string Descripcion { get; set; } = null!;

    [ForeignKey("CodigoMarca")]
    [InverseProperty("LineaVehiculos")]
    public virtual MarcaVehiculo CodigoMarcaNavigation { get; set; } = null!;

    [InverseProperty("LineaVehiculo")]
    public virtual ICollection<Vehiculo> Vehiculos { get; set; } = new List<Vehiculo>();
}
