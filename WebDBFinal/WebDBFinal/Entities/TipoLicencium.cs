using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebDBFinal.Entities;

public partial class TipoLicencium
{
    [Key]
    public byte CodigoTipoLicencia { get; set; }

    [StringLength(1)]
    [Unicode(false)]
    public string TipoLicencia { get; set; } = null!;

    [StringLength(50)]
    [Unicode(false)]
    public string Descripcion { get; set; } = null!;

    [InverseProperty("CodigoTipoLicenciaNavigation")]
    public virtual ICollection<Licencium> Licencia { get; set; } = new List<Licencium>();
}
