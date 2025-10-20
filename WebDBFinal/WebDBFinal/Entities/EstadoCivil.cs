using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebDBFinal.Entities;

[Table("EstadoCivil")]
public partial class EstadoCivil
{
    [Key]
    public byte CodigoEstadoCivil { get; set; }

    [StringLength(20)]
    [Unicode(false)]
    public string Descripcion { get; set; } = null!;

    [InverseProperty("CodigoEstadoCivilNavigation")]
    public virtual ICollection<Persona> Personas { get; set; } = new List<Persona>();
}
