namespace AINewsEngine.Service
{
    public interface ILlmService
    {
        Task<(string? YeniBaslik, string? YeniIcerik)> HaberiYenidenYaz(string orijinalBaslik, string orijinalIcerik);
    }
}