namespace artgallery_server.Enum
{
    public static class ArtTypeExtensions
    {
        public static string ToDisplayName(this ArtType t) => t switch
        {
            ArtType.Unknown     => "Nieznany",
            ArtType.Painting    => "Malarstwo",
            ArtType.Drawing     => "Rysunek",
            ArtType.Sculpture   => "Rzeźba",
            ArtType.Print       => "Grafika",
            ArtType.Photography => "Fotografia",
            ArtType.Digital     => "Sztuka cyfrowa",
            ArtType.MixedMedia  => "Technika mieszana",
            ArtType.Video       => "Wideo",
            ArtType.Textile     => "Tkanina",
            ArtType.Ceramic     => "Ceramika",
            ArtType.Glass       => "Szkło",
            ArtType.Sound       => "Dźwięk",
            ArtType.Street      => "Street art",
            ArtType.Illustration=> "Ilustracja",
            _ => t.ToString()
        };

        public static string ToSlug(this ArtType t)
            => t.ToString().ToLowerInvariant();
    }
}
