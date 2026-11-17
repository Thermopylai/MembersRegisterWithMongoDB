using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MembersRegisterWithMongoDB.Models;

[BsonIgnoreExtraElements]
public class Member
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [Required(ErrorMessage = "Anna etunimi.")]
    [StringLength(100, ErrorMessage = "Etunimi voi olla enintään 100 merkkiä.")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Anna sukunimi.")]
    [StringLength(100, ErrorMessage = "Sukunimi voi olla enintään 100 merkkiä.")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Anna osoite.")]
    [StringLength(200, ErrorMessage = "Osoite voi olla enintään 200 merkkiä.")]
    public string Address { get; set; } = string.Empty;

    [Required(ErrorMessage = "Anna postinumero.")]
    [StringLength(20, ErrorMessage = "Postinumero voi olla enintään 20 merkkiä.")]
    public string PostalCode { get; set; } = string.Empty;

    [Required(ErrorMessage = "Anna puhelinnumero.")]
    [StringLength(40, ErrorMessage = "Puhelinnumero voi olla enintään 40 merkkiä.")]
    public string Phone { get; set; } = string.Empty;

    [Required(ErrorMessage = "Anna sähköpostiosoite.")]
    [EmailAddress(ErrorMessage = "Anna kelvollinen sähköpostiosoite.")]
    [StringLength(254, ErrorMessage = "Sähköpostiosoite voi olla enintään 254 merkkiä.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Anna jäsenyyden alkamispäivä.")]
    [BsonDateTimeOptions(DateOnly = true)]
    public DateTime? MembershipStartDate { get; set; } = DateTime.Today;

    public Member Copy() => (Member)MemberwiseClone();
}