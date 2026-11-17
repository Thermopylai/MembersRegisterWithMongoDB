# Luokat ja komponentit

[Dokumentaation alkuun](README.md)

## Sovelluksen käynnistys

| Tiedosto / luokka | Käyttötarkoitus |
| --- | --- |
| `MauiProgram.cs` / `MauiProgram` | `CreateMauiApp` määrittää MAUI-sovelluksen, fontin ja BlazorWebView'n sekä rekisteröi `MemberService`-palvelun. Debug-käännöksessä otetaan käyttöön myös Blazorin kehittäjätyökalut ja debug-lokitus. |
| `App.xaml` ja `App.xaml.cs` / `App` | MAUI-sovelluksen yhteinen määrittely. `CreateWindow` luo ikkunan, jossa näytetään `MainPage`. |
| `MainPage.xaml` ja `MainPage.xaml.cs` / `MainPage` | MAUI-sivu, jonka XAML sisältää BlazorWebView'n. C#-rakentaja alustaa XAMLissa määritetyt osat. |
| `Platforms\Windows\App.xaml.cs` / `WinUI.App` | Windowsin alustakohtainen MAUI-sovellusluokka. Se välittää sovelluksen luonnin `MauiProgram`-luokalle. Se on eri luokka kuin yhteinen `App`. |
| `Platforms\Android\MainActivity.cs` / `MainActivity` | Androidin käynnistettävä activity eli käyttöjärjestelmän hallinnoima sovellusnäkymä. |
| `Platforms\Android\MainApplication.cs` / `MainApplication` | Androidin sovellusluokka; se kutsuu yhteistä `MauiProgram`-alustusta. |
| `Platforms\iOS` ja `Platforms\MacCatalyst` / `Program`, `AppDelegate`, `SceneDelegate` | Kummallakin Apple-alustalla `Program` käynnistää sovelluksen, `AppDelegate` luo MAUI-sovelluksen ja `SceneDelegate` osallistuu alustan näkymän elinkaareen. |

Projektin `.csproj` määrittelee kohdealustat, MAUI-asetukset ja NuGet-riippuvuudet. `Properties\launchSettings.json` sisältää Windows-käynnistysprofiilin. Kohdealustan valinta Visual Studiossa vaikuttaa siihen, mitä versiota rakennetaan ja käynnistetään.

### Riippuvuusinjektio käytännössä

`MauiProgram` sisältää rekisteröinnin `builder.Services.AddSingleton<MemberService>()`. Singleton tarkoittaa, että saman palvelusäiliön pyynnöt saavat saman `MemberService`-olion. `Home.razor` pyytää sen käyttöönsä rivillä `@inject MemberService Members`.

Näin sivun ei tarvitse itse luoda palvelua. Palvelu säilyttää aktiivisen MongoDB-asiakkaan ja kokoelmaviitteen. Se toteuttaa `IDisposable`-rajapinnan, jotta sen hallitsema asiakas voidaan vapauttaa palvelun elinkaaren päättyessä. Onnistuneessa yhteyden vaihdossa vanha asiakas vapautetaan; epäonnistunut uusi asiakas vapautetaan, ja mahdollinen vanha yhteys jää käyttöön.

## `Member`: yhden jäsenen tiedot

`Models\Member.cs` sisältää `Member`-luokan. Sen ominaisuudet vastaavat lomakkeen kenttiä:

| Ominaisuus | Käyttöliittymässä | Huomio |
| --- | --- | --- |
| `Id` | Ei näytetä lomakkeessa | MongoDB-dokumentin yksilöivä tunniste. C#:ssa merkkijono, tietokannassa `ObjectId`. |
| `FirstName` | Etunimi | Pakollinen; enintään 100 merkkiä. |
| `LastName` | Sukunimi | Pakollinen; enintään 100 merkkiä. |
| `Address` | Osoite | Pakollinen; enintään 200 merkkiä. |
| `PostalCode` | Postinumero | Pakollinen; enintään 20 merkkiä. Merkkijono säilyttää esimerkiksi alussa olevat nollat. |
| `Phone` | Puhelin | Pakollinen; enintään 40 merkkiä. Merkkijono voi sisältää esimerkiksi `+`-merkin. |
| `Email` | Sähköposti | Pakollinen; enintään 254 merkkiä, ja sähköpostin muoto tarkistetaan. |
| `MembershipStartDate` | Jäsenyyden alkamispäivä | Pakollinen lomakkeella; uuden olion oletusarvo on kuluva päivä. |

`[Required]`, `[StringLength]` ja `[EmailAddress]` ovat **attribuutteja**, eli ominaisuuksiin liitettyjä lisätietoja. Blazorin `DataAnnotationsValidator` käyttää niitä lomakkeen tarkistamiseen. Merkintä `DateTime?` sallii C#:ssa myös `null`-arvon, vaikka lomakkeen `[Required]` estää tyhjän päivämäärän hyväksymisen.

`[BsonId]`, `[BsonRepresentation]` ja `[BsonDateTimeOptions]` ohjaavat MongoDB-ajurin muunnosta C#-olion ja dokumentin välillä. `[BsonIgnoreExtraElements]` sallii dokumentin lukemisen, vaikka siinä olisi mallille tuntemattomia lisäkenttiä. `Copy()` tekee jäsenestä kopion muokkausta varten: muokkaaminen voidaan peruuttaa muuttamatta suoraan taulukossa näkyvää oliota. Nykyiset ominaisuudet ovat arvoja tai merkkijonoja; kyseessä on pinnallinen kopio, mikä on huomioitava, jos malliin lisätään myöhemmin muokattavia olioita.

## `MemberService`: tietokantapalvelu

`Services\MemberService.cs` erottaa MongoDB-ajurin käytön käyttöliittymästä.

| Metodi | Tehtävä |
| --- | --- |
| `ConnectAsync` | Luo asiakkaan annetusta yhteysosoitteesta, hakee jäsenet ja vaihtaa aktiivisen yhteyden vasta onnistuneen haun jälkeen. |
| `GetAllAsync` | Hakee jäsenet aktiivisesta kokoelmasta. |
| `AddAsync` | Lisää yhden `Member`-olion. |
| `UpdateAsync` | Päivittää jäsenen kentät tunnisteen perusteella. Palauttaa `true`, jos dokumentti löytyi. |
| `DeleteAsync` | Poistaa jäsenen tunnisteen perusteella. Palauttaa `true`, jos dokumentti poistettiin. |
| `GetCollection` | Sisäinen apumetodi, joka palauttaa aktiivisen kokoelman tai ilmoittaa, ettei yhteyttä ole muodostettu. |
| `ReadAsync` | Sisäinen apumetodi, joka lukee kaikki jäsenet ja järjestää heidät suku- ja etunimen mukaan. |
| `Dispose` | Vapauttaa aktiivisen MongoDB-asiakkaan. |

Palvelu ei itse näytä virheilmoituksia: se välittää tietokantavirheet kutsujalle. Lomakevalidointi tapahtuu käyttöliittymässä, ei palvelun metodeissa tai MongoDB:n määrittelemänä skeemavalidointina.

## Blazor-komponentit

| Tiedosto | Tehtävä |
| --- | --- |
| `Components\Routes.razor` | Valitsee osoitetta vastaavan komponentin ja oletusasettelun. |
| `Components\Layout\MainLayout.razor` | Määrittää sivujen yhteisen ulkokehyksen. `@Body` on valitun sivun sisältö. |
| `Components\Pages\Home.razor` | Jäsenrekisterin päänäkymä, lomake, taulukko, tilamuuttujat ja painikkeiden tapahtumankäsittelijät. |
| `Components\Pages\NotFound.razor` | Ilmoittaa, jos sivua ei löydy, ja tarjoaa linkin takaisin rekisteriin. |
| `Components\_Imports.razor` | Määrittää komponenttien yhteisiä `using`-nimiavaruuksia. |
| `wwwroot\index.html` ja `wwwroot\app.css` | HTML-alkusivu ja sovelluksen yhteiset tyylit. |

### `Home`-komponentin tila

`Home.razor` yhdistää näkymän ja sen tapahtumakoodin. `members` sisältää viimeksi haetun jäsenlistan. `editing` on uusi jäsen tai muokattavan jäsenen kopio; `adding` kertoo, onko kyse lisäyksestä. `deleting` sisältää poistettavaksi valitun jäsenen vahvistusta varten.

`connected` kertoo, että yhteyden muodostus on aiemmin onnistunut. Se ei ole jatkuva palvelimen yhteystilan tarkistus. `busy` poistaa toimintoja käytöstä operaation ajaksi. `error` ja `notice` sisältävät käyttäjälle näytettävät viestit. `requiresRefresh` estää uusia muutoksia, kun listan tila tai kirjoituksen tulos pitää ensin selvittää päivittämällä lista.

Komponentin `ConnectAsync`, `RefreshAsync`, `SaveAsync` ja `DeleteAsync` käsittelevät käyttäjän toimintoja. `NewMember`, `EditMember`, `CancelEdit`, `ConfirmDelete` ja `CancelDelete` muuttavat lomakkeen ja vahvistuksen tilaa. `ReloadAfterChangeAsync` hakee listan kirjoituksen jälkeen, ja `IsDatabaseError` rajaa ne virheet, joille näytetään tietokantaan liittyvä virheilmoitus.

Seuraavaksi: [MongoDB ja tietokantayhteys](mongodb.md).