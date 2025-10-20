using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebDBFinal.Entities;

[Table("Vehiculo")]
[Index("Placa", Name = "UK_Vehiculo_Placa", IsUnique = true)]
public partial class Vehiculo
{
    [Key]
    public int CodigoVehiculo { get; set; }

    public int CodigoLinea { get; set; }

    public int CodigoMarca { get; set; }

    public int Anio { get; set; }

    [StringLength(30)]
    [Unicode(false)]
    public string Color { get; set; } = null!;

    [StringLength(10)]
    [Unicode(false)]
    public string Placa { get; set; } = null!;

    [ForeignKey("CodigoLinea, CodigoMarca")]
    [InverseProperty("Vehiculos")]
    public virtual LineaVehiculo LineaVehiculo { get; set; } = null!;

    [InverseProperty("CodigoVehiculoNavigation")]
    public virtual ICollection<RegistroMovimientoResidencial> RegistroMovimientoResidencials { get; set; } = new List<RegistroMovimientoResidencial>();

    [InverseProperty("CodigoVehiculoNavigation")]
    public virtual ICollection<RegistroVehiculoNoPermitido> RegistroVehiculoNoPermitidos { get; set; } = new List<RegistroVehiculoNoPermitido>();

    [InverseProperty("CodigoVehiculoNavigation")]
    public virtual ICollection<RegistroVehiculoResidente> RegistroVehiculoResidentes { get; set; } = new List<RegistroVehiculoResidente>();
}
