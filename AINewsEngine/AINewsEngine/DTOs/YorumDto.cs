namespace AINewsEngine.DTOs
{
    public class YorumDto
    {
        public int Id { get; set; }
        public string Icerik { get; set; } = string.Empty;
        public DateTime OlusturmaTarihi { get; set; }
        public bool Onaylandi { get; set; }
        public int HaberId { get; set; }
        public string KullaniciId { get; set; } = string.Empty;
        public string KullaniciAdi { get; set; } = string.Empty;
        public int LikeSayisi { get; set; }
        public int DislikeSayisi { get; set; }
        public int YanitSayisi { get; set; }
        public bool? KullaniciLikeDurumu { get; set; } // null: hiç işlem yapmamış, true: like, false: dislike
        public List<YorumYanitiDto> Yanitlar { get; set; } = new List<YorumYanitiDto>();
    }

    public class YorumYanitiDto
    {
        public int Id { get; set; }
        public string Icerik { get; set; } = string.Empty;
        public DateTime OlusturmaTarihi { get; set; }
        public bool Onaylandi { get; set; }
        public int YorumId { get; set; }
        public string KullaniciId { get; set; } = string.Empty;
        public string KullaniciAdi { get; set; } = string.Empty;
        public int LikeSayisi { get; set; }
        public int DislikeSayisi { get; set; }
        public bool? KullaniciLikeDurumu { get; set; }
    }

    public class YorumEkleDto
    {
        public string Icerik { get; set; } = string.Empty;
        public int HaberId { get; set; }
    }

    public class YorumYanitiEkleDto
    {
        public string Icerik { get; set; } = string.Empty;
        public int YorumId { get; set; }
    }

    public class YorumLikeDto
    {
        public bool IsLike { get; set; } // true = Like, false = Dislike
    }
}
