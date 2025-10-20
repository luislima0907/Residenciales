using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebDBFinal.Entities;

[Table("EmpresaSeguridad")]
public partial class EmpresaSeguridad
{
    [Key]
    public int CodigoEmpresa { get; set; }

    [StringLength(100)]
    [Unicode(false)]
    public string RazonSocial { get; set; } = null!;

    [StringLength(15)]
    [Unicode(false)]
    public string? NIT { get; set; }

    [StringLength(30)]
    [Unicode(false)]
    public string ContactoPrincipal { get; set; } = null!;

    [InverseProperty("CodigoEmpresaNavigation")]
    public virtual ICollection<Sucursal> Sucursals { get; set; } = new List<Sucursal>();
}
