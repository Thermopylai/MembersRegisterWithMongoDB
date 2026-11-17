# Arkkitehtuuri ja tiedon kulku

[Dokumentaation alkuun](README.md)

## Mitä sovellus tekee?

Jäsenrekisterillä voidaan lisätä, tarkastella, muokata ja poistaa jäseniä. Jokaisesta jäsenestä tallennetaan etunimi, sukunimi, osoite, postinumero, puhelin, sähköposti ja jäsenyyden alkamispäivä. Tiedot säilyvät MongoDB:ssä sovelluksen sulkemisen jälkeen.

Sovellus on **MAUI Blazor Hybrid** -sovellus. MAUI luo eri alustoilla toimivan sovellusikkunan. Ikkunan sisällä `BlazorWebView` näyttää Blazorilla rakennetun käyttöliittymän. C#-koodi suoritetaan sovelluksessa, ja MongoDB-ajuri keskustelee tietokannan kanssa. Erillistä omaa HTTP-rajapintaa tai sovelluspalvelinta ei nykyisessä toteutuksessa ole.

```text
Käyttäjä
   |
   v
MAUI-ikkuna ja BlazorWebView
   |
   v
Home.razor: lomake, taulukko ja tapahtumat
   |                 |
   | käyttää         | käsittelee
   v                 v
MemberService      Member
   |
   | MongoDB.Driver
   v
MongoDB: jasenrekisteri / jasenet
```

Tämä on looginen kuva, ei luokkaperintähierarkia. `Member` kuvaa jäsenen tiedot, kun taas `MemberService` hoitaa tietokantaoperaatiot. Käyttöliittymä kutsuu palvelua eikä rakenna MongoDB-kyselyitä itse.

## Käynnistymisen vaiheet

1. Käyttöjärjestelmä käynnistää oman alustakoodinsa `Platforms`-kansiosta. Alustakohtaiset MAUI-luokat kutsuvat yhteistä `MauiProgram.CreateMauiApp`-metodia.
2. `MauiProgram` rakentaa sovelluksen, ottaa BlazorWebView'n käyttöön ja rekisteröi `MemberService`-palvelun riippuvuusinjektioon.
3. MAUI-sovelluksen `App` luo ikkunan, jonka sisältö on `MainPage`.
4. `MainPage.xaml` sisältää `BlazorWebView`-näkymän. Sen HTML-alkusivu on `wwwroot\index.html`, ja siihen liitetään `Routes`-komponentti.
5. `Routes.razor` valitsee osoitteeseen `/` `Home.razor`-komponentin ja käyttää sen ympärillä `MainLayout`-asettelua.
6. `Home` yrittää alustuksessaan muodostaa yhteyden oletusosoitteeseen ja hakea jäsenet. Mahdollinen yhteysvirhe näytetään käyttäjälle.

Käyttöjärjestelmäkohtainen käynnistys eroaa alustojen välillä, mutta sen jälkeen jäsenrekisterin käyttöliittymä, malli ja palvelu ovat yhteisiä.

## Esimerkki: uuden jäsenen tallennus

1. Käyttäjä valitsee **Lisää jäsen**. `Home` luo uuden `Member`-olion lomakkeen muokattavaksi malliksi.
2. Käyttäjän syötteet sidotaan mallin ominaisuuksiin `@bind-Value`-merkinnällä. Tallennettaessa Blazorin `EditForm` ja `DataAnnotationsValidator` tarkistavat mallin validointisäännöt.
3. Jos lomake on kelvollinen, `SaveAsync` poistaa tekstikenttien alusta ja lopusta välilyönnit ja kutsuu `MemberService.AddAsync`-metodia.
4. Palvelu kutsuu MongoDB-ajurin `InsertOneAsync`-metodia. Ajuri muuntaa `Member`-olion MongoDB-dokumentiksi ja lähettää sen palvelimelle.
5. Onnistuneen kirjoituksen jälkeen käyttöliittymä hakee jäsenlistan uudelleen ja näyttää ilmoituksen. Jos tallennuksen tulosta ei saada varmistettua, käyttäjää ohjataan peruuttamaan toiminto ja päivittämään lista ennen uutta yritystä.

Metodien nimissä esiintyvä `Async` viittaa asynkroniseen toimintaan. `await` odottaa tietokantaoperaation tulosta estämättä käyttöliittymäsäiettä samalla tavalla kuin pitkä synkroninen odotus. Operaatio voi silti kestää tai epäonnistua.

## Projektin keskeiset kansiot

| Polku | Sisältö |
| --- | --- |
| `MembersRegisterWithMongoDB\Models` | Jäsenen tietomalli ja sen validointisäännöt. |
| `MembersRegisterWithMongoDB\Services` | Tietokannan käsittelyyn tarkoitettu palvelu. |
| `MembersRegisterWithMongoDB\Components` | Blazor-reititys, asettelu ja sivut. |
| `MembersRegisterWithMongoDB\Platforms` | Alustakohtainen käynnistyskoodi ja asetukset. |
| `MembersRegisterWithMongoDB\Resources` | MAUI-resurssit, kuten kuvakkeet, kuvat ja fontit. |
| `MembersRegisterWithMongoDB\wwwroot` | BlazorWebView'n HTML-alkusivu, CSS ja muut verkkokäyttöliittymän resurssit. |
| `MembersRegisterWithMongoDB\bin` ja `obj` | Käännöksen tuottamia tiedostoja; niitä ei yleensä muokata käsin. |

Seuraavaksi: [Luokat ja komponentit](luokat-ja-komponentit.md).