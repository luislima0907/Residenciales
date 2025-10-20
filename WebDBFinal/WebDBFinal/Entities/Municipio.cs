using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebDBFinal.Entities;

[PrimaryKey("CodigoMunicipio", "CodigoDepartamento")]
[Table("Municipio")]
public partial class Municipio
{
    [Key]
    public short CodigoMunicipio { get; set; }

    [Key]
    public short CodigoDepartamento { get; set; }

    [StringLength(100)]
    [Unicode(false)]
    public string Descripcion { get; set; } = null!;

    [ForeignKey("CodigoDepartamento")]
    [InverseProperty("Municipios")]
    public virtual Departamento CodigoDepartamentoNavigation { get; set; } = null!;

    [InverseProperty("Municipio")]
    public virtual ICollection<DireccionSucursal> DireccionSucursals { get; set; } = new List<DireccionSucursal>();
}
