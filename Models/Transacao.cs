using SQLite;
using System;

namespace GastoSmart.Models;

public class Transacao
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Indexed]
    public int PerfilId { get; set; }

    public string Descricao { get; set; } = string.Empty;

    public decimal Valor { get; set; }

    public string Categoria { get; set; } = string.Empty;

    public DateTime Data { get; set; }

    public string CaminhoImagem { get; set; } = string.Empty;
}
