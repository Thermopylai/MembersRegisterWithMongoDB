# Käyttö ja jatkokehitys

[Dokumentaation alkuun](README.md)

## Käynnistäminen Windowsissa

1. Asenna projektin kohdekehyksiä vastaava .NET SDK ja MAUI-työkuorma sekä käynnistä MongoDB-palvelin. Projektin `.csproj` kertoo käytetyt .NET-, MAUI- ja pakettiversiot.
2. Avaa ratkaisu Visual Studiossa ja aseta `MembersRegisterWithMongoDB` käynnistysprojektiksi.
3. Valitse Debug-painikkeen valikosta ensin **Framework → net11.0-windows10.0.26100.0** ja sen jälkeen **Windows Machine**.
4. Käynnistä sovellus. Se yrittää ensin yhdistää osoitteeseen `mongodb://localhost:27017`. Tarvittaessa syötä oikea yhteysosoite ja valitse **Yhdistä**.

Android-emulaattorin oletusosoite on `mongodb://10.0.2.2:27017`. Fyysisellä laitteella tarvitaan laitteen tavoittama palvelinosoite sekä asianmukaiset palvelimen ja verkon asetukset. Älä avaa tietokantapalvelinta julkiseen verkkoon vain yhteyden testaamiseksi.

## Käyttöliittymän kokeilu

Tee kokeilut vain testikäyttöön tarkoitetulla tietokannalla ja testitiedoilla. Sovelluksen tietokannan nimi on nykyisin kiinteä, joten varmista ensin, että antamasi yhteysosoite osoittaa turvalliseen testipalvelimeen eikä tuotantotietoihin.

1. Tarkista, että näkymässä lukee **Yhteys muodostettu**. Lisää jäsen ja täytä kaikki kentät. Kokeile esimerkiksi postinumeroa `00100`, jonka alkunollien tulee säilyä.
2. Tallenna ja varmista, että jäsen näkyy listassa oikeine tietoineen myös **Päivitä lista** -toiminnon jälkeen.
3. Avaa jäsenen muokkaus, muuta jokin kenttä ja tallenna. Tarkista muuttunut arvo listasta. Kokeile myös muokkausta, jonka peruutat.
4. Valitse **Poista**. Tarkista ensin, että vahvistuksen voi peruuttaa. Poista sen jälkeen testijäsen ja varmista, ettei se enää näy päivitetyssä listassa.
5. Kokeile tyhjää pakollista kenttää ja virheellistä sähköpostia: virheellistä lomaketta ei pitäisi voida tallentaa.
6. Kokeile tarvittaessa virheellistä yhteysosoitetta. Jos aiempi yhteys oli muodostettu, sovelluksen pitäisi kertoa, että se jää käyttöön.

Käytä testitiedoissa keksittyjä tietoja. Varmista testien jälkeen, ettei testijäseniä jää rekisteriin. Sovelluksessa ei ole poistettujen tietojen palautustoimintoa.

## Vianhaku

| Oire | Tarkistettavaa |
| --- | --- |
| **Ei yhteyttä** tai yhteyden muodostus epäonnistuu | Onko MongoDB käynnissä? Onko osoite oikea sovelluksen suoritusympäristössä? Pääseekö sovellus palvelimen porttiin ja ovatko mahdolliset tunnukset ja oikeudet kunnossa? |
| Android-emulaattori ei tavoita palvelinta | `localhost` tarkoittaa emulaattoria itseään. Kokeile emulaattorin osoitetta `10.0.2.2` kehityskoneelle ja tarkista palvelimen sekä palomuurin asetukset. |
| Windows Machine vaihtuu takaisin Android-laitteeseen | Valitse ensin Windows-framework ja sen jälkeen **Windows Machine**. |
| Tallennuksen tulosta ei voitu varmistaa | Älä oleta, ettei tallennusta tapahtunut. Peruuta lomake, päivitä lista ja tarkista tiedot ennen uutta yritystä. |
| Lista ei päivity tallennuksen jälkeen | Valitse **Päivitä lista** ja tarkista yhteys. Onnistuneesta tallennuksesta ilmoittava viesti ja listan päivitysvirhe voivat näkyä yhtä aikaa. |
| Build ilmoittaa MAUI-paketin ja työkuorman versioiden ristiriidasta | Vertaa projektin MAUI-pakettien, kohdekehysten ja asennetun työkuorman versioita. Visual Studion Installerilla tai `dotnet workload list` -komennolla voi tarkistaa ympäristöä. |

## Esimerkki: uuden jäsenkentän lisääminen

Oletetaan, että rekisteriin halutaan lisätä vapaaehtoinen lisätieto. Muutos koskee useampaa kerrosta:

1. Lisää ominaisuus `Member`-luokkaan. Valitse tietotyyppi ja päätä, onko kenttä pakollinen. Lisää tarvittaessa validointiattribuutit ja suomenkieliset virheviestit.
2. Lisää `Home.razor`-lomakkeeseen `label`, sopiva syötekomponentti ja `@bind-Value`-sidonta. Jos kenttää validoidaan, lisää myös `ValidationMessage`.
3. Lisää kenttä taulukkoon, jos sen pitää näkyä jäsenlistassa. Tarkista myös, miten uusi arvo näkyy vanhoilla dokumenteilla, joissa kenttää ei vielä ole.
4. Lisää kenttä `MemberService.UpdateAsync`-metodin `Builders<Member>.Update`-ketjuun. Pelkkä ominaisuuden lisääminen malliin ei riitä päivittämään sitä olemassa olevaan dokumenttiin, koska päivityksessä luetellaan kentät erikseen. Uuden jäsenen lisäys puolestaan tallentaa `Member`-olion ajurin sarjoituksen kautta.
5. Kokeile lisäys, luku, muokkaus, peruutus ja poisto testitiedoilla sekä varmista vanhojen dokumenttien toiminta. Päivitä dokumentaatio ja tarvittaessa testit.

Jos uusi ominaisuus on muokattava viitetyyppinen olio tai kokoelma, tarkista myös `Member.Copy()`-metodi: nykyinen pinnallinen kopio ei kopioi tällaisen olion sisäistä sisältöä erilliseksi.

## Nykyiset rajaukset

- Jäsenlista haetaan kokonaisuudessaan; sivutusta tai erillistä hakua ei ole.
- Sähköpostin muoto tarkistetaan, mutta sovellus ei varmista osoitteen omistajuutta tai estä useita jäseniä käyttämästä samaa osoitetta.
- Yhteysosoitetta ei säilytetä käynnistysten välillä. Osoite voi sisältää salaisuuksia, joten sitä ei pidä lisätä lähdekoodiin.
- Lomakkeen tarkistukset eivät korvaa tietokannan käyttöoikeuksia, palvelinpuolen validointia tai muita tietoturvatoimia.
- Suora tietokantayhteys, samanaikaisten muokkausten käsittely ja suurten tietomäärien haku ovat jatkokehityksessä erikseen arvioitavia asioita.