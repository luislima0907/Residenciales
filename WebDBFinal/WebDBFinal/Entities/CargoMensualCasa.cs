using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.InteropServices.JavaScript;
using Microsoft.EntityFrameworkCore;

namespace WebDBFinal.Entities;

[Table("CargoMensualCasa")]
[Index("NumeroCasa", "CodigoCluster", "CodigoSucursal", "CodigoSector", "MesCargo", "AnioCargo", Name = "UK_CargoMensualCasa_CasaMesAnio", IsUnique = true)]
public partial class CargoMensualCasa
{
    [Key]
    public int CodigoCargoMensual { get; set; }

    public int NumeroCasa { get; set; }

    public int CodigoCluster { get; set; }

    public int CodigoSucursal { get; set; }

    public int CodigoSector { get; set; }

    public byte MesCargo { get; set; }

    public int AnioCargo { get; set; }

    [Column(TypeName = "date")]
    public DateTime FechaVencimiento { get; set; }
    
    [Column(TypeName = "money")]
    public decimal MontoTotal { get; set; }

    public bool Pagado { get; set; }

    [ForeignKey("NumeroCasa, CodigoCluster, CodigoSucursal, CodigoSector")]
    [InverseProperty("CargoMensualCasas")]
    public virtual Casa Casa { get; set; } = null!;

    [InverseProperty("CodigoCargoMensualNavigation")]
    public virtual ICollection<DetalleCargoMensual> DetalleCargoMensuals { get; set; } = new List<DetalleCargoMensual>();
}
