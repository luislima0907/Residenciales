using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebDBFinal.Entities;

[Table("RegistroVehiculoNoPermitido")]
public partial class RegistroVehiculoNoPermitido
{
    [Key]
    public int CodigoVehiculoNoPermitido { get; set; }

    public int CodigoVehiculo { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime FechaDeclaracion { get; set; }

    [StringLength(100)]
    [Unicode(false)]
    public string Motivo { get; set; } = null!;

    public bool? Estado { get; set; }

    [ForeignKey("CodigoVehiculo")]
    [InverseProperty("RegistroVehiculoNoPermitidos")]
    public virtual Vehiculo CodigoVehiculoNavigation { get; set; } = null!;
}
