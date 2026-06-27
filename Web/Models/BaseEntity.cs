namespace Web.Models
{
    // Zajednička osnova za sve entitete.
    // Id  -> jedinstveni identifikator zapisa u JSON datoteci
    // IsDeleted -> logičko (soft) brisanje: entitet ostaje u datoteci,
    //              ali se ne prikazuje korisnicima (zahtev iz specifikacije)
    public abstract class BaseEntity
    {
        public int Id { get; set; }
        public bool IsDeleted { get; set; }
    }
}
