using System.Collections.Generic;

namespace AbiaApp.API.Services // <--- Ajustado
{
    public class ItemDominio
    {
        public int Id { get; set; }
        public string Nome { get; set; }
    }

    public static class CacheSistema
    {
        public static List<ItemDominio> Grupos { get; set; } = new();
        public static List<ItemDominio> Marcas { get; set; } = new();
        public static List<ItemDominio> Generos { get; set; } = new();
        public static List<ItemDominio> Perfis { get; set; } = new();
    }
}