using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebDBFinal.Entities;

[Table("TipoMarcaje")]
public partial class TipoMarcaje
{
    [Key]
    public byte CodigoTipoMarcaje { get; set; }

    [StringLength(30)]
    [Unicode(false)]
    public string Descripcion { get; set; } = null!;

    [InverseProperty("CodigoTipoMarcajeNavigation")]
    public virtual ICollection<MarcajeLaboral> MarcajeLaborals { get; set; } = new List<MarcajeLaboral>();
}
