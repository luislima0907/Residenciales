using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebDBFinal.Entities;

[PrimaryKey("CodigoDireccionSucursal", "CodigoSucursal")]
[Table("DireccionSucursal")]
public partial class DireccionSucursal
{
    [Key]
    public int CodigoDireccionSucursal { get; set; }

    [Key]
    public int CodigoSucursal { get; set; }

    [StringLength(30)]
    [Unicode(false)]
    public string Calle { get; set; } = null!;

    [StringLength(30)]
    [Unicode(false)]
    public string Avenida { get; set; } = null!;

    public short Zona { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string Ciudad { get; set; } = null!;

    public short CodigoMunicipio { get; set; }

    public short CodigoDepartamento { get; set; }

    [ForeignKey("CodigoSucursal")]
    [InverseProperty("DireccionSucursals")]
    public virtual Sucursal CodigoSucursalNavigation { get; set; } = null!;

    [ForeignKey("CodigoMunicipio, CodigoDepartamento")]
    [InverseProperty("DireccionSucursals")]
    public virtual Municipio Municipio { get; set; } = null!;
}
