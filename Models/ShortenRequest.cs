namespace TinyUrlApi.Models;

public record ShortenRequest(string OriginalUrl, bool IsPrivate = false);