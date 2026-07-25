using SQLite;
using System;

namespace GastoSmart.Models;

public class PerfilUsuario
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public string Nome { get; set; } = string.Empty;

    public string SenhaPinHash { get; set; } = string.Empty;

    public string Cor { get; set; } = "#512BD4"; // Default MAUI purple

    public DateTime DataCriacao { get; set; }
}
