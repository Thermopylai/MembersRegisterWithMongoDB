# MongoDB ja tietokantayhteys

[Dokumentaation alkuun](README.md)

## Tietokannan rakenne

MongoDB:ssä tietokanta sisältää **kokoelmia**, ja kokoelmat sisältävät **dokumentteja**. Tässä sovelluksessa:

```text
MongoDB-palvelin
  └── jasenrekisteri       tietokanta
       └── jasenet         kokoelma
            ├── jäsenen dokumentti
            └── toisen jäsenen dokumentti
```

Nimet `jasenrekisteri` ja `jasenet` on määritelty `MemberService.ConnectAsync`-metodissa. Kokoelman ja tietokannan käsittely ei edellytä niiden luomista käsin: MongoDB luo ne tarvittaessa ensimmäisen tallennuksen yhteydessä. Pelkkä yhteyden muodostus ja tyhjän kokoelman lukeminen ei tarkoita, että tietokanta olisi jo pysyvästi luotu.

Seuraava on havainnollistava esimerkki dokumentista MongoDB Shellin syntaksilla, ei oikean henkilön tietoja:

```javascript
{
  _id: ObjectId("65a000000000000000000001"),
  FirstName: "Malli",
  LastName: "Jäsen",
  Address: "Esimerkkitie 1",
  PostalCode: "00100",
  Phone: "+358 40 000 0000",
  Email: "malli@example.invalid",
  MembershipStartDate: ISODate("2024-01-15T00:00:00Z")
}
```

Kenttien nimet tulevat `Member`-luokan ominaisuuksista. MongoDB tallentaa tunnisteen `_id`-kenttään `ObjectId`-tyyppisenä; C#-koodissa sitä käsitellään `Id`-merkkijonona BSON-attribuuttien avulla. Kun uusi jäsen lisätään ilman tunnistetta, ajuri luo sille tunnisteen. Alkamispäivä käsitellään päivämääränä `[BsonDateTimeOptions(DateOnly = true)]` -asetuksella. Käyttöliittymä näyttää sen muodossa `pp.kk.vvvv`.

## Yhteysosoite

Yhteysosoite eli *connection string* kertoo MongoDB-ajurille, mihin palvelimeen yhdistetään ja millaisia yhteysasetuksia käytetään. Esimerkiksi:

```text
mongodb://localhost:27017
```

`mongodb://` kertoo osoitteen tyypin, `localhost` viittaa siihen koneeseen tai ympäristöön, jossa sovellus suoritetaan, ja `27017` on MongoDB:n tavallinen portti. Osoite voi sisältää myös tunnistukseen ja salattuun yhteyteen liittyviä asetuksia. Älä laita oikeita tunnuksia esimerkkeihin, lähdekoodiin tai versionhallintaan.

| Suoritusympäristö | Sovelluksen oletusosoite | Huomio |
| --- | --- | --- |
| Windows ja muut kuin Android | `mongodb://localhost:27017` | `localhost` tarkoittaa sovelluksen omaa suoritusympäristöä, ei automaattisesti jotakin toista konetta. |
| Android | `mongodb://10.0.2.2:27017` | Tarkoitettu Android-emulaattorille, jossa `10.0.2.2` viittaa kehityskoneeseen. Fyysisellä Android-laitteella on annettava sille sopiva palvelinosoite. |

Palvelimen on oltava kyseisestä ympäristöstä saavutettavissa ja käyttöoikeuksien on oltava kunnossa. Yhteysosoite syötetään päänäkymässä ja sitä käytetään vain nykyisen sovelluskäynnistyksen ajan; sovellus ei tallenna sitä asetustiedostoon. Jos osoite vaihdetaan, valitaan **Yhdistä**. Tietokannan ja kokoelman nimet tulevat silti palvelun koodista, eivät osoitekentästä.

## Mitä `ConnectAsync` tekee?

1. `MongoClientSettings.FromConnectionString` tulkitsee annetun osoitteen.
2. `ServerSelectionTimeout` asetetaan viiteen sekuntiin. Tämä rajoittaa aikaa, jonka ajuri odottaa sopivan palvelimen valintaa; se ei ole kaikkien mahdollisten tietokantatoimintojen kokonaisaikaraja.
3. Luodaan uusi `MongoClient`, jonka kautta saadaan viite tietokantaan ja sen `IMongoCollection<Member>`-kokoelmaan.
4. `ReadAsync` yrittää hakea jäsenet. Näin yhteyttä kokeillaan oikealla lukuoperaatiolla eikä pelkän asiakasolion luomisen perusteella.
5. Vasta onnistunut haku vaihtaa uuden asiakkaan ja kokoelman aktiivisiksi. Aiempi asiakas vapautetaan.
6. Jos uuden yhteyden muodostus epäonnistuu, uusi asiakas vapautetaan `finally`-lohkossa. Jos aiempi yhteys oli olemassa, se jää käyttöön ja käyttöliittymä ilmoittaa asiasta.

MongoDB-ajuri hallitsee asiakkaan käyttämiä yhteyksiä. Siksi palvelu säilyttää aktiivisen `MongoClient`-olion eikä luo uutta jokaiselle lisäys- tai lukukutsulle. Palvelin voi kuitenkin tulla myöhemmin saavuttamattomaksi; aiemmin onnistunut yhteys ei takaa seuraavan operaation onnistumista.

## CRUD käytännössä

| Käyttäjän toiminto | `MemberService` | Ajurin operaatio ja tulos |
| --- | --- | --- |
| Lisää jäsen | `AddAsync` | `InsertOneAsync` lisää dokumentin. |
| Lue jäsenet | `GetAllAsync` ja `ReadAsync` | `Find` hakee kaikki dokumentit, järjestää ne sukunimen ja etunimen mukaan ja `ToListAsync` palauttaa listan. |
| Muokkaa | `UpdateAsync` | `UpdateOneAsync` etsii dokumentin `Id`-tunnisteella ja asettaa mallin jäsenkentät. `MatchedCount` kertoo, löytyikö dokumentti. |
| Poista | `DeleteAsync` | `DeleteOneAsync` etsii dokumentin tunnisteella. `DeletedCount` kertoo, poistettiinko dokumentti. |

Muokkaus asettaa erikseen jäsenen tunnetut kentät eikä korvaa koko dokumenttia. Näin tietokannassa mahdollisesti olevat muut kentät säilyvät. `UpdateAsync` palauttaa `true` myös silloin, kun dokumentti löytyi mutta kenttien arvot olivat jo samat: tarkistus perustuu `MatchedCount`-arvoon, ei muuttuneiden dokumenttien määrään.

Esimerkki lukemisen ajatuksesta: `Find(FilterDefinition<Member>.Empty)` tarkoittaa, ettei hakua rajata tiettyyn jäseneen. Tässä toteutuksessa koko lista haetaan kerralla, eikä sivutusta tai hakutoimintoa ole. Tämä on yksinkertainen ratkaisu pienelle rekisterille, mutta suurilla tietomäärillä sitä olisi kehitettävä.

## Virheet ja epävarma kirjoitustulos

Tietokantaoperaatio voi epäonnistua esimerkiksi väärän osoitteen, puuttuvien oikeuksien tai verkkokatkon vuoksi. Käyttöliittymä näyttää käsittelemistään tietokantavirheistä suomenkielisen ilmoituksen. Osoitteen vaihtaminen ei automaattisesti vaihda yhteyttä: käyttäjän on painettava **Yhdistä**.

Verkkokatko voi tapahtua myös sen jälkeen, kun palvelin on ehtinyt tallentaa muutoksen mutta ennen kuin sovellus saa vahvistuksen. Tällöin pelkästä virheestä ei voi päätellä varmasti, tapahtuiko kirjoitus. Käyttöliittymä estää uusia muutoksia, kunnes käyttäjä peruuttaa keskeneräisen toiminnon ja valitsee **Päivitä lista**. Näin tietoja voi tarkistaa ennen uutta yritystä.

Jos muokattavaa tai poistettavaa dokumenttia ei enää löydy, palvelu palauttaa `false` ja käyttöliittymä ilmoittaa tilanteesta. Samaan jäseneen samanaikaisesti tehdyt onnistuneet muokkaukset eivät kuitenkaan saa erillistä ristiriidan tunnistusta: myöhempi päivitys voi korvata aiemman kenttäarvoja.

## Tietoturvan rajat

Sovellus yhdistää MongoDB:hen suoraan. Se ei sisällä omaa käyttäjähallintaa, välipalvelinta tai erillistä oikeuksien tarkistuskerrosta. Jokainen sovelluskäyttäjä toimii MongoDB-yhteysosoitteessa käytettävien tunnusten oikeuksilla. Lomakevalidointi parantaa syötteen laatua, mutta se ei yksin suojaa tietokantaa tai estä muuta asiakasohjelmaa kirjoittamasta erilaisia tietoja.

Kehityskoneen paikallinen, ilman tunnistusta toimiva yhteys ei sellaisenaan sovi tuotantokäyttöön. Henkilötietojen käsittely edellyttää muun muassa asianmukaisia käyttöoikeuksia, verkkorajoituksia, TLS-salausta, varmuuskopiointia ja organisaation tietosuojakäytäntöjen huomioimista. Suoraan asiakassovelluksessa käytettävät tietokantatunnukset ovat sovelluskäyttäjän saatavilla: tuotantoratkaisussa voi olla tarpeen toteuttaa erillinen palvelinrajapinta ja käyttäjäkohtainen valtuutus.

Seuraavaksi: [Käyttö ja jatkokehitys](kaytto-ja-jatkokehitys.md).