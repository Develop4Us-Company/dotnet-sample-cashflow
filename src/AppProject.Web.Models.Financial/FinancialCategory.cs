using System;
using AppProject.Models;

namespace AppProject.Web.Models.Financial;

public class FinancialCategory : ObservableModel, IEntity
{
    private Guid? id;
    private string name = default!;
    private byte[]? rowVersion;

    public Guid? Id { get => this.id; set => this.Set(ref this.id, value); }

    public string Name { get => this.name; set => this.Set(ref this.name, value); }

    public byte[]? RowVersion { get => this.rowVersion; set => this.Set(ref this.rowVersion, value); }
}
