using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebDBFinal.Entities;

public partial class TipoMultum
{
    [Key]
    public byte CodigoTipoMulta { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string Descripcion { get; set; } = null!;

    [Column(TypeName = "money")]
    public decimal ValorMulta { get; set; }

    [InverseProperty("CodigoTipoMultaNavigation")]
    public virtual ICollection<Multum> Multa { get; set; } = new List<Multum>();
}
