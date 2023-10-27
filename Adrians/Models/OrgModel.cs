namespace Adrians.Models
{
    public class OrgModel
    {
        public string? organisasjonsnummer { get; set; }
        public string? navn { get; set; }
        public string? registreringsdatoEnhetsregisteret { get; set; }
        public bool? registrertIMvaregisteret { get; set; }
        public int? antallAnsatte { get; set; }
        public bool? registrertIForetaksregisteret { get; set; }
        public bool? registrertIStiftelsesregisteret { get; set; }
        public bool? registrertIFrivillighetsregisteret { get; set; }
        public bool? konkurs { get; set; }
        public bool? underAvvikling { get; set; }
        public bool? underTvangsavviklingEllerTvangsopplosning { get; set; }
        public string? maalform { get; set; }
    }
}
