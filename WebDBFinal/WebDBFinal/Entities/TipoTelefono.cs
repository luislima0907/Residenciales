using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebDBFinal.Entities;

[Table("TipoTelefono")]
public partial class TipoTelefono
{
    [Key]
    public byte CodigoTipoTelefono { get; set; }

    [StringLength(30)]
    [Unicode(false)]
    public string Descripcion { get; set; } = null!;

    [InverseProperty("CodigoTipoTelefonoNavigation")]
    public virtual ICollection<PersonaTelefono> PersonaTelefonos { get; set; } = new List<PersonaTelefono>();

    [InverseProperty("CodigoTipoTelefonoNavigation")]
    public virtual ICollection<TelefonoSucursal> TelefonoSucursals { get; set; } = new List<TelefonoSucursal>();
}
