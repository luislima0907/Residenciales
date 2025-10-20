using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebDBFinal.Entities;

[Table("TipoRol")]
public partial class TipoRol
{
    [Key]
    public byte CodigoTipoRol { get; set; }

    [StringLength(20)]
    [Unicode(false)]
    public string Descripcion { get; set; } = null!;

    [InverseProperty("CodigoTipoRolNavigation")]
    public virtual ICollection<PersonaRol> PersonaRols { get; set; } = new List<PersonaRol>();
}
