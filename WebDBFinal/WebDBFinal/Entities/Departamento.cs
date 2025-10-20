using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebDBFinal.Entities;

[Table("Departamento")]
public partial class Departamento
{
    [Key]
    public short CodigoDepartamento { get; set; }

    [StringLength(100)]
    [Unicode(false)]
    public string Descripcion { get; set; } = null!;

    [InverseProperty("CodigoDepartamentoNavigation")]
    public virtual ICollection<Municipio> Municipios { get; set; } = new List<Municipio>();
}
